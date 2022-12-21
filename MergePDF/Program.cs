using PassportPDF.Api;
using PassportPDF.Client;
using PassportPDF.Model;

namespace PdfMerger
{

    public class PdfMerger
    {
        static async Task Main(string[] args)
        {
            GlobalConfiguration.ApiKey = "YOUR-PASSPORT-CODE";

            PassportManagerApi apiManager = new();
            PassportPDFPassport passportData = await apiManager.PassportManagerGetPassportInfoAsync(GlobalConfiguration.ApiKey);

            if (passportData == null)
            {
                throw new ApiException("The Passport number given is invalid, please set a valid passport number and try again.");
            }
            else if (passportData.IsActive is false)
            {
                throw new ApiException("The Passport number given not active, please go to your PassportPDF dashboard and active your plan.");
            }

            string uri1 = "https://passportpdfapi.com/test/invoice_with_barcode.pdf";
            string uri2 = "https://passportpdfapi.com/test/multiple_pages.pdf";

            DocumentApi docApi = new();

            Console.WriteLine("Loading documents into PassportPDF...");

            DocumentLoadResponse document1 = await docApi.DocumentLoadFromURIAsync(new LoadDocumentFromURIParameters(uri1));
            DocumentLoadResponse document2 = await docApi.DocumentLoadFromURIAsync(new LoadDocumentFromURIParameters(uri2));

            string uri1Id = document1.FileId;
            string uri2Id = document2.FileId;

            Console.WriteLine("Documents loaded.");

            Console.WriteLine("Merging PDF documents..");
            
            PDFApi pdfApi = new();

            List<string> listOfFilesIds = new List<string> { uri1Id, uri2Id };
            PdfMergeResponse pdfMergeResponse = await pdfApi.MergeAsync(new PdfMergeParameters(listOfFilesIds));

            if (pdfMergeResponse.Error is not null)
            {
                throw new ApiException(pdfMergeResponse.Error.ExtResultMessage);
            }
            else
            {
                Console.WriteLine("PDF documents have been successfully merged.");
            }

            // Download merged document
            Console.WriteLine("Downloading merged document..");
            try
            {
                string mergedDocumentId = pdfMergeResponse.FileId;

                string savePath = Path.Join(Directory.GetCurrentDirectory(), "merged_document.pdf");

                PdfSaveDocumentResponse pdfSaveDocumentResponse = await pdfApi.SaveDocumentAsync(new PdfSaveDocumentParameters(mergedDocumentId));

                File.WriteAllBytes(savePath, pdfSaveDocumentResponse.Data);

                if (pdfMergeResponse.Error is not null)
                {
                    throw new ApiException(pdfSaveDocumentResponse.Error.ExtResultMessage);
                }
                else
                {
                    Console.WriteLine("Merged document has been successfully downloaded. It has been saved in : {0}", savePath);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not download merged document! : {0}", ex.Message);
            }

        }
    }
}


