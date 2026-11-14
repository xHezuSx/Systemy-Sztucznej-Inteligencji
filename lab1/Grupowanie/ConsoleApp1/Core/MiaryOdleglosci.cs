using System;

namespace SSI_Lab1_Grupowanie.Core
{
    public enum TypMetryki
    {
        Euklidesowa = 0,
        TylkoX1 = 1,
        Manhattan = 2, // weź to taxi  już i do mnie wróć ja czekam na Ciebie
        Czybyszewa = 3,
        Minkowskiego =4,
    }


    public static class MiaryOdleglosci
    {
        public static double Policz(double a1, double a2, double b1, double b2, TypMetryki metryka, double mink = 3.0)
        {
            double dx = Math.Abs(a1 - b1);
            double dy = Math.Abs(a2 - b2);

            switch (metryka)
            {
                case TypMetryki.TylkoX1:
                    return dx;
                case TypMetryki.Manhattan:
                    return dx + dy; ;
                case TypMetryki.Czybyszewa:
                    return Math.Max(dx, dy);
                case TypMetryki.Minkowskiego:
                    return Math.Pow(Math.Pow(dx, mink) + Math.Pow(dy, mink), 1.0 / mink);
                case TypMetryki.Euklidesowa:
                default:
                    return Math.Sqrt(dx*dx + dy*dy);

            }
        }
    }
}