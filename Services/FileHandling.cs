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
            string filename = "output.txt";
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
                    MedelTempUte = g.Where(x => x.Plats == "Ute").Average(x => x.Temperatur),
                    MedelFuktighetUte = g.Where(x => x.Plats == "Ute").Average(x => x.Fuktighet),
                    MedelTempInne = g.Where(x => x.Plats == "Inne").Average(x => x.Temperatur),
                    MedelFuktighetInne = g.Where(x => x.Plats == "Inne").Average(x => x.Fuktighet),

                    UteRisk = Math.Clamp((g.Where(x => x.Plats == "Ute").Average(x => x.Fuktighet) - 75) *
                                         (g.Where(x => x.Plats == "Ute").Average(x => x.Temperatur) / 15) * 22, 0, 100),

                    InneRisk = Math.Clamp((g.Where(x => x.Plats == "Inne").Average(x => x.Fuktighet) - 20) *
                                          (g.Where(x => x.Plats == "Inne").Average(x => x.Temperatur) / 15) * 1, 0, 100)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            foreach (var line in monthlyData)
            {
                reportLines.Add($"{line.Year}-{line.Month:D2} | Medelluftfuktighet inne/ute: ({line.MedelFuktighetInne:F1})({line.MedelFuktighetUte:F1})%" +
                    $" | Medeltemp inne/ute: ({line.MedelTempInne:F1})({line.MedelTempUte:F1})°C" +
                    $" | Mögelrisk inne/ute: ({line.InneRisk:F1})({line.UteRisk:F1})%");
            }
            reportLines.Add("");

            //Kollar säsongerna
            reportLines.Add("===SÄSONGSSTART 2016====");

            var Meterologisk = measurements
                .Where(m => m.Plats == "Ute")
                .GroupBy(m => m.Datum.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    MedelTemp = g.Average(x => x.Temperatur)
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Hitta Höst (5 dagar under 10 grader efter 1 aug)
            int count = 0;
            foreach (var dag in Meterologisk)
            {
                if (dag.Date < new DateTime(dag.Date.Year, 8, 1)) //Datumet är minst den 1a augusti
                    continue;
                if (dag.MedelTemp < 10.0)
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
                if (dag.MedelTemp <= 0.0)
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
                    .Where(d => Math.Round(d.MedelTemp, 1) <= 0.0)
                    .FirstOrDefault();

                if (fristColdDay != null)
                {
                    winterResult = fristColdDay.Date;
                    string didWinterStart =
                        count < 5 ? $"Vintern uppstod inte meterologiskt, visar närmsta dag {winterResult}" :
                        count == 5 ? $"Vinter började: {winterResult}" :
                        "Error";
                    reportLines.Add(didWinterStart);

                }
            }
            reportLines.Add("");
            reportLines.Add("================");
            reportLines.Add("Datan skrevs ut: " + DateTime.Now);
            // 4. SKRIV TILL FIL
            WriteFile(reportLines);
        }

    }
}
