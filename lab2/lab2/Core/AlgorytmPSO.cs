using System;
using System.Collections.Generic;
using System.IO;

namespace SSI_Lab2_Ewolucyjne.Core
{
    public class AlgorytmPSO
    {
        public List<StanIteracji> Wykonaj(int N, double rIteracji, double rLok, int iteracjeLiczba, int seed = 123)
        {
            List<StanIteracji> historia = new List<StanIteracji>();

            Random los = new Random(seed);
            
            double xMin = 0.0;
            double xMax = 100.0;

            List<Osobnik> roj = new List<Osobnik>();

            for(int j =0; j<N; j++)
            {
                double x1 = xMin + los.NextDouble() * (xMax - xMin);
                double x2 = xMin + los.NextDouble() * (xMax - xMin);

                roj.Add(new Osobnik(x1, x2));
            }

            Osobnik xGlob = ZnajdzNajlepszego(roj).Klonuj();
            historia.Add(ZbudujStan(0, roj, xGlob));

            // EWOLUCAAAA!
            for (int iter=1; iter <= iteracjeLiczba; iter++)
            {
                for(int j=0; j < N; j++)
                {
                    roj[j].Ocena = FunkcjePrzystosowania.Oblicz2D(roj[j].X1, roj[j].X2);

                    if (roj[j].Ocena > xGlob.Ocena)
                    {
                        xGlob = roj[j].Klonuj();
                    }

                    if (roj[j].Ocena > roj[j].NajlepszaOcena)
                    {
                        roj[j].NajlepszeX1 = roj[j].X1;
                        roj[j].NajlepszeX2 = roj[j].X2;
                        roj[j].NajlepszaOcena = roj[j].Ocena;
                    }
                }

                
                // tutaj nowa prędkość i pozycja/przesunięcie dla osobników w roju 
                for (int j =0; j < N; j++)
                {
                    double rndGlob = los.NextDouble();
                    double rndLok = los.NextDouble();

                    roj[j].Vx1 = roj[j].Vx1 * rIteracji + (xGlob.X1 - roj[j].X1) * rndGlob + (roj[j].NajlepszeX1 - roj[j].X1) * rLok * rndLok;
                    roj[j].Vx2 = roj[j].Vx2 * rIteracji + (xGlob.X2 - roj[j].X2) * rndGlob + (roj[j].NajlepszeX2 - roj[j].X2) * rLok * rndLok;

                    if (roj[j].Ocena > xGlob.Ocena)
                    {
                        xGlob = roj[j].Klonuj();
                    }
                }


                historia.Add(ZbudujStan(iter, roj, xGlob));
            }

            return historia;
        }


        private Osobnik ZnajdzNajlepszego(List<Osobnik> pula)
        {
            Osobnik naj = pula[0];


            for (int i = 1; i< pula.Count; i++)
            {
                if (pula[i].Ocena > naj.Ocena) naj = pula[i];
            }
            return naj;
        }


        private StanIteracji ZbudujStan(int iter, List<Osobnik> roj, Osobnik xGlob)
        {
            StanIteracji stan = new StanIteracji
            {
                NumerIteracji = iter,
                NajlepszyOsobnik = xGlob.Klonuj()
            };

            Osobnik najgorszy = roj[0];
            for(int i =0; i< roj.Count; i++)
            {
                stan.PulaGlowna.Add(roj[i].Klonuj());
                if (roj[i].Ocena < najgorszy.Ocena) najgorszy = roj[i];
            }

            StringWriter sw = new StringWriter();
            sw.WriteLine($"=== ALGORYTM PSO (ITERACJA {iter}) ===");
            sw.WriteLine($"Najlepszy globalnie (X_glob):");
            sw.WriteLine($"  x1 = {xGlob.X1:F4}, x2 = {xGlob.X2:F4}");
            sw.WriteLine($"  F(x1, x2) = {xGlob.Ocena:F5}");
            sw.WriteLine($"\nNajgorszy punkt w roju:");
            sw.WriteLine($"  x1 = {najgorszy.X1:F4}, x2 = {najgorszy.X2:F4}");
            sw.WriteLine($"  F(x1, x2) = {najgorszy.Ocena:F5}");

            sw.WriteLine("\nPozycje wszystkich czastek:");

            for (int i = 0; i< roj.Count; i++)
            {
                sw.WriteLine($"  #{i + 1}: [{roj[i].X1:F2}; {roj[i].X2:F2}] -> F = {roj[i].Ocena:F4}");
            }

            stan.TekstRaportu = sw.ToString();
            return stan;
        }
    }
}