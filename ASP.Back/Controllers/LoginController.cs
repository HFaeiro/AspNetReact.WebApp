
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TeamManiacs.Core.Models;
using TeamManiacs.Data;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using ASP.Back.Libraries;

namespace ASP.Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContext;
        private readonly TeamManiacsDbContext _context;

        private static PasswordManagement? passwordManagement;

        public LoginController(TeamManiacsDbContext context, IConfiguration config, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _config = config;
            passwordManagement = new PasswordManagement(hostEnvironment);
        }


        // POST: api/UserModel
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public ActionResult PostUserModel(Login login)
        {

            var user = Authenticate(login);
            if(user != null)
            {
                if(user.Status == TeamManiacs.Core.Enums.UserStatus.inactive) {

                    return StatusCode(201, new Users{ UserId = user.UserId});
                }
                var token = GenToken(user);
                Profile profile = new Profile(user.UserId, user.Username, token, user.Privileges, user.Videos);
                if (profile != null)
                {
                    return Ok(profile);
                }
            }    

            return BadRequest(login);
        }
        private string GenToken(Users user)
        {
            
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier ,user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Privileges.ToString())
            };
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTimeOffset.UtcNow.AddSeconds(30).DateTime,
                signingCredentials: credentials);
            string sToken = new JwtSecurityTokenHandler().WriteToken(token);
            return sToken;


        }


        private Users? Authenticate(Login login)
        {
            if (passwordManagement == null)
            {
                return null;
            }
            byte[] encryptedPassword = passwordManagement.EncryptPassword(login.Password);
            if(encryptedPassword == null)
            {
                return null;
            }            

            var currentUser = _context.UserModels.
                                FirstOrDefault(x => 
                                x.Username.ToLower() == login.Username.ToLower() || x.Email.ToLower() == login.Email.ToLower()
                             && x.Password == encryptedPassword);
            return currentUser;

        }

    }

  
}
