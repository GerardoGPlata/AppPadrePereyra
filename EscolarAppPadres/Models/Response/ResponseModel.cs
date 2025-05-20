using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models.Response
{
    public class ResponseModel<T>
    {
        public bool Result { get; set; } = false;
        public bool Valoration { get; set; } = false;
        public string Message { get; set; } = "Mensaje por defecto";
        public string? Log { get; set; } = null;
        public List<T> Data { get; set; } = new List<T>();

        public bool IsClientError { get; set; } = false;
    }
}
