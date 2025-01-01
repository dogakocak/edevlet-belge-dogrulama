class Program
{
    private static readonly string endpoint = "https://www.turkiye.gov.tr";
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        var dogrulama = new DocumentVerification();

        Console.Write("Barkod Numarası:");
        string barkod = Console.ReadLine(); // Kullanmak istediğiniz barkod numarasını girin

        Console.Write("Tc:");
        string tcKimlik = Console.ReadLine(); // Kullanmak istediğiniz TC Kimlik Numarasını girin

        var pdfText = await dogrulama.GetPdfTextAsync(barkod, tcKimlik);

        if (pdfText == null)
        {
            Console.WriteLine("İşlem başarısız.");
            Console.ReadLine();
            return;
        }

        dogrulama.WriteInformations(pdfText);

        Console.ReadLine();
    }

}
