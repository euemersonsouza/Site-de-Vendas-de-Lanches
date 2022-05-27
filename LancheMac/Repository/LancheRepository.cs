using LancheMac.Context;
using LancheMac.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace LancheMac.Repository
{
    public class LancheRepository : ILancheRepository
    {
        private readonly AppDbContext _context;
        public LancheRepository(AppDbContext contexto)
        {
              _context = contexto;
        }
        public IEnumerable<Lanche> Lanches => _context.Lanches.Include(c => c.Categoria);


        public IEnumerable<Lanche> LanchesPreferidos => _context.Lanches.
            Where(l => l.IsLanchePreferido)
            .Include(c => c.Categoria);

        public Lanche GetLancheById(int LancheId)
        {
           return _context.Lanches.FirstOrDefault(l => l.LancheId == LancheId);
        }
    }
}
