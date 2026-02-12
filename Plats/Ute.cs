using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using weather.Classes;
using weather.FilterMeasurements;

namespace weather.UteInne
{
    public class Ute
    {
        public static void Outdoor()
        {
            Console.Clear();
            Console.WriteLine("Utomhus");
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine(
                "1. Medeltemperatur och luftfuktighet per dag, för valt datum (sökmöjlighet med validering)\r\n" +
                "2. Sortering av varmast till kallaste dagen enligt medeltemperatur per dag\r\n" +
                "3. Sortering av torrast till fuktigaste dagen enligt medelluftfuktighet per dag\r\n" +
                "4. Sortering av minst till störst risk av mögel\r\n" +
                "5. Datum för meteorologisk Höst\r\n" +
                "6. Datum för meteologisk vinter (OBS Mild vinter!)");
            Console.Write("Val: ");
            if (!int.TryParse(Console.ReadLine(), out int choice))
                return;
            switch (choice)
            {
                case 1:
                    AvgTempAvgHum();
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
                case 5:
                    CalcFallAndWinter("fall");
                    break;
                case 6:
                    CalcFallAndWinter("winter");
                    break;
                default:
                    break;
            }
        }
        internal static void AvgTempAvgHum()
        {
            List<Measurement> measurements = new();
            Console.Write("Ange datum (YYYY-MM-DD): ");
            string inputDate = Console.ReadLine();

            if (!DateTime.TryParse(inputDate, out DateTime selectedDate))
            {
                Console.WriteLine("Ogiltigt datumformat. Använd formatet YYYY-MM-DD.");
                return;
            }
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, selectedDate, true, "ute");
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
        internal static void Sort_Varmaste()
        {
            List<Measurement> measurements = new();
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, null, false, "ute");
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
        internal static void Sort_Torraste()
        {
            List<Measurement> measurements = new();
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, null, false, "ute");
            var result = measurements
                .GroupBy(m => m.Datum.Date).Select(g => new {Date = g.Key,AvgTemp = g.Average(x => x.Temperature), AvgHumidity = g.Average(x => x.Humidity)})
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
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, null, false, "ute");
            var result = measurements
                .GroupBy(m => m.Datum.Date)
                .Select(g =>
                {
                    double temp = g.Average(x => x.Temperature);
                    double humidity = g.Average (x => x.Humidity);
                    double risk = (humidity > 75 && temp > 0) ? (humidity - 75) * (temp / 15) : 0;
                    return new
                    {
                        Date = g.Key,
                        AvgTemp = temp,
                        AvgHumidity = humidity,
                        MoldRisk = Math.Clamp(risk,0,100)
                    };
                })
                .OrderByDescending(order => order.MoldRisk)
                .ToList();
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
                if(line.MoldRisk == 0) //Skippar dagar med 0% mögelrisk
                    continue;
                Console.WriteLine($"{line.Date:yyyy-MM-dd} | Risk för mögel: {riskLevel} ({line.MoldRisk:F1})% | Medelluftfuktighet: {line.AvgHumidity:F1}% | Temperatur: {line.AvgTemp:F1}°C");
            }
            Console.ResetColor();
        }    
        public static void CalcFallAndWinter(string choice)
        {
            List<Measurement> measurements = new();
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, null,false, "ute");

            var sortedDays = measurements
                .GroupBy(m => m.Datum.Date)
                .Select(g => new 
                { 
                    Date = g.Key, 
                    AvgTemp = g.Average(x => x.Temperature) 
                })
                .OrderBy(d => d.Date)
                .ToList();
            DateTime? FallStart = null;
            DateTime? WinterStart = null;
            int dagarIrad = 0;
            if(choice == "fall")
            {
                foreach (var dag in sortedDays)
                {
                    if (dag.Date < new DateTime(dag.Date.Year, 8, 1))
                        continue;
                    if (dag.AvgTemp < 10.0)
                        dagarIrad++;
                    else
                        dagarIrad = 0;
                    if (dagarIrad == 5)
                    {
                        FallStart = dag.Date.AddDays(-4);
                        dagarIrad = 0;
                        break;
                    }
                }
                Console.WriteLine($"Höst började: {FallStart}");

            }
            else if (choice == "winter")
            {
                foreach (var day in sortedDays)
                {
                    // Regel: Dygnsmedeltemperatur 0.0 grader eller lägre
                    if (day.AvgTemp <= 0.0)
                        dagarIrad++;
                    else
                        dagarIrad = 0;

                    if (dagarIrad == 5)
                    {
                        WinterStart = day.Date.AddDays(-4);
                        break;
                    }
                }
                if(WinterStart == null)
                {
                    var fristColdDay = sortedDays
                        .OrderBy(d => d.Date)
                        .Where(d => Math.Round(d.AvgTemp, 1) <= 0.0)
                        .FirstOrDefault();

                    if (fristColdDay != null)
                    {
                        WinterStart = fristColdDay.Date;
                        string didWinterStart =
                            dagarIrad < 5 ? $"Det uppstod inte 5 dagar i rad med < 0 grader, visar närmsta dag {WinterStart}" :
                            dagarIrad == 5 ? $"Vinter började: {WinterStart}" :
                            "Error";
                        Console.WriteLine(didWinterStart);
                    }
                }
            }
            else
                Console.WriteLine("Inget val");
        }

    }
}
