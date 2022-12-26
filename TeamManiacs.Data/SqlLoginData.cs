
using TeamManiacs.Core;
using TeamManiacs.Core.Models;

namespace TeamManiacs.Data
{
    public class SqlLoginData : IManiacsData
    {
        private readonly TeamManiacsDbContext db;

        public SqlLoginData(TeamManiacsDbContext db)
        {
            this.db = db;
        }

        public Login AddProfile(Login login)
        {
            db.Add(login);
            return login;
        }

        public int Commit()
        {
            return db.SaveChanges();
        }

        public Login DeleteProfile(int id)
        {
            Login login = GetProfileById(id);
            if (login != null)
            {
                db.Remove(login);
            }
            return login;
        }

        public Login GetProfileById(int id)
        {
            return db.Logins.Find(id);
        }

        public IEnumerable<Login> GetProfileByUsername(string username)
        {
            return from u in db.Logins
                   where string.IsNullOrEmpty(username) || u.Username.StartsWith(username)
                   orderby u.Username
                   select u;
        }

        public Login UpdateProfileData(Login login)
        {
            throw new NotImplementedException();
        }
    }
}
