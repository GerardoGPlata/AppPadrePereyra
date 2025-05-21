using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class StudentGrade
    {
        public int Id { get; set; }
        public int AlumnoId { get; set; }
        public int MateriaId { get; set; }
        public string MateriaNombre { get; set; }
        public string NombreCorto { get; set; }
        public decimal? Calificacion { get; set; }
        public string? Ponderacion { get; set; }
        public string? Observacion { get; set; }
        public int? CalificacionFinalPeriodoPorAlumnoId { get; set; }
        public bool SeImprimeEnBoleta { get; set; }
    }

}
