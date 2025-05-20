using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class Decrypt
    {
        public string? SingleData { get; set; }
        public List<string>? DataListCipher { get; set; }
        public bool IsMultiple { get; set; }
        public string? token { get; set; }
    }
}
