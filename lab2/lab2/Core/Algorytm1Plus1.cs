using System;
using System.Collections.Generic;
using System.IO;

namespace SSI_Lab2_Ewolucyjne.Core
{
    public class Algorytm1Plus1
    {
        public List<StanIteracji> Wykonaj(double rozrzutPoczatkowy, double wspPrzyrostu, int lIteracji, double startMin =0.0, double startMax=100.0, int seed = 123)
        {
            List<StanIteracji> historia = new List<StanIteracji>();
            Random los = new Random(seed);

            double zakresMin = 0.0;
            double zakresMax = 100.0;
            double rozrzut = rozrzutPoczatkowy;

            double x = startMin + los.NextDouble() * (startMax - startMin);
            double y = FunkcjePrzystosowania.Oblicz1D(x);

            historia.Add(ZbudujStan(0, x, y, x, y, rozrzut));

            for (int iter = 1; iter<= lIteracji; iter++)
            {
                double przesuniecie = (los.NextDouble() * 2.0 - 1.0) * rozrzut;
                double xPot = x + przesuniecie;
                xPot = FunkcjePrzystosowania.PrzytnijDoZakresu(xPot, zakresMin, zakresMax);
                double yPot = FunkcjePrzystosowania.Oblicz1D(xPot);

                if (yPot >= y)
                {
                    x = xPot;
                    y = yPot;
                    rozrzut *= wspPrzyrostu;
                } else
                {
                    rozrzut /= wspPrzyrostu;
                }

                historia.Add(ZbudujStan(iter, x,y,xPot,yPot, rozrzut));
            }

            return historia;
        }



        private StanIteracji ZbudujStan(int iter, double x, double y, double xPot, double yPot, double rozrzut)
        {
            StanIteracji stan = new StanIteracji
            {
                NumerIteracji = iter,
                X = x,
                Y = y,
                XPot = xPot,
                YPot = yPot,
                Rozrzut = rozrzut
            };
            
            StringWriter sw = new StringWriter();
            sw.WriteLine($"=== ALGORYTM 1+1 (ITERACJA {iter}) ===");
            sw.WriteLine($"Aktualne x:       {x:F5}");
            sw.WriteLine($"Aktualne y = f(x):{y:F5}");
            sw.WriteLine($"Rozrzut:          {rozrzut:F5}");
            if (iter > 0)
            {
                sw.WriteLine($"\nOstatni kandydat x_pot: {xPot:F5}");
                sw.WriteLine($"Ocena kandydata y_pot:  {yPot:F5}");
            }

            stan.TekstRaportu = sw.ToString();

            return stan;
        }
    }
}