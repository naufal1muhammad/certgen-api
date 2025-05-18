using System.Text.Json;
using CertGenAPI.Models;

namespace CertGenAPI.Services
{
    public class FileStorageService
    {
        private readonly string _submissionFile = "/data/submissions.json";
        private static readonly object _fileLock = new();

        public async Task SaveSubmissionAsync(CertificateRequest request)
        {
            try
            {
                var submission = new
                {
                    name = request.Name,
                    email = request.Email,
                    icNumber = request.ICNumber,
                    mmcNumber = request.MMCNumber,
                    role = request.Role,
                    feedbackContent = request.FeedbackContent,
                    feedbackDuration = request.FeedbackDuration,
                    feedbackSpeakers = request.FeedbackSpeakers,
                    feedbackFacilitators = request.FeedbackFacilitators,
                    timestamp = DateTime.UtcNow
                };

                lock (_fileLock)
                {

                    List<object> allSubmissions = new();

                    if (File.Exists(_submissionFile))
                    {
                        string existingData = File.ReadAllText(_submissionFile);
                        if (!string.IsNullOrWhiteSpace(existingData))
                            allSubmissions = JsonSerializer.Deserialize<List<object>>(existingData) ?? new List<object>();
                    }

                    allSubmissions.Add(submission);

                    string json = JsonSerializer.Serialize(allSubmissions, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_submissionFile, json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File save error: {ex.Message}");
                throw;
            }
        }

        public List<object> GetAllSubmissions()
        {
            lock (_fileLock)
            {
                if (!File.Exists(_submissionFile))
                    return new List<object>();

                string json = File.ReadAllText(_submissionFile);
                return JsonSerializer.Deserialize<List<object>>(json) ?? new List<object>();
            }
        }
    }
}
