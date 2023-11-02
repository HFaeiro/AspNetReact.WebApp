using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using System.Security.Principal;
using TeamManiacs.Core.Models;
using TeamManiacs.Data;

namespace ASP.Back.Controllers
{
    internal static class ControllerHelpers
    {
        static public int? GetUserIdFromToken(IIdentity? identity)
        {
            var claimsIdentity = identity as ClaimsIdentity;

           
            string? claimId = null;
            try
            {
                claimId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"\t\t{nameof(GetUserIdFromToken)} - {claimId}");
                if (claimId != null)
                    return Int32.Parse(claimId);
            }
            catch (FormatException)
            {
                return null;
            }

            return null;
        }
        static public DateTime GetExpirationFromToken(IIdentity? identity)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            
            
            //or as DateTime:
            Claim? exp = claimsIdentity?.FindFirst("exp");
            if (exp != null)
            {
                var expiration = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp.Value));
                return expiration.UtcDateTime;
            }
            else return DateTime.UtcNow;
        }

        static public async Task<Users?> GetUserById(int id, TeamManiacsDbContext context)
        {

            Console.WriteLine($"\t\t{nameof(GetUserById)} - {id}");
            return await context.UserModels.FindAsync(id);
        }

        static public Users? GetUserByUsername(string username, TeamManiacsDbContext context)
        {
            return context.UserModels.FirstOrDefault(x =>
                                                   x.Username.ToLower() == username.ToLower());
        }
        static public string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                + "_"
                + Guid.NewGuid().ToString().Substring(0, 4)
                + Path.GetExtension(fileName);
        }
    }
   

}