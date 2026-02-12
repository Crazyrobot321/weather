using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using weather.Classes;
using weather.FilterMeasurements;

namespace weather.UteInne
{
    public class Inne
    {
        public static void Indoor()
        {
            Console.Clear();
            Console.WriteLine("Inne");
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine(
                "1. Medeltemperatur per dag, för valt datum (sökmöjlighet med validering)\r\n" +
                "2. Sortering av varmast till kallaste dagen enligt medeltemperatur per dag\r\n" +
                "3. Sortering av torrast till fuktigaste dagen enligt medelluftfuktighet per dag\r\n" +
                "4. Sortering av minst till störst risk av mögel\r\n");
            Console.Write("Val: ");
            if (!int.TryParse(Console.ReadLine(), out int choice))
                return;
            switch (choice)
            {
                case 1:
                    AvgTemp();
                    break;
                case 2:
                    Sort_Varmaste();
                    break;
                case 3:
                    Sort_Torraste();
                    break;
                case 4:
                    Mold_Risk();
                    break;
                default:
                    break;
            }
        }
        internal static void AvgTemp()
        {
            List<Measurement> measurements = new();
            Console.Write("Ange datum (YYYY-MM-DD): ");
            string inputDate = Console.ReadLine();

            if (!DateTime.TryParse(inputDate, out DateTime selectedDate))
            {
                Console.WriteLine("Ogiltigt datumformat. Använd formatet YYYY-MM-DD.");
                return;
            }
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, selectedDate, true, "inne");
            var result = measurements
                .GroupBy(m => m.Datum.Date).Select(g => new { Date = g.Key, AvgTemp = g.Average(x => x.Temperature)})
                .FirstOrDefault();

            if (result == null)
            {
                Console.WriteLine("Inga mätningar hittades för valt datum.");
            }
            else
            {
                Console.WriteLine($"{result.Date:yyyy-MM-dd} | Medeltemp: {result.AvgTemp:F1}°C");
            }

        }
        internal static void Sort_Varmaste()
        {
            List<Measurement> measurements = new();
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, null, false, "inne");
            var result = measurements
                .GroupBy(m => m.Datum.Date).Select(g => new { Date = g.Key, AvgTemp = g.Average(x => x.Temperature), AvgHumidity = g.Average(x => x.Humidity) })
                .OrderByDescending(order => order.AvgTemp)
                .ToList();
            foreach (var line in result)
            {
                Console.WriteLine($"{line.Date:yyyy-MM-dd} | Medeltemp: {line.AvgTemp:F1}°C | Medelluftfuktighet: {line.AvgHumidity:F1}%");
            }
        }
        internal static void Sort_Torraste()
        {
            List<Measurement> measurements = new();
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, null, false, "inne");
            var result = measurements
                .GroupBy(m => m.Datum.Date).Select(g => new { Date = g.Key, AvgTemp = g.Average(x => x.Temperature), AvgHumidity = g.Average(x => x.Humidity) })
                .OrderBy(order => order.AvgHumidity)
                .ToList();
            foreach (var line in result)
            {
                Console.WriteLine($"{line.Date:yyyy-MM-dd} | Medelluftfuktighet: {line.AvgHumidity:F1}% | Medeltemp: {line.AvgTemp:F1}°C");
            }
        }
        internal static void Mold_Risk()
        {
            List<Measurement> measurements = new();
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, null, false, "inne");
            var result = measurements
                .GroupBy(m => m.Datum.Date)
                .Select(g =>
                {
                    double temp = g.Average(x => x.Temperature);
                    double humidity = g.Average(x => x.Humidity);
                    double risk = (humidity > 75 && temp > 0) ? (humidity - 75) * (temp / 15) : 0;
                    return new
                    {
                        Date = g.Key,
                        AvgTemp = temp,
                        AvgHumidity = humidity,
                        MoldRisk = Math.Clamp(risk, 0, 100)
                    };
                })
                .OrderByDescending(order => order.MoldRisk)
                .ToList();
            Console.WriteLine($"");
            foreach (var line in result)
            {
                Console.ForegroundColor =
                    line.MoldRisk < 5 ? ConsoleColor.Green :
                    line.MoldRisk < 15 ? ConsoleColor.DarkYellow :
                    line.MoldRisk < 30 ? ConsoleColor.DarkRed :
                    ConsoleColor.Magenta;
                string riskLevel =
                    line.MoldRisk < 5 ? "Låg" :
                    line.MoldRisk < 15 ? "Medel" :
                    line.MoldRisk < 30 ? "Hög" :
                    "Mycket hög";
                if (line.MoldRisk == 0) //Skippar dagar med 0% mögelrisk vilket i detta fall blir inga resultat alls
                    continue;
                Console.WriteLine($"{line.Date:yyyy-MM-dd} | Risk för mögel: {riskLevel} ({line.MoldRisk:F1})% | Medelluftfuktighet: {line.AvgHumidity:F1}% | Temperatur: {line.AvgTemp:F1}°C");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        
    }
}
