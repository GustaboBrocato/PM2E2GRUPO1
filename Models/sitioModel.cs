using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM2E2GRUPO1.Models
{
    public class sitioModel
    {
        public int id { get; set; }
        public string? descripcion { get; set; }
        public double latitud { get; set; }
        public double longitud { get; set; }
        public string? videoDigital { get; set; }
        public string? audioFile { get; set; }
    }
}
