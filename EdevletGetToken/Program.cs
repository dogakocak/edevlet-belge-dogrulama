class Program
{
    private static readonly string endpoint = "https://www.turkiye.gov.tr";
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        var dogrulama = new DocumentVerification();
        var token = await dogrulama.GetTokenAsync();
        string tcResponse = String.Empty;

        if (!string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Barkod Numarası:");
            string barkod = Console.ReadLine(); // Kullanmak istediğiniz barkod numarasını girin
            string response = await dogrulama.SendFormAsync(token, barkod);
            Console.WriteLine(response);

            if (response == "TC Girme aşamasına geçildi.")
            {
                Console.WriteLine("Tc:");
                string tcKimlik = Console.ReadLine(); // Kullanmak istediğiniz TC Kimlik Numarasını girin
                tcResponse = await dogrulama.SendTcKimlikFormAsync(tcKimlik, barkod, token);
                Console.WriteLine(tcResponse);
            }

            if(tcResponse == "TC Kimlik numarası doğrulaması basarili.")
            {
                var finalResponse = await dogrulama.SendOnayFormAsync(token);
                if(finalResponse)
                {
                    Console.WriteLine("pdf bulundu");
                    await dogrulama.GetPdfUrl(token);
                }
                else
                {
                    Console.WriteLine("pdf bulunamadi");
                }
            }
            
        }
        else
        {
            Console.WriteLine("Token alınamadı.");
        }
    }
}
