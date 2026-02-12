using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using weather.Models;
using weather.Services;
using weather.Plats;
namespace weather
{
    public class Program
    {
        public static List<string> lines = new List<string>();
        public static string defaultpath = "../../../Files/Input/";
        public static string defaultFileName = "tempdata.txt";
        static void Main(string[] args)
        {
            FileHandling.FileInput(defaultFileName, defaultpath);
            List<Measurement> measurements = new();
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
                            Ute.Outdoor();
                            break;
                        case 2:
                            Inne.Indoor();
                            break;
                        case 3:
                            Sorting.WriteToFile(measurements);
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
        
    }
}
