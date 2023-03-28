using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kursovoi.Classes
{
    public class Hashing
    {
        public static string hashPassword(string password)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] b = Encoding.ASCII.GetBytes(password);
            byte[] hash = sha256.ComputeHash(b);
            StringBuilder sb = new StringBuilder();
            foreach (var item in hash)
            {
                sb.Append(item.ToString("X2"));
            }
            return sb.ToString();
        }



    }
}
