using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using weather.Models;

namespace weather.Services
{
    public class ParseAndFilter
    {
        internal static void ParseOchFiltreraMeasurements(List<Measurement> measurements, DateTime? selectedDate, bool specificDate, string location)
        {
            string pattern = $@"^(?<datum>\d{{4}}-\d{{1,2}}-\d{{1,2}})\W(?<tid>\d{{2}}:\d{{2}}:\d{{2}}),(?<plats>{location}),(?<temp>\d{{1,2}}[\.,]\d{{1,2}}),(?<fuktighet>\d{{1,3}})$";
            foreach (var line in Program.lines)
            {
                var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    string dateText = match.Groups["datum"].Value;
                    if (!DateTime.TryParse(dateText, CultureInfo.InvariantCulture, out DateTime datum))
                    {
                        //Console.WriteLine($"Invalid date found '{dateText}', skipping...");
                        continue;
                    }
                    if (specificDate && !selectedDate.HasValue)
                    {
                        Console.WriteLine("Inget datum valt");
                        continue;
                    }
                    if (specificDate)
                    {
                        if (datum.Date == selectedDate.Value.Date)
                        {
                            measurements.Add(new Measurement
                            {   //InvariantCulture används för att undvika problem med decimaltecken pga regionsinställningar
                                Datum = datum,
                                Temperature = double.Parse(match.Groups["temp"].Value, CultureInfo.InvariantCulture),
                                Humidity = double.Parse(match.Groups["fuktighet"].Value, CultureInfo.InvariantCulture),
                                Location = match.Groups["plats"].Value
                            });
                        }
                    }
                    else
                    {
                        measurements.Add(new Measurement
                        {
                            Datum = datum,
                            Temperature = double.Parse(match.Groups["temp"].Value, CultureInfo.InvariantCulture),
                            Humidity = double.Parse(match.Groups["fuktighet"].Value, CultureInfo.InvariantCulture),
                            Location = match.Groups["plats"].Value
                        });
                    }

                }
            }
        }
    }
}
