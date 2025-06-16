namespace EscolarAppPadres.Models
{
    public class CreateChargeMovilRequestDto
    {
        public string DocumentoPorPagarId { get; set; } = string.Empty;
        public string TipoPago { get; set; } = string.Empty;
        public decimal Importe { get; set; }
        public string Concepto { get; set; } = string.Empty;
    }
}