using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class HijoConColor
    {
        public int AlumnoId { get; set; }
        public string NombreCompleto { get; set; }
        public string Matricula { get; set; }
        public Color Color { get; set; } // Cambiamos a Color para bindear mejor
        public string ColorHex
        {
            get => Color.ToHex();
            set => Color = Color.FromHex(value);
        }
    }


}
