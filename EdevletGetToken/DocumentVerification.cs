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
        var token = await client.GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;

        var isFormSent = await client.SendFormAsync(token, barkod);
        if (!isFormSent) return null;

        var isTcKimlikFormSent = await client.SendTcIdentityFormAsync(tcKimlik, barkod, token);
        if (!isTcKimlikFormSent) return null;

        var isOnayFormSent = await client.SendConfirmationFormAsync(token);
        if (!isOnayFormSent) return null;

        var pdfBytes = await client.GetPdfAsync(token);
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
