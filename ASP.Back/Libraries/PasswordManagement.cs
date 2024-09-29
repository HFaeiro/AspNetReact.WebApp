using Microsoft.AspNetCore.Mvc;

namespace ASP.Back.Libraries
{
    public class PasswordManagement
    {
       private Crypto crypto;

        public PasswordManagement(IWebHostEnvironment hostEnvironment) {

            crypto = new Crypto(hostEnvironment, Crypto.ObjectType.Password);

        }
        public bool ValidatePassword(string receivedPassword, Byte[] storedPassword)
        {
           
            if(string.IsNullOrEmpty(receivedPassword) || storedPassword == null)
            {
                return false;
            }
            return crypto.DecryptToString(storedPassword) == receivedPassword;
        }
        public byte[] EncryptPassword(string receivedPassword)
        {
            return crypto.EncryptToBytes(receivedPassword);
        }
        public string DecryptPassword(Byte[] storedPassword)
        {
            return crypto.DecryptToString(storedPassword);
        }
    }
}
