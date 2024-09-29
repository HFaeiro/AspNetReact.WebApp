using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Hosting;
using Mono.Unix.Native;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

namespace ASP.Back.Libraries
{
    public class Crypto
    {
        private readonly byte[]? key = null;
        private readonly byte[]? iv = null;
        private readonly string rootPath = "";
        private readonly string vaultRootPath = "";
        private readonly string currentVaultPath = "";
        public enum ObjectType
        {
            Password = 0,
        }

        public Crypto(IWebHostEnvironment hostEnvironment, ObjectType objType)
        {
            rootPath = hostEnvironment.WebRootPath;
            vaultRootPath = Path.Combine(rootPath, "vault");
            currentVaultPath = "";
            switch (objType)
            {
                case ObjectType.Password:
                    {
                        currentVaultPath = Path.Combine(vaultRootPath, objType.GetTypeCode().ToString());
                        break;
                    }
                default: { return; }
            }

            // we need to either load an AES key or Create one. 
            if (!Directory.Exists(currentVaultPath))
            {
                Directory.CreateDirectory(currentVaultPath);
                FileStream keyStream = File.Create(Path.Combine(currentVaultPath, "key"));
                FileStream ivStream = File.Create(Path.Combine(currentVaultPath, "iv"));
                using (Aes aes = Aes.Create())
                {
                    keyStream.Write(aes.Key, 0, aes.Key.Length);
                    keyStream.Close();
                    ivStream.Write(aes.IV, 0, aes.IV.Length);
                    ivStream.Close();
                }
            }
            else
            {
                FileStream keyStream = File.OpenRead(Path.Combine(currentVaultPath, "key"));
                FileStream ivStream = File.OpenRead(Path.Combine(currentVaultPath, "iv"));
                if (!(keyStream.Length > 0) || !(ivStream.Length > 0))
                {
                    return;
                }
                this.key = new byte[keyStream.Length];
                this.iv = new byte[ivStream.Length];
                keyStream.Read(key);
                keyStream.Close();
                ivStream.Read(iv);
                ivStream.Close();
            }
        }

        public byte[]? EncryptToBytes(string dataToEncrypt)
        {
            byte[]? encryptedData = null;
            if (this.key == null || this.iv == null || string.IsNullOrEmpty(dataToEncrypt))
            {
                return null;
            }
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = this.key;
                    aes.IV = this.iv;

                    ICryptoTransform encryptor = aes.CreateEncryptor();
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                //Write all data to the stream.
                                swEncrypt.Write(dataToEncrypt);
                            }
                            encryptedData = msEncrypt.ToArray();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
            }
            return encryptedData;
        }

        public string DecryptToString(byte[] dataToDecrypt)
        {
            string decryptedData = "";

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = this.key;
                    aes.IV = this.iv;

                    ICryptoTransform decryptor = aes.CreateDecryptor();
                    using (MemoryStream msDecrypt = new MemoryStream(dataToDecrypt))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader swDecrypt = new StreamReader(csDecrypt))
                            {
                                //Read all data to from stream.
                                decryptedData = swDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace + "\n\n");
            }
            return decryptedData;
        }

    }
}
