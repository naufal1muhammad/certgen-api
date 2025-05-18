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
        private readonly IWebHostEnvironment _env;

        public CertificateController(CertificateService certService, EmailService emailService, FileStorageService fileStorageService, IWebHostEnvironment env)
        {
            _certService = certService;
            _emailService = emailService;
            _fileStorageService = fileStorageService;
            _env = env;
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
                var pdfPath = _certService.GenerateCertificate(request.Name, request.Role, request.ICNumber);

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
            var submissionsPath = "/data/submissions.json";

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

        [HttpDelete("delete-submission")]
        public async Task<IActionResult> DeleteSubmission([FromQuery] string icNumber, [FromQuery] string token)
        {
            if (token != "adminsecret123") return Unauthorized("Access denied.");

            if (string.IsNullOrEmpty(icNumber))
            {
                return BadRequest("Please provide a valid IC Number.");
            }

            var filePath = "/data/submissions.json";

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("submissions.json not found.");
            }

            // Read existing data
            var json = await System.IO.File.ReadAllTextAsync(filePath);
            var submissions = JsonSerializer.Deserialize<List<CertificateRequest>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (submissions == null || submissions.Count == 0)
            {
                return NotFound("No submissions found.");
            }

            // Try to remove the matching submission
            var removedCount = submissions.RemoveAll(s => s.ICNumber == icNumber);

            if (removedCount == 0)
            {
                return NotFound($"No submission found with IC Number: {icNumber}");
            }

            // Save updated list
            var updatedJson = JsonSerializer.Serialize(submissions, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(filePath, updatedJson);

            return Ok($"Deleted {removedCount} submission(s) with IC Number: {icNumber}");
        }

        [HttpDelete("delete-certificate")]
        public IActionResult DeleteCertificate([FromQuery] string icNumber, [FromQuery] string token)
        {
            if (token != "adminsecret123") return Unauthorized("Access denied.");

            if (string.IsNullOrEmpty(icNumber))
            {
                return BadRequest("Please provide a valid IC Number.");
            }

            var certificatesDir = Path.Combine(_env.ContentRootPath, "data", "Certificates");

            if (!Directory.Exists(certificatesDir))
            {
                return NotFound("Certificates folder not found.");
            }

            // Look for matching files (assuming they contain IC Number in the filename)
            var matchingFiles = Directory.GetFiles(certificatesDir, $"*{icNumber}*.pdf");

            if (matchingFiles.Length == 0)
            {
                return NotFound($"No certificate found for IC Number: {icNumber}");
            }

            foreach (var file in matchingFiles)
            {
                System.IO.File.Delete(file);
            }

            return Ok($"Deleted {matchingFiles.Length} certificate(s) for IC Number: {icNumber}");
        }
    }
}
