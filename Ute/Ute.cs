using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace weather.Ute
{
    public class Ute
    {
        public static void Outdoor()
        {

            double avgTemp = 0;
            double avgHumidity = 0;
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

            foreach (var line in Program.lines)
            {
                var match = Regex.Match(line, @"^(?<datum>\d{4}-\d{1,2}-\d{1,2})\W(?<tid>\d{2}:\d{2}:\d{2}),(?<plats>ute),(?<temp>\d{1,2}[\.,]\d{1,2}),(?<fuktighet>\d{1,3})$", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    DateTime datum = DateTime.Parse(match.Groups["datum"].Value, CultureInfo.InvariantCulture);

                    if (datum.Date == selectedDate.Date)
                    {
                        measurements.Add(new Measurement
                        {   //InvariantCulture används för att undvika problem med decimaltecken pga regionsinställningar
                            Datum = datum,
                            Temperature = double.Parse(match.Groups["temp"].Value, CultureInfo.InvariantCulture),
                            Humidity = double.Parse(match.Groups["fuktighet"].Value, CultureInfo.InvariantCulture)
                        });
                    }
                }
            }
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
            foreach (var line in Program.lines)
            {
                var match = Regex.Match(line, @"^(?<datum>\d{4}-\d{1,2}-\d{1,2})\W(?<tid>\d{2}:\d{2}:\d{2}),(?<plats>ute),(?<temp>\d{1,2}[\.,]\d{1,2}),(?<fuktighet>\d{1,3})$", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    DateTime datum = DateTime.Parse(match.Groups["datum"].Value, CultureInfo.InvariantCulture);

                    measurements.Add(new Measurement
                    {   //InvariantCulture används för att undvika problem med decimaltecken pga regionsinställningar
                        Datum = datum,
                        Temperature = double.Parse(match.Groups["temp"].Value, CultureInfo.InvariantCulture),
                        Humidity = double.Parse(match.Groups["fuktighet"].Value, CultureInfo.InvariantCulture)
                    });
                }
            }
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
            foreach (var line in Program.lines)
            {
                var match = Regex.Match(line, @"^(?<datum>\d{4}-\d{1,2}-\d{1,2})\W(?<tid>\d{2}:\d{2}:\d{2}),(?<plats>ute),(?<temp>\d{1,2}[\.,]\d{1,2}),(?<fuktighet>\d{1,3})$", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    DateTime datum = DateTime.Parse(match.Groups["datum"].Value, CultureInfo.InvariantCulture);

                    measurements.Add(new Measurement
                    {   //InvariantCulture används för att undvika problem med decimaltecken pga regionsinställningar
                        Datum = datum,
                        Temperature = double.Parse(match.Groups["temp"].Value, CultureInfo.InvariantCulture),
                        Humidity = double.Parse(match.Groups["fuktighet"].Value, CultureInfo.InvariantCulture)
                    });
                }
            }
            var result = measurements
                .GroupBy(m => m.Datum.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    AvgTemp = g.Average(x => x.Temperature),
                    AvgHumidity = g.Average(x => x.Humidity)
                })
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
            foreach (var line in Program.lines)
            {
                var match = Regex.Match(line, @"^(?<datum>\d{4}-\d{1,2}-\d{1,2})\W(?<tid>\d{2}:\d{2}:\d{2}),(?<plats>ute),(?<temp>\d{1,2}[\.,]\d{1,2}),(?<fuktighet>\d{1,3})$", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    DateTime datum = DateTime.Parse(match.Groups["datum"].Value, CultureInfo.InvariantCulture);

                    measurements.Add(new Measurement
                    {   //InvariantCulture används för att undvika problem med decimaltecken pga regionsinställningar
                        Datum = datum,
                        Temperature = double.Parse(match.Groups["temp"].Value, CultureInfo.InvariantCulture),
                        Humidity = double.Parse(match.Groups["fuktighet"].Value, CultureInfo.InvariantCulture)
                    });
                }
            }
            var result = measurements
                .GroupBy(m => m.Datum.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    AvgTemp = g.Average(x => x.Temperature),
                    AvgHumidity = g.Average(x => x.Humidity),
                    MoldRisk = Math.Clamp((g.Average(x => x.Humidity)-75.0) / 20.0 * 100.0, 0.0, 100.0)
                })
                .OrderByDescending(order => order.MoldRisk)
                .ToList();
            foreach (var line in result)
            {
                Console.WriteLine($"{line.Date:yyyy-MM-dd} | Risk för mögel: {line.MoldRisk:F1}% | Medelluftfuktighet: {line.AvgHumidity:F1}% | Temperatur: {line.AvgTemp:F1}°C");
            }
        }
    }
}
