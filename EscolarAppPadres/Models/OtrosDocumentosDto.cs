using System.ComponentModel;

namespace EscolarAppPadres.Models
{
    public class OtrosDocumentosDto : INotifyPropertyChanged
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

        public decimal ImporteCalculado => SaldoTotal + Interes;

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Propiedades del DTO
        public int DocumentoPorPagarId { get; set; }
        public string Documento { get; set; }
        public int SubConceptoId { get; set; }
        public int PagoEstatusId { get; set; }
        public int Visible { get; set; }
        public int AlumnoId { get; set; }
        public int? SolicitudAdmisionId { get; set; }
        public int CicloId { get; set; }
        public string Ciclo { get; set; }
        public int NivelId { get; set; }
        public string Grado { get; set; }
        public int GradoId { get; set; }
        public string Nivel { get; set; }
        public string Grupo { get; set; }
        public int? MedioPagoId { get; set; }
        public decimal Saldo { get; set; }
        public decimal SaldoTotal { get; set; }
        public decimal DescuentoDoc { get; set; }
        public decimal Importe { get; set; }
        public decimal ImporteInscripcion { get; set; }
        public decimal SaldoInscripcion { get; set; }
        public DateTime FechaLimitePago { get; set; }
        public string FechaLimite { get; set; }
        public string FechaLimiteFormato { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaProntoPago { get; set; }
        public string Referencia { get; set; }
        public string ReferenciaBanco { get; set; }
        public int DocumentoId { get; set; }
        public string IdNomina { get; set; }
        public int Hermanos { get; set; }
        public int Reingreso { get; set; }
        public int PadreExAlumno { get; set; }
        public string Concepto { get; set; }
        public int EsCurricular { get; set; }
        public int? AcuerdoId { get; set; }
        public int? TipoAcuerdoId { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal IVA { get; set; }
        public decimal Interes { get; set; }
        public string Hoy { get; set; }
        public string Alumno { get; set; }
        public string Matricula { get; set; }
        public int ClaveFamiliarId { get; set; }
        public int PadresOTutoresId { get; set; }
        public int TipoDocumento { get; set; }
        public int EmpresaId { get; set; }

        // Propiedades calculadas para UI
        public string TipoDocumentoTexto => "Otros";
        public string ImporteFormateado => ImporteCalculado.ToString("C2");
    }
}