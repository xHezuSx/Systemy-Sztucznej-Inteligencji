using System;

namespace SSI_Lab2_Ewolucyjne.Core
{
    public static class FunkcjePrzystosowania
    {
        public static double Oblicz1D(double x)
        {
            return Math.Sin(x / 10.0) * Math.Sin(x / 200.0);
        }

        public static double Oblicz2D(double x1, double x2)
        {
            return Math.Sin(x1 * 0.05) + Math.Sin(x2 * 0.05) + 0.4 * Math.Sin(x1 * 0.15) * Math.Sin(x2 * 0.15);
        }

        public static double PrzytnijDoZakresu(double wartosc, double min = 0.0, double max = 100.0)
        {
            if (wartosc < min) return min;
            if (wartosc > max) return max;
            return wartosc;
        }
    }
}