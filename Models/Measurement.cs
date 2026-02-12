using System;
using System.Collections.Generic;
using System.Text;

namespace weather.Models
{
    public class Measurement
    {
        public DateTime Datum { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public string Location { get; set; }
    }
}
