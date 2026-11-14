using System.Collections.Generic;

namespace SSI_Lab1_Grupowanie.Core
{
    public interface IAlgorytmGrupowania
    {
        List<StanIteracji> Wykonaj(DaneProbek dane, int m, int iters, TypMetryki metryka, int seed = 123);
    }
}