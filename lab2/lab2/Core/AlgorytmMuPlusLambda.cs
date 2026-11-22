using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using SSI_Lab2_Ewolucyjne.Core;

namespace SSI_Lab2_Ewolucyjne.Core
{
    public class AlgorytmMuPlusLambda
    {
        public List<StanIteracji> Wykonaj(
                int mu,
                int lambda,
                int turniejRozmiar,
                double mutacjaPoziom,
                int iteracjeLiczba,
                int seed = 123)
        {
            List<StanIteracji> historia = new List<StanIteracji>();
            Random los = new Random(seed);

            double xMin = 0.0;
            double xMax = 100.0;

            List<Osobnik> pulaRodzicielska = new List<Osobnik>();
            for (int i = 0; i < mu; i++)
            {
                double x1 = xMin + los.NextDouble() * (xMax - xMin);
                double x2 = xMin + los.NextDouble() * (xMax - xMin);
                pulaRodzicielska.Add(new Osobnik(x1, x2));
            }

            SortujMalejaco(pulaRodzicielska);
            historia.Add(ZbudujStan(0, pulaRodzicielska, new List<Osobnik>()));

            for (int iter=1; iter<= iteracjeLiczba; iter++)
            {
                List<Osobnik> pulaPotomna = new List<Osobnik>();

                for (int k =0; k < lambda; k++)
                {
                    Osobnik zwyciezcaTurnieju = WybierzTurniejowo(pulaRodzicielska, turniejRozmiar, los);

                    double dx1 = (los.NextDouble() * 2.0 - 1.0 * mutacjaPoziom);
                    double dx2 = (los.NextDouble() * 2.0 - 1.0 * mutacjaPoziom);

                    double noweX1 = FunkcjePrzystosowania.PrzytnijDoZakresu(zwyciezcaTurnieju.X1 + dx1, xMin, xMax);
                    double noweX2 = FunkcjePrzystosowania.PrzytnijDoZakresu(zwyciezcaTurnieju.X1 + dx1, xMin, xMax);

                    pulaPotomna.Add(new Osobnik(noweX1, noweX2));
                }

                List<Osobnik> polaczonaPula = new List<Osobnik>();
                for (int i = 0; i < pulaRodzicielska.Count; i++) polaczonaPula.Add(pulaRodzicielska[i]);
                for (int i = 0; i < pulaPotomna.Count; i++) polaczonaPula.Add(pulaPotomna[i]);

                SortujMalejaco(polaczonaPula);

                List<Osobnik> nowaPulaRodzicielska = new List<Osobnik>();
                for(int i =0; i<mu; i++)
                {
                    nowaPulaRodzicielska.Add(polaczonaPula[i]);
                }

                historia.Add(ZbudujStan(iter, nowaPulaRodzicielska, pulaPotomna));
                pulaRodzicielska = nowaPulaRodzicielska;
            }

            return historia;
        }


        private void SortujMalejaco(List<Osobnik> pula)
        {
            pula.Sort((a, b) => b.Ocena.CompareTo(a.Ocena));
        }


        private StanIteracji ZbudujStan(int iter,  List<Osobnik> rodzice, List<Osobnik> potomkowie)
        {
            StanIteracji stan = new StanIteracji { NumerIteracji = iter, NajlepszyOsobnik = rodzice[0].Klonuj() };
            for (int i = 0; i<rodzice.Count; i++)
            {
                stan.PulaGlowna.Add(rodzice[i].Klonuj());
            }
            for (int i = 0; i < potomkowie.Count; i++)
            {
                stan.PulaPotomna.Add(potomkowie[i].Klonuj());
            }

            StringWriter sw = new StringWriter();
            sw.WriteLine($"=== STRATEGIA mu + lambda (ITERACJA {iter}) ===");
            sw.WriteLine($"Najlepszy wynik F(x1, x2): {rodzice[0].Ocena:F5}");
            sw.WriteLine($"Wspolrzedne: x1 = {rodzice[0].X1:F4}, x2 = {rodzice[0].X2:F4}");

            sw.WriteLine($"\nPula rodzicielska (mu = {rodzice.Count}):");

            for (int i = 0; i < rodzice.Count; i++)
            {
                sw.WriteLine($"  R{i + 1}: [{rodzice[i].X1:F2}; {rodzice[i].X2:F2}] -> F = {rodzice[i].Ocena:F4}");
            }

            if (potomkowie.Count > 0)
            {
                sw.WriteLine($"\nPula potomna w tej iteracji (lambda = {potomkowie.Count}):");

                for (int i = 0; i < potomkowie.Count; i++)
                {
                    sw.WriteLine($"  P{i + 1}: [{potomkowie[i].X1:F2}; {potomkowie[i].X2:F2}] -> F = {potomkowie[i].Ocena:F4}");
                }
            }
            stan.TekstRaportu = sw.ToString();
            return stan;
        }

        private Osobnik WybierzTurniejowo(List<Osobnik> pula, int turniejRozmiar, Random los)
        {
            Osobnik najlepszy = null;
            
            for (int t = 0; t < turniejRozmiar; t++)
            {
                int losowyIdx = los.Next(0, pula.Count);
                Osobnik kandydat = pula[losowyIdx];

                if (najlepszy == null || kandydat.Ocena > najlepszy.Ocena)
                {
                    najlepszy = kandydat;
                }
            }
            return najlepszy;
        }
    }
}
    
