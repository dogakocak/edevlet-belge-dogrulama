using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdevletGetToken
{
    using System.Text.RegularExpressions;

    public class TextExtractor
    {
        public static string ExtractNameSurname(string text)
        {
            // Adı / Soyadı kısmı için regex
            return ExtractInformation(text, @"Adı / Soyadı\s*(.*)\s*Anne Adı");
        }

        public static string ExtractFaculty(string text)
        {
            // Fakülte bilgisi için regex
            return ExtractInformation(text, @"ÜNİVERSİTESİ/(.*?FAKÜLTESİ)");
        }

        public static string ExtractDepartment(string text)
        {
            // Bölüm bilgisi için regex
            return ExtractInformation(text, @"Program\s.*?\/.*?\/([^\/]*?BÖLÜMÜ)");
        }

        public static string ExtractUniversity(string text)
        {
            // Üniversite bilgisi için regex
            return ExtractInformation(text, @"Program\s(.*?ÜNİVERSİTESİ)");
        }

        public static string ExtractInformation(string text, string pattern)
        {
            var regex = new Regex(pattern);
            var match = regex.Match(text);

            if (match.Success)
            {
                var result = match.Groups[1].Value;
                // Tüm istenmeyen karakterleri temizle (\n, \t, vb.)
                return CleanString(result);
            }

            return null;
        }

        public static string ExtractToken(string html)
        {
            var match = Regex.Match(html, @"data-token=""(.+?)""");
            return match.Success ? match.Groups[1].Value : null;
        }

        public static string CleanString(string text)
        {
            var textReplaced = text.Replace("\n", " ").Replace("\r", " ").Replace("\t", " ").Trim();
            return textReplaced;
        }
    }

}
