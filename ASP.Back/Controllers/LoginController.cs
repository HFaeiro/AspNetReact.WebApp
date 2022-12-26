
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using TeamManiacs.Core.Models;
using TeamManiacs.Data;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ASP.Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContext;
        private readonly TeamManiacsDbContext _context;


        public LoginController(TeamManiacsDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;

            
        }


        // POST: api/UserModel
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public ActionResult PostUserModel(Login login)
        {
            var user = Authenticate(login);
            if(user != null)
            {
                var token = GenToken(user);
                return Ok(token);
            }    

            return BadRequest();
        }
        private string GenToken(Users user)
        {
            
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier ,user.Username),
                new Claim(ClaimTypes.Role, user.Privileges.ToString())
            };
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(15),
                signingCredentials: credentials);
            string sToken = new JwtSecurityTokenHandler().WriteToken(token);
            return sToken;


        }


        private Users? Authenticate(Login login)
        {
            var currentUser = _context.UserModels.
                                FirstOrDefault(x => 
                                x.Username.ToLower() == login.Username.ToLower()
                             && x.Password == login.Password);
            return currentUser;

        }

    }

  
}
