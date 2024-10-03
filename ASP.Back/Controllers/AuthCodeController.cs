using ASP.Back.Libraries;
using Microsoft.AspNetCore.Mvc;
using System;
using TeamManiacs.Core.Models;
using TeamManiacs.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ASP.Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthCodeController : ControllerBase
    {
        private readonly TeamManiacsDbContext _context;
        public AuthCodeController(TeamManiacsDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
        }


        //// GET: api/<ValuesController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<ValuesController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "";
        //}

        // POST api/<ValuesController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AuthCode usersCode)
        {
            //Post Auth Code to server. If code is valid and user is inactive, user will be activated. 

            AuthCode? ourCode = await _context.AuthCodes.FindAsync(usersCode.Uid);
            if (ourCode == null)
            {
                return NotFound();
            }

            if(ourCode.Code != usersCode.Code)
            {
                return BadRequest();
            }
            Delete(ourCode);

            Users? users = await _context.UserModels.FindAsync(usersCode.Uid);

            if (users == null)
            {
                return NotFound();
            }
            if (users.Status != TeamManiacs.Core.Enums.UserStatus.inactive)
            {
                return BadRequest();
            }
            users.Status = TeamManiacs.Core.Enums.UserStatus.active;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT api/<ValuesController>/[AuthCode]
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] AuthCode usersCode)
        {
            //Generate a new code, if user is not inactive, code will not be generated. 
            Users? user = await _context.UserModels.FindAsync(usersCode.Uid);

            if(user == null)
            {
                return NotFound();
            }    

            if(user.Status != TeamManiacs.Core.Enums.UserStatus.inactive)
            {
                return BadRequest();
            }

            _context.AuthCodes.Remove(usersCode);


            //Generate code
            AuthCode authCode = ControllerHelpers.GenerateAuthCode(user.UserId);

            if (authCode == null)
            {
                return BadRequest();
            }
            
            _context.AuthCodes.Add(authCode);

            await _context.SaveChangesAsync();


            return Ok();
        }

        private async void Delete(AuthCode usersCode)
        {    
            _context.AuthCodes.Remove(usersCode);

            await _context.SaveChangesAsync();
        }
    }
}
