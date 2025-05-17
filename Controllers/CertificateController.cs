using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CertGenAPI.Models;
using CertGenAPI.Services;
using System.Text.Json;

namespace CertGenAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificateController : ControllerBase
    {
        private readonly CertificateService _certService;
        private readonly EmailService _emailService;
        private readonly FileStorageService _fileStorageService;

        public CertificateController(CertificateService certService, EmailService emailService, FileStorageService fileStorageService)
        {
            _certService = certService;
            _emailService = emailService;
            _fileStorageService = fileStorageService;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateCertificate([FromBody] CertificateRequest request)
        {
            Console.WriteLine("Received POST request.");
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest("Name and Email are required.");

                // Determine certificate type based on role
                string templateType = request.Role.Equals("Committee", StringComparison.OrdinalIgnoreCase) ? "A" : "B";

                // Generate certificate with appropriate type
                var pdfPath = _certService.GenerateCertificate(request.Name, request.Role);

                // Save submission to JSON
                await _fileStorageService.SaveSubmissionAsync(request);

                // Email certificate
                await _emailService.SendCertificateAsync(request.Email, request.Name, pdfPath);

                Console.WriteLine("Certificate processed successfully.");
                return Ok(new { message = "Certificate generated and emailed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error occurred:");
                Console.WriteLine(ex.ToString()); // This shows full exception details
                return StatusCode(500, "An error occurred while processing the certificate.");
            }
        }
            /*public IActionResult GenerateCertificate([FromBody] CertificateRequest request)
            {
                if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest("Name and Email are required.");

                string pdfPath = _certService.GenerateCertificate(request.Name);

                return Ok(new { message = "Certificate generated", path = pdfPath });
            }*/
        [HttpGet("all")]
        public IActionResult GetAllSubmissions()
        {
            var submissions = _fileStorageService.GetAllSubmissions();
            return Ok(submissions);
        }

        [HttpGet("check-ic")]
        public IActionResult CheckICNumber([FromQuery] string icNumber)
        {
            var submissionsPath = Path.Combine(Directory.GetCurrentDirectory(), "submissions.json");

            if (!System.IO.File.Exists(submissionsPath))
            {
                return Ok(false); // No file means no submissions yet
            }

            var jsonData = System.IO.File.ReadAllText(submissionsPath);
            var submissions = JsonSerializer.Deserialize<List<CertificateRequest>>(jsonData);

            Console.WriteLine($"Checking for IC: {icNumber}");
            Console.WriteLine("Existing ICs:");
            foreach (var sub in submissions)
            {
                Console.WriteLine(sub.ICNumber);
            }

            bool exists = submissions.Any(s => s.ICNumber?.Trim() == icNumber.Trim());
            return Ok(exists);
        }
    }
}
