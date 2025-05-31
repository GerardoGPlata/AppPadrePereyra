using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class SchoolDirectory
    {
        public int DirectorioEscolarId { get; set; }
        public string NombreDepartamento { get; set; }
        public string CorreoElectronico { get; set; }
        public string Telefono { get; set; }
        public string Extension { get; set; }
        public string NombreResponsable { get; set; }
        public int OrdenDirectorio { get; set; }
        public bool Activo { get; set; }
    }
}
