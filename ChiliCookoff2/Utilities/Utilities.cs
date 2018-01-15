using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChiliCookoff2.Utilities
{
    public class Utilities
    {
        public static string GeneratePartyCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}