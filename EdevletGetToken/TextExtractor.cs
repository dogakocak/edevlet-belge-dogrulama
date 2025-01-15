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
    // Üniversite ve Fakülte arasında "/" olmayabilir, boşluk olabilir
    var pattern1 = @"ÜNİVERSİTESİ\s+(.*?FAKÜLTESİ)";
    
    // Veya direkt "Fakültesi" kelimesini içeren kısmı alabiliriz
    var pattern2 = @"(.*?FAKÜLTESİ)";
    
    // İlk pattern'i dene
    var result = ExtractInformation(text, pattern1);
    
    // Bulunamazsa ikinci pattern'i dene
    if (string.IsNullOrEmpty(result))
    {
        result = ExtractInformation(text, pattern2);
    }
    
    return result;
}

       public static string ExtractDepartment(string text)
{
    // Birkaç farklı pattern deneyelim
    var patterns = new[]
    {
        @"Program\s.*?\/.*?\/([^\/]*?BÖLÜMÜ)",         // Mevcut pattern
        @"BÖLÜMÜ\s*:\s*(.*?(?:BÖLÜMÜ|$))",            // "BÖLÜMÜ:" formatı için
        @"FAKÜLTESİ\s*(.*?BÖLÜMÜ)",                   // Fakülteden sonra gelen bölüm
        @"(?:Program|Bölüm)\s*:?\s*(.*?BÖLÜMÜ)",      // Program/Bölüm ile başlayan
        @"([^\/\n]*?BÖLÜMÜ)"                          // Genel BÖLÜMÜ içeren kısım
    };

    foreach (var pattern in patterns)
    {
        var result = ExtractInformation(text, pattern)?.Trim();
        if (!string.IsNullOrEmpty(result))
        {
            return result;
        }
    }

    return null;
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
