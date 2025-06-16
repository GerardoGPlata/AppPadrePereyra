namespace EscolarAppPadres.Models.Openpay
{
    public class OpenpayMovilChargeRequestDto
    {
        public string DocumentoPorPagarId { get; set; }
        public string TipoPago { get; set; }
        public decimal Importe { get; set; }
        public string Concepto { get; set; }
    }
}