using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace OsumeProject
{
    public static class Hasher
    {
        public static string sha1(string input)
        {
            string output = "";
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            SHA1 hasher = SHA1.Create();
            byte[] computedHash = hasher.ComputeHash(inputBytes);
            foreach (var hashedByte in computedHash) {
                output += hashedByte.ToString("X2");
            }
            if (string.IsNullOrEmpty(output)) return "Error!";
            else return output;
        }
    }
}
