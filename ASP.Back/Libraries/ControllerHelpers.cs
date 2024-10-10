using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Claims;
using System.Security.Principal;
using TeamManiacs.Core.Models;
using TeamManiacs.Data;

namespace ASP.Back.Libraries
{
    internal static class ControllerHelpers
    {
        private static Random random = new Random();
        static public int? GetUserIdFromToken(IIdentity? identity)
        {
            var claimsIdentity = identity as ClaimsIdentity;


            string? claimId = null;
            try
            {
                claimId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"\t\t{nameof(GetUserIdFromToken)} - {claimId}");
                if (claimId != null)
                    return int.Parse(claimId);
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
        static public string GetUniqueFileName(string fileName, int userIdHash)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                + "_"
                + Guid.NewGuid().ToString().Substring(0, 4)
                + userIdHash.ToString()
                + Path.GetExtension(fileName);
        }
        static public string ReplaceKeyInString(string orig, string key, string value)
        {
            return orig.Replace("{{"+key+"}}", value);
        }
        static public string GenerateRandomString(int length)
        {
            const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
            return new string(Enumerable.Repeat(chars, length)
               .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        static public AuthCode GenerateAuthCode(int uID)
        {
            string code = ControllerHelpers.GenerateRandomString(16);

            AuthCode authCode = new AuthCode();
            authCode.Uid = uID;
            authCode.Code = code;
            authCode.CreatedDate = DateTime.Now;

            return authCode;
        }
    }


}