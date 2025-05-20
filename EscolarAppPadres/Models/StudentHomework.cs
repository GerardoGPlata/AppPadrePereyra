using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class StudentHomework
    {
        public string MateriaNombre { get; set; }
        public int MateriaId { get; set; }
        public string ProfesorNombre { get; set; }
        public string GradoNombre { get; set; }
        public string GrupoNombre { get; set; }
        public string PeriodoEvaluacionNombre { get; set; }
        public string PeriodoEvaluacionNombreCorto { get; set; }
        public int PeriodoEvaluacionId { get; set; }  // periodoevaluacionid
        public string TareaNombre { get; set; }
        public int TareaId { get; set; }  // TareaID
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public string fechai { get; set; }  // corresponde a "fechai"
        public string fechaini { get; set; }  // corresponde a "fechaini"
        public string fechaf { get; set; }  // corresponde a "fechaf"
        public string Descripcion { get; set; }
        public int TareaAlumnoId { get; set; }  // tareaalumnoid
        public bool Entregado { get; set; }
        public double? Calificacion { get; set; }
        public string TipoEntrega { get; set; }
        public int TipoEntregaId { get; set; }  // tipoentregaid
        public string HoraLimite { get; set; }
        public double PuntajeMaximo { get; set; }
        public bool EntregaExtemporanea { get; set; }
        public int IDAlumno { get; set; }  // IDAlumno
        public bool Captura { get; set; }
        public int CriterioEvaluacionGrupoId { get; set; }  // criterioevaluaciongrupoid
        public int gradoportallercurricularid { get; set; }  // gradoportallercurricularid
    }
}
