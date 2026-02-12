using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using weather.Entities;

namespace weather.Services
{
    internal class FileHandling
    {
        public static void FileInput(string filename, string path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path + filename))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        if (!line.Contains("2016-05") && !line.Contains("2017-01"))
                            Program.lines.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }
            catch
            {
                Console.WriteLine("Filen finns inte");
            }
        }
        public static void WriteFile(List<string> content)
        {
            string filename = "TextFile1.txt";
            try
            {
                using (StreamWriter writer = new StreamWriter("../../../Files/Output/" + filename, true))
                {
                    foreach (string line in content)
                    {
                        writer.WriteLine(line);
                    }
                }
                Console.WriteLine("Skrivit klart till filen!");
            }
            catch
            {
                Console.WriteLine("Fil error!");
            }
        }
        public static void CreateFinalReport(List<Measurement> measurements)
        {
            List<string> reportLines = new();

            reportLines.Add(
                    @"=== FORMEL FÖR MÖGELRISK ===
                    1. MedelRF = Average(Humidity)
                    2. MedelTemp = Average(Temperature)
                    3. FuktÖverskott = MedelRF - 75
                    4. TemperaturFaktor = MedelTemp / 15
                    5. Risken = FuktÖverskott * TemperaturFaktor
                    6. SkaladRisk = Risken * 22
                    7. Riskvärde = Clamp(SkaladRisk, 0, 100)
                    ");

            reportLines.Add("=== MÅNADSSTATISTIK ===");
            var monthlyData = measurements
                .GroupBy(m => new { m.Datum.Year, m.Datum.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    AvgTempUte = g.Where(x => x.Location == "Ute").Average(x => x.Temperature),
                    AvgHumUte = g.Where(x => x.Location == "Ute").Average(x => x.Humidity),
                    AvgTempInne = g.Where(x => x.Location == "Inne").Average(x => x.Temperature),
                    AvgHumInne = g.Where(x => x.Location == "Inne").Average(x => x.Humidity),

                    UteRisk = Math.Clamp((g.Where(x => x.Location == "Ute").Average(x => x.Humidity) - 75) *
                                         (g.Where(x => x.Location == "Ute").Average(x => x.Temperature) / 15) * 22, 0, 100),

                    InneRisk = Math.Clamp((g.Where(x => x.Location == "Inne").Average(x => x.Humidity) - 75) *
                                          (g.Where(x => x.Location == "Inne").Average(x => x.Temperature) / 15) * 22, 0, 100)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            foreach (var line in monthlyData)
            {
                reportLines.Add($"{line.Year}-{line.Month:D2} | Medelluftfuktighet inne/ute: ({line.AvgHumInne:F1})({line.AvgHumUte:F1})%" +
                    $" | Medeltemp inne/ute: ({line.AvgTempInne:F1})({line.AvgTempUte:F1})°C" +
                    $" | Mögelrisk inne/ute: ({line.InneRisk:F1})({line.UteRisk:F1})%");
            }
            reportLines.Add("");

            //Kollar säsongerna
            reportLines.Add("=== SÄSONGSSTART 2016 ====");

            var Meterologisk = measurements
                .Where(m => m.Location == "Ute")
                .GroupBy(m => m.Datum.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    AvgTemp = g.Average(x => x.Temperature)
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Hitta Höst (5 dagar under 10 grader efter 1 aug)
            int count = 0;
            foreach (var dag in Meterologisk)
            {
                if (dag.Date < new DateTime(dag.Date.Year, 8, 1)) //Datumet är minst den 1a augusti
                    continue;
                if (dag.AvgTemp < 10.0)
                    count++;
                else
                    count = 0;
                if (count == 5)
                {
                    string fallResultString = $"Hösten startade: {dag.Date.AddDays(-4):yyyy-MM-dd}";
                    reportLines.Add(fallResultString);
                    count = 0;
                    break;
                }
            }

            // Hitta Vinter (5 dagar under 0 grader)
            DateTime? winterResult = null;
            count = 0;
            foreach (var dag in Meterologisk)
            {
                if (dag.AvgTemp <= 0.0)
                    count++;
                else
                    count = 0;

                if (count == 5)
                {
                    string result = $"Vintern startade: {dag.Date.AddDays(-4):yyyy-MM-dd}";
                    break;
                }
            }
            if (winterResult == null)
            {
                var fristColdDay = Meterologisk
                    .OrderBy(d => d.Date)
                    .Where(d => Math.Round(d.AvgTemp, 1) <= 0.0)
                    .FirstOrDefault();

                if (fristColdDay != null)
                {
                    winterResult = fristColdDay.Date;
                    string didWinterStart =
                        count < 5 ? $"Det uppstod inte 5 dagar i rad med < 0 grader, visar närmsta dag {winterResult}" :
                        count == 5 ? $"Vinter började: {winterResult}" :
                        "Error";
                    reportLines.Add(didWinterStart);

                }
            }

            // 4. SKRIV TILL FIL
            WriteFile(reportLines);
        }

    }
}
