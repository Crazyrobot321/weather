using System;
using System.Collections.Generic;
using System.Text;

namespace weather.Entities
{
    public class Measurement
    {
        public DateTime Datum { get; set; }
        public double Temperatur { get; set; }
        public double Fuktighet { get; set; }
        public string Plats { get; set; }
    }
}
