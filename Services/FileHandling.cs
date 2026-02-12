using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
                        Console.WriteLine("Wrote: " + line);
                    }
                }
            }
            catch
            {
                Console.WriteLine("Fil error!");
            }
        }
    }
}
