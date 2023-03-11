using Microsoft.EntityFrameworkCore;
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
                if (claimId != null)
                    return Int32.Parse(claimId);
            }
            catch (FormatException)
            {
                return null;
            }

            return null;
        }
        static public DateTime? GetExpirationFromToken(IIdentity? identity)
    {
        var claimsPrinciple = identity as ClaimsPrincipal;
        Claim? exp = claimsPrinciple?.FindFirst("exp");
        if (exp != null)
        {
            var expiration = new DateTime(long.Parse(exp.Value));
            return expiration;
        }
        else return null;
    }

        static public async Task<Users?> GetUserById(int id, TeamManiacsDbContext context)
        {
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