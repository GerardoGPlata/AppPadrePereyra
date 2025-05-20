using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class Token
    {
        public int UsuarioId { get; set; }
        public string? WebToken { get; set; }
        public DateTime Finaliza { get; set; }
    }
}
