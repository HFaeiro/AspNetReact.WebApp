using ASP.Back.Libraries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TeamManiacs.Core.Models;
using TeamManiacs.Data;

namespace ASP.Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TeamManiacsDbContext _context;
        private static PasswordManagement? passwordManagement;
        private static Emailer? _emailer;
        public UsersController(TeamManiacsDbContext context, IWebHostEnvironment hostEnvironment, Emailer emailer)
        {
             _context = context;
            passwordManagement = new PasswordManagement(hostEnvironment);
            _emailer = emailer;
        }
   
        // GET: api/Users
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Profile>>> GetUserModels()
        {
            var users = await _context.UserModels.ToListAsync();
            return UserModelToProfile(users);
        }



        private ActionResult<IEnumerable<Profile>> UserModelToProfile(IEnumerable<Users> users)
        {
            List<Profile> p = new List<Profile>();
            foreach (Users user in users)
                p.Add(new Profile(user));
            return p;
        }


        //// GET: api/Users/5
        //[HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        //public async Task<ActionResult<Users>> GetUserModel(int id)
        //{
        //    var UserModel = await _context.UserModels.FindAsync(id);

        //    if (UserModel == null)
        //    {
        //        return NotFound();
        //    }

        //    return UserModel;
        //}
        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Login>> GetUserModel(int id)
        {
            if (id == 1)
            {
                var UserModel = await _context.UserModels.FindAsync(id);

                if (UserModel == null)
                {
                    return NotFound();
                }
                Login userLogin = new Login();
                userLogin.Username = UserModel.Username;
                userLogin.Password = passwordManagement.DecryptPassword(UserModel.Password);
                return userLogin;
            }
            else
            {
                return NotFound();
            }
        }
        [HttpGet("ping/")]
        [Authorize]
        public async Task<IActionResult> GetPing()
        {
            string expirationDate = ControllerHelpers.GetExpirationFromToken(User.Identity).ToString();
            return Ok(expirationDate);
        }
        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutUserModel(int id, Users UserModel)
        {

            if (id != UserModel.UserId)
            {
                return BadRequest();
            }
            var userModel = await _context.UserModels.FindAsync(id);
            if (userModel == null)
            {
                return NotFound();
            }


            if (userModel.Username != UserModel.Username)
            {

                var tstUser = _context.UserModels.FirstOrDefault(x =>
                                                       x.Username.ToLower() == UserModel.Username.ToLower());
                if (tstUser == null)
                {
                    _context.Entry(userModel.replace(UserModel)).State = EntityState.Modified;
                }
                else
                    return BadRequest($"Username Already Taken");
            }
            else
                _context.Entry(userModel.replace(UserModel)).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok($"Edited Successfully");

        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Users>> PostUserModel(Login newUserModel)
        {
            if (_emailer == null)
            {
                return BadRequest();
            }

            var userModel = _context.UserModels.FirstOrDefault(x =>
                x.Username == newUserModel.Username || x.Email.ToLower() == newUserModel.Email.ToLower()
           );
            if (userModel == null)
            {
                if (passwordManagement != null)
                {
                    byte[] encryptedPassword = passwordManagement.EncryptPassword(newUserModel.Password);
                    if (encryptedPassword != null)
                    {
                        Users newUser = new Users(newUserModel.Email, encryptedPassword, newUserModel.Username);

                        if (newUser == null)
                        {
                            return BadRequest();
                        }

                        newUser.Status = TeamManiacs.Core.Enums.UserStatus.inactive;
                        newUser = (await _context.UserModels.AddAsync(newUser)).Entity;
                        _context.SaveChanges();

                        AuthCode authCode = ControllerHelpers.GenerateAuthCode(newUser.UserId);

                        if(authCode == null)
                        {
                            return BadRequest();
                        }
                        await _context.AuthCodes.AddAsync(authCode);

                        await _context.SaveChangesAsync();
                        newUser.Password = null;

                        _emailer.SendTwoFactorEmail(newUser.Email, authCode.Code);



                        return Ok(newUser);
                    }
                }
            }
            return BadRequest();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUserModel(int id)
        {

            if (id == 1)
                return BadRequest();
            var tmpUserModel = await _context.UserModels.FindAsync(id);
            if (tmpUserModel == null)
            {
                return NotFound();
            }

            _context.UserModels.Remove(tmpUserModel);
            await _context.SaveChangesAsync();

            return Ok($"Deleted User With Id of: " + id);
        }

        private async Task<bool> UserModelExists(int id)
        {            
            return await _context.UserModels.AnyAsync(e => e.UserId == id);
        }
    }
}
