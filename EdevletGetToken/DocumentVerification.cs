using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text.RegularExpressions;

public class DocumentVerification
{
    private readonly string endpoint = "https://www.turkiye.gov.tr";
    private readonly HttpClient client;

    public DocumentVerification()
    {
        client = new HttpClient
        {
            BaseAddress = new Uri(endpoint),
            Timeout = TimeSpan.FromSeconds(1)
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
            var token = ExtractToken(responseBody);

            return !string.IsNullOrEmpty(token) ? token : throw new Exception("Token alınamadı");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Bağlantı hatası: {e.Message}");
            return null;
        }
    }

    public async Task<string> SendFormAsync(string token, string barkod)
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

            // Yanıtı kontrol et
            if (responseBody.Contains("/belge-dogrulama?islem=dogrulama&submit"))
            {
                return "TC Girme aşamasına geçildi.";
            }
            else
            {
                return "Girilen barkod numarası e-Devlet Kapısında tanımlı değildir.";
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Bağlantı hatası: {e.Message}");
            return null;
        }
    }

    public async Task<string> SendTcKimlikFormAsync(string tcKimlik, string barkod, string token)
    {
        try
        {
            string url = $"/belge-dogrulama?islem=dogrulama&submit&barkod={barkod}";
            var formData = new MultipartFormDataContent
            {
                { new StringContent(tcKimlik), "ikinciAlan" },
                { new StringContent(token), "token" }
            };
            var response = await client.PostAsync(url, formData);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            // Yanıtı kontrol et
            return responseBody.Contains(tcKimlik) ? "TC Kimlik numarası doğrulaması basarili." : "TC Kimlik numarası doğrulaması başarısız.";
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Bağlantı hatası: {e.Message}");
            return null;
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

    public async Task<bool> GetPdfUrl(string token)
    {
        try
        {
            string url = $"/belge-dogrulama?belge=goster&goster=1&display=display";
            byte[] pdfBytes = await client.GetByteArrayAsync(url);

            // PDF içeriğini stringe çevir
            string pdfText = ExtractTextFromPdf(pdfBytes);

            // TC kimlik bilgisini regex ile bul ve ekrana yazdır
            string tcKimlik = ExtractTcKimlikNo(pdfText);
            if (!string.IsNullOrEmpty(tcKimlik))
            {
                Console.WriteLine($"TC Kimlik No: {tcKimlik}");
            }
            else
            {
                Console.WriteLine("TC Kimlik No bulunamadı.");
            }

            string adiSoyadi = ExtractAdiSoyadi(pdfText);
            if (!string.IsNullOrEmpty(adiSoyadi))
            {
                Console.WriteLine($"Adı / Soyadı: {adiSoyadi}");
            }
            else
            {
                Console.WriteLine("Adı / Soyadı bulunamadı.");
            }

            return pdfText.Contains("/belge-dogrulama?islem=dogrulama") ? false : true;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Bağlantı hatası: {e.Message}");
            return false;
        }
    }

    private string ExtractTextFromPdf(byte[] pdfBytes)
    {
        using (var pdfStream = new System.IO.MemoryStream(pdfBytes))
        {
            using (var pdfReader = new PdfReader(pdfStream))
            {
                using (var pdfDocument = new PdfDocument(pdfReader))
                {
                    var strategy = new SimpleTextExtractionStrategy();
                    var pdfText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(1), strategy);
                    return pdfText;
                }
            }
        }
    }

    private string ExtractTcKimlikNo(string text)
    {
        // TC kimlik no için basit bir regex örneği: 11 haneli sadece rakamlardan oluşan
        var regex = new Regex(@"\b\d{11}\b");
        var match = regex.Match(text);
        return match.Success ? match.Value : null;
    }

    private string ExtractAdiSoyadi(string text)
    {
        // Adı / Soyadı kısmı için regex
        var regex = new Regex(@"Adı / Soyadı\s*(.*)\s*Anne Adı");
        var match = regex.Match(text);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }



    private string ExtractToken(string html)
    {
        var match = Regex.Match(html, @"data-token=""(.+?)""");
        return match.Success ? match.Groups[1].Value : null;
    }
}
