using System.Globalization;
using System.Text.RegularExpressions;
using weather.Classes;
using weather.UteInne;

namespace weather
{
    internal class Program
    {
        public static List<string> lines = new List<string>();
        public static string path = "../../../Files/Input/";
        static void Main(string[] args)
        {
            FileInput("tempdata.txt");
            while (true)
            {
                Console.WriteLine("1. Utomhus");
                Console.WriteLine("2. Inomhus");
                Console.WriteLine("3. Skriv till fil");
                Console.WriteLine("4. Avsluta");
                Console.Write("Val: ");
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            UteInne.Ute.Outdoor();
                            break;
                        case 2:
                            UteInne.Inne.Indoor();
                            break;
                        case 3:
                            Console.Write("Filnamn: ");
                            WriteFile(Console.ReadLine());
                            break;
                        case 4:
                            return;
                        default:
                            Console.WriteLine("Ogiltigt val, försök igen.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Vänligen ange ett giltigt nummer.");
                }
            }
        }

        static void FileInput(string filename)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path + filename))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        if (!line.Contains("2016-05") && !line.Contains("2017-01"))
                            lines.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }
            catch
            {
                Console.WriteLine("Filen finns inte");
            }
        }
        static void WriteFile(string? filename)
        {
            List<Measurement> measurements = new();
            if (string.IsNullOrEmpty(filename))
            {
                Console.WriteLine("Inget filnamn specifierat");
                return;
            }
            //Inne.tempName(measurements, null, false);
            //Ute.tempName(measurements, null, false);
            var inneResult = measurements
                .GroupBy(m => new { m.Datum.Year, m.Datum.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    AvgTemp = g.Average(x => x.Temperature),
                    AvgHumidity = g.Average(x => x.Humidity)
                })
                .ToList();
            foreach (var line in inneResult)
            {
                Console.WriteLine($"{line.Year}-{line.Month} | Medelluftfuktighet: {line.AvgHumidity:F1}% | Medeltemp: {line.AvgTemp:F1}°C");
            }
        }
    }
}
