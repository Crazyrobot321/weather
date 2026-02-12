using System;
using System.Collections.Generic;
using System.Text;
using weather.Models;

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
                    AvgTemp = g.Average(x => x.Temperature),
                    AvgHumidity = g.Average(x => x.Humidity)
                })
                .OrderByDescending(order => order.AvgTemp)
                .ToList();
            foreach (var line in result)
            {
                Console.WriteLine($"{line.Date:yyyy-MM-dd} | Medeltemp: {line.AvgTemp:F1}°C | Medelluftfuktighet: {line.AvgHumidity:F1}%");
            }
        }
        public static void Sort_Torraste(List<Measurement> measurements, string plats)
        {
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, null, false, plats);
            var result = measurements
                .GroupBy(m => m.Datum.Date).Select(g => new { Date = g.Key, AvgTemp = g.Average(x => x.Temperature), AvgHumidity = g.Average(x => x.Humidity) })
                .OrderBy(order => order.AvgHumidity)
                .ToList();
            foreach (var line in result)
            {
                Console.WriteLine($"{line.Date:yyyy-MM-dd} | Medelluftfuktighet: {line.AvgHumidity:F1}% | Medeltemp: {line.AvgTemp:F1}°C");
            }
        }
        public static void Mold_Risk(List<Measurement> measurements, string plats)
        {
            List<string> moldAlgorithmSteps = new List<string>
            {
                "1. MedelRF = Average(Humidity)",
                "2. MedelTemp = Average(Temperature)",
                "3. FuktÖverskott = MedelRF - 75",
                "4. TemperaturFaktor = MedelTemp / 15",
                "5. Risken = FuktÖverskott * TemperaturFaktor",
                "6. SkaladRisk = Risken * 22",
                "7. UteRisk = Clamp(SkaladRisk, 0, 100)"
            };
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, null, false, plats);
            var result = measurements
                .GroupBy(m => m.Datum.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    AvgTemp = g.Average(x => x.Temperature),
                    AvgHumidity = g.Average(x => x.Humidity),
                    UteRisk = Math.Clamp((g.Average(x => x.Humidity) - 75) * (g.Average(x => x.Temperature) / 15) * 22, 0, 100),
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
                Console.WriteLine($"{line.Date:yyyy-MM-dd} | Risk för mögel: {riskLevel} ({line.UteRisk:F1})% | Medelluftfuktighet: {line.AvgHumidity:F1}% | Temperatur: {line.AvgTemp:F1}°C");
            }
            Console.ResetColor();
            FileHandling.WriteFile(moldAlgorithmSteps);
        }
        public static void WriteToFile(List<Measurement> measurements)
        {
            List<string> test = new();
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, null, false, "ute|inne");
            var inneResult = measurements
                .GroupBy(m => new { m.Datum.Year, m.Datum.Month })
                .Select(g => new
                {

                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    AvgTempUte = g.Where(x => x.Location == "Ute").Average(x => x.Temperature),
                    AvgHumUte = g.Where(x => x.Location == "Ute").Average(x => x.Humidity),
                    AvgTempInne = g.Where(x => x.Location == "Inne").Average(x => x.Temperature),
                    AvgHumInne = g.Where(x => x.Location == "Inne").Average(x => x.Humidity),

                    UteRisk = Math.Clamp((g.Where(x => x.Location == "Ute").Average(x => x.Humidity) - 75) *
                                         (g.Where(x => x.Location == "Ute").Average(x => x.Temperature) / 15) * 22, 0, 100),

                    InneRisk = Math.Clamp((g.Where(x => x.Location == "Inne").Average(x => x.Humidity) - 75) *
                                          (g.Where(x => x.Location == "Inne").Average(x => x.Temperature) / 15) * 22, 0, 100)
                })
                .OrderBy(x => x.Year)
                .ToList();
            foreach (var line in inneResult)
            {
                test.Add($"{line.Year}-{line.Month} | Medelluftfuktighet inne/ute: ({line.AvgHumInne:F1})({line.AvgHumUte:F1})%" +
                    $" | Medeltemp inne/ute: ({line.AvgTempInne:F1})({line.AvgTempUte:F1})°C" +
                    $" | Mögelrisk inne/ute: ({line.InneRisk:F1})({line.UteRisk:F1})%");
            }
            FileHandling.WriteFile(test);
        }
    }
}
