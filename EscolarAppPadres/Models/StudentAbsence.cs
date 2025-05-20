namespace EscolarAppPadres.Models
{
    public class StudentAbsence
    {
        public int AlumnoPorCicloId { get; set; }
        public string Matricula { get; set; }
        public string NombreCompleto { get; set; }
        public string Materia { get; set; }
        public string TipoAsistenciaNombre { get; set; }
        public DateTime Fecha { get; set; }
        public string NombreProfesor { get; set; }
        public int ProfesorPorMateriaPlanEstudioId { get; set; }
    }
}
