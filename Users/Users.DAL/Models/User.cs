using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Users.DAL.Models
{
    public class User
    {
        [SwaggerSchema(ReadOnly = true)]
        public Guid Id { get; set; } // = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;    // AES256 en BD/DAL
        [JsonPropertyName("tax_id")]
        public string TaxId { get; set; } = string.Empty;       // RFC
        [JsonPropertyName("created_at")]
        [SwaggerSchema(ReadOnly = true)]
        public string CreatedAt { get; set; }  = DateTime.UtcNow.AddHours(3).ToString("dd-MM-yyyy HH:mm");   // dd-MM-yyyy HH:mm Madagascar
        public List<Address> Addresses { get; set; } = new();
    }
}
