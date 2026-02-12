using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using weather.Models;
using weather.Services;

namespace weather.Plats
{
    public class Inne
    {
        public static void Indoor()
        {
            Console.Clear();
            Console.WriteLine("Inne");
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine(
                "1. Medeltemperatur per dag, för valt datum (sökmöjlighet med validering)\r\n" +
                "2. Sortering av varmast till kallaste dagen enligt medeltemperatur per dag\r\n" +
                "3. Sortering av torrast till fuktigaste dagen enligt medelluftfuktighet per dag\r\n" +
                "4. Sortering av minst till störst risk av mögel\r\n");
            Console.Write("Val: ");
            if (!int.TryParse(Console.ReadLine(), out int choice))
                return;
            List<Measurement> measurments = new();
            switch (choice)
            {
                case 1:
                    AverageTemp.Calculate_AverageTemp_SpecifiedDate(measurments, "inne");
                    break;
                case 2:
                    Sorting.Sort_Varmaste(measurments, "inne");
                    break;
                case 3:
                    Sorting.Sort_Torraste(measurments, "inne");
                    break;
                case 4:
                    Sorting.Mold_Risk(measurments, "inne");
                    break;
                default:
                    break;
            }
        }
        
    }
}
