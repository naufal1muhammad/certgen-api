using System.Text.Json.Serialization;

namespace CertGenAPI.Models
{
    public class CertificateRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }

        [JsonPropertyName("icNumber")]
        public string ICNumber { get; set; } = string.Empty;
        public string MMCNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Committee or Participant
        public int FeedbackContent { get; set; }
        public int FeedbackDuration { get; set; }
        public int FeedbackSpeakers { get; set; }
        public int FeedbackFacilitators { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
