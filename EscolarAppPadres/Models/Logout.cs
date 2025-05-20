using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class Logout
    {
        public int UsuarioId { get; set; }
        public string? WebToken { get; set; }
    }
}
