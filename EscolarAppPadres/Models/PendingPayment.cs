using System;
using System.ComponentModel;

namespace EscolarAppPadres.Models
{
    public class PendingPayment : INotifyPropertyChanged
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

        public decimal ImporteCalculado => Saldo - DescuentoDoc + Interes;

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Propiedades ya existentes
        public int DocumentoPorPagarId { get; set; }
        public string Documento { get; set; }
        public int AlumnoId { get; set; }
        public string Alumno { get; set; }
        public string Matricula { get; set; }
        public int ClaveFamiliarId { get; set; }
        public int PadresOTutoresId { get; set; }
        public int CicloId { get; set; }
        public string Ciclo { get; set; }
        public string Grado { get; set; }
        public int GradoId { get; set; }
        public string Nivel { get; set; }
        public int NivelId { get; set; }
        public string Grupo { get; set; }
        public string Referencia { get; set; }
        public string ReferenciaBanco { get; set; }
        public int PagoEstatusId { get; set; }
        public int DocumentoId { get; set; }
        public decimal Saldo { get; set; }
        public decimal Importe { get; set; }
        public string Concepto { get; set; }
        public decimal DescuentoDoc { get; set; }
        public decimal Interes { get; set; }
        public string FechaLimite { get; set; }
        public string FechaLimiteFormato { get; set; }
        public string Hoy { get; set; }
        public bool Visible { get; set; }
    }
}
