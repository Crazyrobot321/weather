using System;
using System.Collections.Generic;
using System.Text;
using weather.Entities;

namespace weather.Services
{
    internal class FallAndWinter
    {
        public static void CalcFallAndWinter(List<Measurement> measurements, string choice)
        {
            ParseAndFilter.ParseOchFiltreraMeasurements(measurements, null, false, "ute");
            var sortedDays = measurements
                .GroupBy(m => m.Datum.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    MedelTemp = g.Average(x => x.Temperatur)
                })
                .OrderBy(d => d.Date)
                .ToList();
            DateTime? FallStart = null;
            DateTime? WinterStart = null;
            int dagarIrad = 0;
            if (choice == "fall")
            {
                foreach (var dag in sortedDays)
                {
                    if (dag.Date < new DateTime(dag.Date.Year, 8, 1))
                        continue;
                    if (dag.MedelTemp < 10.0)
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
                foreach (var dag in sortedDays)
                {
                    if (dag.MedelTemp <= 0.0)
                        dagarIrad++;
                    else
                        dagarIrad = 0;

                    if (dagarIrad == 5)
                    {
                        WinterStart = dag.Date.AddDays(-4);
                        break;
                    }
                }
                if (WinterStart == null)
                {
                    var fristColdDay = sortedDays
                        .OrderBy(d => d.Date)
                        .Where(d => Math.Round(d.MedelTemp, 1) <= 0.0)
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
            {
                Console.WriteLine("Inget val");
            }
        }

    }
}
