using TeamManiacs.Core;
using TeamManiacs.Core.Models;

namespace TeamManiacs.Data
{
    public interface IManiacsData
    {

        IEnumerable<Login> GetProfileByUsername(string username);
        Login GetProfileById(int id);
        Login UpdateProfileData(Login login);
        Login AddProfile(Login login);
        Login DeleteProfile(int id);
        int Commit();
    }
}
