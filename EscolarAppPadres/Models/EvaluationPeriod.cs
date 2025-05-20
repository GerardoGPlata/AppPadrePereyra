using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class EvaluationPeriod
    {
        public int PeriodoEvaluacionId { get; set; }
        public string Descripcion { get; set; }
        public string DescripcionCorta { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime FechaPublicacionPrevia { get; set; }
        public DateTime FechaPublicacionDefinitiva { get; set; }
        public List<StudentGrade> Calificaciones { get; set; }
    }
}
