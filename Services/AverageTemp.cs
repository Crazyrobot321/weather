using System;
using System.Collections.Generic;
using System.Text;
using weather.Entities;

namespace weather.Services
{
    public class AverageTemp
    {
        public static void Calculate_AverageTemp_SpecifiedDate(List<Measurement> measurements, string plats)
        {
            Console.Write("Ange datum (YYYY-MM-DD): ");
            string inputDate = Console.ReadLine();

            if (!DateTime.TryParse(inputDate, out DateTime selectedDate))
            {
                Console.WriteLine("Ogiltigt datumformat. Använd formatet YYYY-MM-DD.");
                return;
            }
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, selectedDate, true, plats);
            var result = measurements
                .GroupBy(m => m.Datum.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    AvgTemp = g.Average(x => x.Temperature),
                    AvgHumidity = g.Average(x => x.Humidity)
                })
                .FirstOrDefault();

            if (result == null)
            {
                Console.WriteLine("Inga mätningar hittades för valt datum.");
            }
            else
            {
                Console.WriteLine($"{result.Date:yyyy-MM-dd} | Medeltemp: {result.AvgTemp:F1}°C | Medelluftfuktighet: {result.AvgHumidity:F1}%");
            }
        }
    }
}
