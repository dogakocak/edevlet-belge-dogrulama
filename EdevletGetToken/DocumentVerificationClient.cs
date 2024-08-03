using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdevletGetToken
{
    public class DocumentVerificationClient
    {
        private readonly string endpoint = "https://www.turkiye.gov.tr";
        private readonly HttpClient client;

        public DocumentVerificationClient()
        {
            client = new HttpClient
            {
                BaseAddress = new Uri(endpoint)
            };
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
        }

        public async Task<string> GetTokenAsync()
        {
            try
            {
                var response = await client.GetAsync("/belge-dogrulama");
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var token = TextExtractor.ExtractToken(responseBody);

                return !string.IsNullOrEmpty(token) ? token : throw new Exception("Token alınamadı");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Bağlantı hatası: {e.Message}");
                return null;
            }
        }

        public async Task<bool> SendFormAsync(string token, string barkod)
        {
            try
            {
                string url = "/belge-dogrulama?submit";
                var formData = new MultipartFormDataContent
            {
                { new StringContent(barkod), "sorgulananBarkod" },
                { new StringContent(token), "token" }
            };
                var response = await client.PostAsync(url, formData);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                return responseBody.Contains("/belge-dogrulama?islem=dogrulama&submit");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Bağlantı hatası: {e.Message}");
                return false;
            }
        }

        public async Task<bool> SendTcKimlikFormAsync(string tcKimlik, string barkod, string token)
        {
            try
            {
                if (!ValidationUtils.IsValidTcKimlikNo(tcKimlik))
                {
                    return false;
                }

                string url = $"/belge-dogrulama?islem=dogrulama&submit&barkod={barkod}";
                var formData = new MultipartFormDataContent
            {
                { new StringContent(tcKimlik), "ikinciAlan" },
                { new StringContent(token), "token" }
            };
                var response = await client.PostAsync(url, formData);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                return !responseBody.Contains("fieldError");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Bağlantı hatası: {e.Message}");
                return false;
            }
        }

        public async Task<bool> SendOnayFormAsync(string token)
        {
            try
            {

                string url = $"/belge-dogrulama?islem=onay&submit";
                var formData = new MultipartFormDataContent
            {
                { new StringContent("1"), "chkOnay" },
                { new StringContent(token), "token" }
            };
                var response = await client.PostAsync(url, formData);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                return responseBody.Contains("/belge-dogrulama?islem=dogrulama") ? false : true;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Bağlantı hatası: {e.Message}");
                return false;
            }
        }

        public async Task<byte[]> GetPdfAsync(string token)
        {
            try
            {
                string url = $"/belge-dogrulama?belge=goster&goster=1&display=display";
                byte[] pdfBytes = await client.GetByteArrayAsync(url);

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    Console.WriteLine("Öğrenci belgesi bulunamadı! Girdiğiniz bilgileri kontrol ediniz.");
                    return null;
                }

                return pdfBytes;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Connection error: {e.Message}");
                return null;
            }
        }
    }
}
