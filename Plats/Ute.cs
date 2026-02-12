using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using weather.Entities;
using weather.Services;

namespace weather.Plats
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
            List<Measurement> measurments = new();
            switch (choice)
            {

                case 1:
                    AverageTemp.Calculate_AverageTemp_SpecifiedDate(measurments, "ute");
                    break;
                case 2:
                    Sorting.Sort_Varmaste(measurments, "ute");
                    break;
                case 3:
                    Sorting.Sort_Torraste(measurments, "ute");
                    break;
                case 4:
                    Sorting.Mold_Risk(measurments, "ute");
                    break;
                case 5:
                    FallAndWinter.CalcFallAndWinter(measurments,"fall");
                    break;
                case 6:
                    FallAndWinter.CalcFallAndWinter(measurments, "winter");
                    break;
                default:
                    break;
            }
        }
                
    }
}
