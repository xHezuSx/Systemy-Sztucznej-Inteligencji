using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace ConsoleApp1
{
    internal class Program
    {
        class Punkt
        {  
            public double X1 { get; set; }
            public double X2 { get; set; }
            public int Grupa { get; set; }
        }


        static void Main(string[] args)
        {
            Random rand = new Random(123);
            string plik = "spiralka.csv";
            if (!File.Exists(plik)){
                Console.WriteLine("BŁĄD ODCZYTU PILIKU!");
                return;
            }

            List<Punkt> probki = WczytajProbki(plik);
            
            // foreach (var p in probki) { Console.WriteLine($"X1: {p.X1}, X2: {p.X2}, G: {p.Grupa}\n"); };

        }


        static List<Punkt> WczytajProbki(string sciezka)
        {
            var punkty = new List<Punkt>();
            foreach (var linia in File.ReadAllLines(sciezka))
            {
                if (string.IsNullOrWhiteSpace(linia)) continue;
                var czesci = linia.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (czesci.Length >= 2)
                {
                    double x1 = double.Parse(czesci[0].Trim(), CultureInfo.InvariantCulture);
                    double x2 = double.Parse(czesci[1].Trim(), CultureInfo.InvariantCulture);
                    punkty.Add(new Punkt { X1 = x1, X2 = x2 });
                }
            }
            return punkty;
        }
    }
}
