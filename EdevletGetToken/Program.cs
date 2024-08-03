class Program
{
    private static readonly string endpoint = "https://www.turkiye.gov.tr";
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        var dogrulama = new DocumentVerification();
        var token = await dogrulama.GetTokenAsync();
        var tcResponse = false;

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Token alınamadı.");
            Console.ReadLine();
            return;
        }

        Console.Write("Barkod Numarası:");
        string barkod = Console.ReadLine(); // Kullanmak istediğiniz barkod numarasını girin
        var response = await dogrulama.SendFormAsync(token, barkod);

        if (!response)
        {
            Console.WriteLine("Girilen barkod numarası e-Devlet Kapısında tanımlı değildir.");
            Console.ReadLine();
            return;
        }

        Console.Write("Tc:");
        string tcKimlik = Console.ReadLine(); // Kullanmak istediğiniz TC Kimlik Numarasını girin
        tcResponse = await dogrulama.SendTcKimlikFormAsync(tcKimlik, barkod, token);

        if (!tcResponse)
        {
            Console.WriteLine("TC Kimlik numarası doğrulaması başarısız.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("TC Kimlik numarası doğrulaması başarılı.");

        var finalResponse = await dogrulama.SendOnayFormAsync(token);
        if (!finalResponse)
        {
            Console.WriteLine("Öğrenci belgesi bulunamadı");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Öğrenci belgesi bulundu.");
        await dogrulama.GetPdfUrl(token);
        Console.ReadLine();
    }
}
