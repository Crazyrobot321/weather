using System.Globalization;
using System.Text.RegularExpressions;

namespace weather
{
    internal class Program
    {
        public static List<string> lines = new List<string>();
        public static string path = "../../../Files/";
        static void Main(string[] args)
        {
            FileInput("tempdata.txt");
            while (true)
            {
                Console.WriteLine("1. Utomhus");
                Console.WriteLine("2. Inomhus");
                Console.WriteLine("3. Avsluta");
                Console.Write("Val: ");
                if(int.TryParse(Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            Ute.Ute.Outdoor();
                            break;
                        case 2:
                            break;
                        case 3:
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
                    while(line != null)
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
    }
}
