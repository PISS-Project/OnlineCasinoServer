using OnlineCasinoServer.Data.Entities;
using System;
using System.Security.Cryptography;
using System.Text;

namespace OnlineCasinoServer.Core.Helpers
{
    public static class CryptographicManager
    {
        public static string GenerateSalt(int size)
        {
            var rngCsp = new RNGCryptoServiceProvider();
            var buff = new byte[size];
            rngCsp.GetBytes(buff);
            return Convert.ToBase64String(buff);
        }

        public static string GenerateSHA256Hash(string input, string salt)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input + salt);
            var sha256Hasher = new SHA256Managed();

            var hash = sha256Hasher.ComputeHash(bytes);

            return Convert.ToBase64String(hash);

        }

        public static void SetNewUserInfo(User user, string username, string password)
        {
            string salt = GenerateSalt(32);
            string hashedPassword = GenerateSHA256Hash(password, salt);

            user.Username = username;
            user.Password = hashedPassword;
            user.Salt = salt;
        }
    }
}
