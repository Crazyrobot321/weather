using System;
using System.Collections.Generic;
using System.Text;

namespace weather.Classes
{
    public class Measurement
    {
        public DateTime Datum { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }
}
