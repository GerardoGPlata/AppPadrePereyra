using EscolarAppPadres.Models;

namespace EscolarAppPadres.Models
{
    public class PendingPaymentsResponse
    {
        public List<ColegiaturaDto> Colegiaturas { get; set; } = new List<ColegiaturaDto>();
        public List<InscripcionDto> Inscripciones { get; set; } = new List<InscripcionDto>();
        public List<OtrosDocumentosDto> OtrosDocumentos { get; set; } = new List<OtrosDocumentosDto>();
        public ResumenPagos Resumen { get; set; } = new ResumenPagos();
    }

    public class ResumenPagos
    {
        public int TotalColegiaturas { get; set; }
        public int TotalInscripciones { get; set; }
        public int TotalOtrosDocumentos { get; set; }
        public int TotalDocumentos { get; set; }
        public decimal ImporteTotalColegiaturas { get; set; }
        public decimal ImporteTotalInscripciones { get; set; }
        public decimal ImporteTotalOtros { get; set; }
        public decimal ImporteGrandTotal { get; set; }
    }
}