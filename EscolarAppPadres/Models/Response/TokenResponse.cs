namespace EscolarAppPadres.Models.Response
{
    public class TokenResponse
    {
        public int SesionId { get; set; }
        public int UsuarioId { get; set; }
        public int PadreId { get; set; }  // Nuevo campo
        public int TipoUsuarioId { get; set; }
        public string? Correo { get; set; }
        public string? Nombre { get; set; }
        public string? Token { get; set; }
        public DateTime Inicia { get; set; }
        public DateTime Finaliza { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
        public List<Hijo>? Hijos { get; set; }  // Nueva lista de hijos
    }
}
