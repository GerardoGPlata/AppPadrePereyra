using System.Text.Json.Serialization;

namespace EscolarAppPadres.Models.Openpay
{
    public class OpenpayChargeRequestDto
    {
        [JsonPropertyName("customer")]
        public CustomerDto Customer { get; set; }
        [JsonPropertyName("charge_request")]
        public ChargeRequestDto ChargeRequest { get; set; }
    }

    public class ChargeRequestDto
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("redirectUrl")] // <-- Cambiado para coincidir con el request
        public string RedirectUrl { get; set; }
        [JsonPropertyName("confirm")]
        public string Confirm { get; set; }
        [JsonPropertyName("sendEmail")]
        public bool SendEmail { get; set; }
    }

    public class OpenpayChargeResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("authorization")]
        public string Authorization { get; set; }
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("operation_type")]
        public string OperationType { get; set; }
        [JsonPropertyName("transaction_type")]
        public string TransactionType { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("conciliated")]
        public bool Conciliated { get; set; }
        [JsonPropertyName("creation_date")]
        public DateTime CreationDate { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("error_message")]
        public string ErrorMessage { get; set; }
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("payment_method")]
        public PaymentMethodDto PaymentMethod { get; set; }
        [JsonPropertyName("customer")]
        public CustomerDto Customer { get; set; }
    }

    public class PaymentMethodDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class CustomerDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("lastName")] // <-- Cambiado para coincidir con el request
        public string LastName { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("phoneNumber")] // <-- Cambiado para coincidir con el request
        public string PhoneNumber { get; set; }
    }
}