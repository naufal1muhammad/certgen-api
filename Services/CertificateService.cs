using System;
using System.IO;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;

namespace CertGenAPI.Services
{
    public class CertificateService
    {
        private readonly string _templateFolder = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        private readonly string _outputFolder = "/data/Certificates";

        public CertificateService()
        {
            if (!Directory.Exists(_outputFolder))
                Directory.CreateDirectory(_outputFolder);
        }

        public string GenerateCertificate(string name, string role, string icNumber)
        {
            try
            {
                Console.WriteLine($"[Syncfusion] Generating certificate for: {name} ({role})");

                string templatePath = role.ToLower() == "committee"
                    ? Path.Combine(_templateFolder, "committee_template.docx")
                    : Path.Combine(_templateFolder, "participant_template.docx");

                if (!File.Exists(templatePath))
                    throw new FileNotFoundException("Template not found", templatePath);

                // Load the Word template
                using WordDocument document = new WordDocument(templatePath, FormatType.Docx);

                // Replace placeholders
                document.Replace("{{name}}", name, false, true);

                // Convert to PDF
                using DocIORenderer renderer = new DocIORenderer();
                using PdfDocument pdfDocument = renderer.ConvertToPDF(document);

                // Generate output path
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string outputPath = Path.Combine(_outputFolder, $"{name}_{icNumber}.pdf");

                // Save PDF
                using FileStream outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                pdfDocument.Save(outputStream);

                Console.WriteLine($"✅ PDF certificate created at: {outputPath}");
                return outputPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("‼️ Error generating Syncfusion certificate:");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}