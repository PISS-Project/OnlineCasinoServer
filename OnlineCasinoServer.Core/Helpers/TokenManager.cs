using OnlineCasinoServer.Data.Models;
using System;
using System.Linq;

namespace OnlineCasinoServer.Core.Helpers
{
    public static class TokenManager
    {
        public static string GenerateToken()
        {
            //create token. timestamp + GUID
            byte[] time = BitConverter.GetBytes(DateTime.Now.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            string token = Convert.ToBase64String(time.Concat(key).ToArray());

            return token;
        }

        public static bool IsTokenPresent(string token)
        {
            if (token == null)
                return false;

            using (var db = new OnlineCasinoDb())
            {
                if (db.Logins.FirstOrDefault(l => object.Equals(l.Token, token)) == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
