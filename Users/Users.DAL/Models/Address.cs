using System.Text.Json.Serialization;

namespace Users.DAL.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }
    }
}
