using System;
using System.Collections.Generic;
using System.Text;
using weather.Entities;

namespace weather.Services
{
    internal class Sorting
    {
        public static void Sort_Varmaste(List<Measurement> measurements, string plats)
        {
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, null, false, plats);
            var result = measurements
                .GroupBy(m => m.Datum.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    MedelTemp = g.Average(x => x.Temperatur),
                    MedelFuktighetidity = g.Average(x => x.Fuktighet)
                })
                .OrderByDescending(order => order.MedelTemp)
                .ToList();
            foreach (var line in result)
            {
                Console.WriteLine($"{line.Date:yyyy-MM-dd} | Medeltemp: {line.MedelTemp:F1}°C | Medelluftfuktighet: {line.MedelFuktighetidity:F1}%");
            }
        }
        public static void Sort_Torraste(List<Measurement> measurements, string plats)
        {
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, null, false, plats);
            var result = measurements
                .GroupBy(m => m.Datum.Date)
                .Select(g => new 
                { 
                    Date = g.Key, 
                    MedelTemp = g.Average(x => x.Temperatur), 
                    MedelFuktighetidity = g.Average(x => x.Fuktighet) 
                })
                .OrderBy(order => order.MedelFuktighetidity)
                .ToList();
            foreach (var line in result)
            {
                Console.WriteLine($"{line.Date:yyyy-MM-dd} | Medelluftfuktighet: {line.MedelFuktighetidity:F1}% | Medeltemp: {line.MedelTemp:F1}°C");
            }
        }
        public static void Mold_Risk(List<Measurement> measurements, string plats)
        {
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, null, false, plats);
            var result = measurements
                .GroupBy(m => m.Datum.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    MedelTemp = g.Average(x => x.Temperatur),
                    MedelFuktighetidity = g.Average(x => x.Fuktighet),
                    UteRisk = Math.Clamp((g.Average(x => x.Fuktighet) - 75) * (g.Average(x => x.Temperatur) / 15) * 22, 0, 100),
                })
                .OrderByDescending(order => order.UteRisk)
                .ToList();
            foreach (var line in result)
            {
                Console.ForegroundColor =
                    line.UteRisk < 15 ? ConsoleColor.Green :
                    line.UteRisk < 45 ? ConsoleColor.DarkYellow :
                    line.UteRisk < 75 ? ConsoleColor.DarkRed :
                    ConsoleColor.Magenta;
                string riskLevel =
                    line.UteRisk < 15 ? "Låg" :
                    line.UteRisk < 45 ? "Medel" :
                    line.UteRisk < 75 ? "Hög" :
                    "Mycket hög";
                Console.WriteLine($"{line.Date:yyyy-MM-dd} | Risk för mögel: {riskLevel} ({line.UteRisk:F1})% | Medelluftfuktighet: {line.MedelFuktighetidity:F1}% | Temperatur: {line.MedelTemp:F1}°C");
            }
            Console.ResetColor();
        }
    }
}
