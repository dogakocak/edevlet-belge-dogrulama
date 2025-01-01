using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdevletGetToken
{
    using System.Text.RegularExpressions;

    public class ValidationUtils
    {
        public static bool IsValidTurkishIdentityNo(string tcKimlikNo)
        {
            return Regex.IsMatch(tcKimlikNo, @"^[1-9]\d{9}[02468]$");
        }
    }

}
