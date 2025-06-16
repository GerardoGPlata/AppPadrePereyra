namespace EscolarAppPadres.Models
{
    public class CreateChargeMovilResponseDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
    }
}