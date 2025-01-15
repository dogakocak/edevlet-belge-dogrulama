using EdevletGetToken;

public class DocumentVerification
{
    private readonly DocumentVerificationClient client;

    public DocumentVerification()
    {
        client = new DocumentVerificationClient();
    }

    public async Task<string> GetPdfTextAsync(string barkod, string tcKimlik)
    {
        // İlk token alınır
        var token = await client.GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;

        // Form gönderilir ve yeni token alınır
        var formResult = await client.SendFormAsync(token, barkod);
        if (!formResult.IsSuccess || string.IsNullOrEmpty(formResult.NewToken)) return null;

        // TC Kimlik formu gönderilir ve yeni token alınır
        var tcKimlikResult = await client.SendTcIdentityFormAsync(tcKimlik, barkod, formResult.NewToken);
        if (!tcKimlikResult.IsSuccess || string.IsNullOrEmpty(tcKimlikResult.NewToken)) return null;

        // Onay formu gönderilir ve yeni token alınır
        var onayResult = await client.SendConfirmationFormAsync(tcKimlikResult.NewToken);
        if (!onayResult.IsSuccess || string.IsNullOrEmpty(onayResult.NewToken)) return null;

        // Son token ile PDF alınır
        var pdfBytes = await client.GetPdfAsync(onayResult.NewToken);
        if (pdfBytes == null) return null;

        var pdfText = PdfProcessor.ExtractTextFromPdf(pdfBytes);

        // PDF içeriğini doğrula
        bool isValid = PdfProcessor.ValidatePdf(pdfText);

        if (!isValid)
        {
            Console.WriteLine("PDF doğrulaması başarısız.");
            return null;
        }

        return pdfText;
    }

    public void WriteInformations(string pdfText)
    {
        // Bu metod aynı kalabilir çünkü token işlemleriyle ilgisi yok
        string adiSoyadi = TextExtractor.ExtractNameSurname(pdfText);
        string university = TextExtractor.ExtractUniversity(pdfText);
        string faculty = TextExtractor.ExtractFaculty(pdfText);
        string department = TextExtractor.ExtractDepartment(pdfText);

        if (string.IsNullOrEmpty(adiSoyadi))
        {
            Console.WriteLine("Adı / Soyadı bulunamadı.");
            return;
        }

        if (string.IsNullOrEmpty(university))
        {
            Console.WriteLine("Üniversite bilgisi bulunamadı.");
            return;
        }

        if (string.IsNullOrEmpty(faculty))
        {
            Console.WriteLine("Fakülte bilgisi bulunamadı.");
            return;
        }
        if (string.IsNullOrEmpty(department))
        {
            Console.WriteLine("Bölüm bilgisi bulunamadı.");
            return;
        }

        Console.WriteLine($"Adı / Soyadı: {adiSoyadi}");
        Console.WriteLine($"Üniversite: {university}");
        Console.WriteLine($"Fakülte: {faculty}");
        Console.WriteLine($"Bölüm: {department}");
    }
}