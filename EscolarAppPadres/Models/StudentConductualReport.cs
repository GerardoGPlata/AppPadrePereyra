using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class StudentConductualReport
    {
        public int ReporteDisciplinaId { get; set; }
        public int AlumnoPorCicloId { get; set; }
        public string Observaciones { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int TipoReporteId { get; set; }
        public int MateriaPorPlanEstudiosId { get; set; }
        public int UsuarioId { get; set; }
        public int AreaDisciplinaId { get; set; }
    }
}
