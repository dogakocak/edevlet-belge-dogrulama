using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EdevletGetToken
{
    public class ApiResult
    {
        public bool IsSuccess { get; set; }
        public string NewToken { get; set; }

        public ApiResult(bool isSuccess, string newToken)
        {
            IsSuccess = isSuccess;
            NewToken = newToken;
        }
    }

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
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                var response = await client.GetAsync("/belge-dogrulama");
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var token = TextExtractor.ExtractToken(responseBody);
                return token ?? null;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Bağlantı hatası: {e.Message}");
                return null;
            }
        }

        public async Task<ApiResult> SendFormAsync(string? token, string? barkod)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(barkod))
                return new ApiResult(false, null);

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

                var newToken = TextExtractor.ExtractToken(responseBody);
                var isSuccess = responseBody.Contains("/belge-dogrulama?islem=dogrulama&submit");
                
                return new ApiResult(isSuccess, newToken);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Bağlantı hatası: {e.Message}");
                return new ApiResult(false, null);
            }
        }

        public async Task<ApiResult> SendTcIdentityFormAsync(string tcKimlik, string barkod, string token)
        {
            try
            {
                if (!ValidationUtils.IsValidTurkishIdentityNo(tcKimlik))
                {
                    return new ApiResult(false, null);
                }

                string url = $"/belge-dogrulama?islem=dogrulama&submit";
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(tcKimlik), "ikinciAlan" },
                    { new StringContent(token), "token" },
                    { new StringContent("Devam Et"), "btn" }
                };
                var response = await client.PostAsync(url, formData);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                var newToken = TextExtractor.ExtractToken(responseBody);
                var isSuccess = !responseBody.Contains("fieldError");
                
                return new ApiResult(isSuccess, newToken);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Bağlantı hatası: {e.Message}");
                return new ApiResult(false, null);
            }
        }

        public async Task<ApiResult> SendConfirmationFormAsync(string token)
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

                var newToken = TextExtractor.ExtractToken(responseBody);
                var isSuccess = !responseBody.Contains("/belge-dogrulama?islem=dogrulama");
                
                return new ApiResult(isSuccess, newToken);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Bağlantı hatası: {e.Message}");
                return new ApiResult(false, null);
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