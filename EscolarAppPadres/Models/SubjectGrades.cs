using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class SubjectGrades
    {
        public string Materia { get; set; } // Nombre de la materia

        // Las siguientes propiedades se mapearán dinámicamente
        public Dictionary<string, string> CalificacionesPorPeriodo { get; set; } = new();
    }

}
