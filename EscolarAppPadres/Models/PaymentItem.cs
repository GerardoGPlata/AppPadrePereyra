using System.ComponentModel;

namespace EscolarAppPadres.Models.Payments
{
    public class PaymentItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Propiedades unificadas
        public string DocumentoPorPagarId { get; set; }
        public string Documento { get; set; }
        public string Concepto { get; set; }
        public string Alumno { get; set; }
        public string Matricula { get; set; }
        public string Grado { get; set; }
        public string Grupo { get; set; }
        public decimal ImporteCalculado { get; set; }
        public string FechaLimiteFormato { get; set; }
        public string TipoDocumentoTexto { get; set; }
        public string ImporteFormateado => ImporteCalculado.ToString("C2");
        public int TipoDocumento { get; set; } // 1=Inscripción, 2=Colegiatura, 3=Otros

        // Propiedades para identificar el origen
        public ColegiaturaDto ColegiaturaOriginal { get; set; }
        public InscripcionDto InscripcionOriginal { get; set; }
        public OtrosDocumentosDto OtroDocumentoOriginal { get; set; }

        public decimal ImporteBase
        {
            get
            {
                if (ColegiaturaOriginal != null) return ColegiaturaOriginal.Importe;
                if (InscripcionOriginal != null) return InscripcionOriginal.Importe;
                if (OtroDocumentoOriginal != null) return OtroDocumentoOriginal.Importe;
                return 0;
            }
        }

        public decimal DescuentoDoc
        {
            get
            {
                if (ColegiaturaOriginal != null) return ColegiaturaOriginal.DescuentoDoc;
                if (InscripcionOriginal != null) return InscripcionOriginal.DescuentoDoc;
                if (OtroDocumentoOriginal != null) return OtroDocumentoOriginal.DescuentoDoc;
                return 0;
            }
        }

        public decimal InteresDoc
        {
            get
            {
                if (ColegiaturaOriginal != null) return ColegiaturaOriginal.Interes;
                if (InscripcionOriginal != null) return InscripcionOriginal.Interes;
                if (OtroDocumentoOriginal != null) return OtroDocumentoOriginal.Interes;
                return 0;
            }
        }
    }
}