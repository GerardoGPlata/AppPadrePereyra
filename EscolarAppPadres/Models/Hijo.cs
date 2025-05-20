using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class Hijo
    {
        public int AlumnoId { get; set; }
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string? Matricula { get; set; }
        public int ClaveFamiliarId { get; set; }
        public string? ClaveFamiliar { get; set; }
        public int PadresOTutoresId { get; set; }
        public string? NombreTutor { get; set; }
        public string? CorreoTutor { get; set; }
        public string? NombreCompleto { get; set; }
    }
}
