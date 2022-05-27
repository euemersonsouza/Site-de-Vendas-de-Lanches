using LancheMac.Models;
using System.Collections.Generic;

namespace LancheMac.Repository
{
    public interface ILancheRepository
    {
       IEnumerable<Lanche>Lanches { get; }
        IEnumerable<Lanche> LanchesPreferidos { get; }
        Lanche GetLancheById(int LancheId);
    }
}
