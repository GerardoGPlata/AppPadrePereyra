using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class StudentSubject
    {
        public int ProfesorPorMateriaId { get; set; }
        public int MateriaPorPlanEstudioId { get; set; }
        public string Profesor { get; set; }
        public string Materia { get; set; }
    }
}
