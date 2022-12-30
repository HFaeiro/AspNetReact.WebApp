using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using TeamManiacs.Core;
using TeamManiacs.Core.Models;
using TeamManiacs.Data;

namespace ASP.Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TeamManiacsDbContext _context;




        public UsersController(TeamManiacsDbContext context )
        {

             _context = context;
           


        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Users>>> GetUserModels()
        {
            return await _context.UserModels.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Users>> GetUserModel(int id)
        {
            var UserModel = await _context.UserModels.FindAsync(id);

            if (UserModel == null)
            {
                return NotFound();
            }

            return UserModel;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutUserModel(int id,Users UserModel)
        {

            if (id != UserModel.UserId)
            {
                return BadRequest();
            }
            
            _context.Entry(UserModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserModelExists(id))
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
        public async Task<ActionResult<Users>> PostUserModel(Users UserModel)
        {

            var userModel = _context.UserModels.FirstOrDefault(x =>
                                                    x.Username.ToLower() == UserModel.Username.ToLower());
            if (userModel == null)
            {
                _context.UserModels.Add(UserModel);
                await _context.SaveChangesAsync();

                return Ok(UserModel);
            }
            else
                return BadRequest();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUserModel(int id)
        {
            
           
            var tmpUserModel = await _context.UserModels.FindAsync(id);
            if (tmpUserModel == null)
            {
                return NotFound();
            }

            _context.UserModels.Remove(tmpUserModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserModelExists(int id)
        {
            return _context.UserModels.Any(e => e.UserId == id);
        }
    }
}
