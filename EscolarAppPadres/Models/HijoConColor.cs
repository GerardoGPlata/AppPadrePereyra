using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class HijoConColor : INotifyPropertyChanged
    {
        public int AlumnoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        private Color _color = Colors.Transparent; // respaldo inicial
        public Color Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged(nameof(Color));
                    OnPropertyChanged(nameof(ColorHex));
                }
            }
        }
        public string ColorHex
        {
            get => Color.ToHex();
            set
            {
                var newColor = Color.FromArgb(value);
                if (_color != newColor)
                {
                    _color = newColor;
                    OnPropertyChanged(nameof(Color));
                    OnPropertyChanged(nameof(ColorHex));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


}
