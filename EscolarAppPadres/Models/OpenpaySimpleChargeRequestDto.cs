using System.Text.Json.Serialization;

namespace EscolarAppPadres.Models
{
    public class OpenpaySimpleChargeRequestDto
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("LastName")]
        public string LastName { get; set; }
        [JsonPropertyName("PhoneNumber")]
        public string PhoneNumber { get; set; }
        [JsonPropertyName("Email")]
        public string Email { get; set; }
        [JsonPropertyName("Amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("Description")]
        public string Description { get; set; }
        [JsonPropertyName("OrderId")]
        public string OrderId { get; set; }
        [JsonPropertyName("RedirectUrl")]
        public string RedirectUrl { get; set; }
    }
}