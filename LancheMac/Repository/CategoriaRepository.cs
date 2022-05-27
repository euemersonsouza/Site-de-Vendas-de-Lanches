using LancheMac.Context;
using LancheMac.Models;
using System.Collections.Generic;

namespace LancheMac.Repository
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Categoria> Categorias => throw new System.NotImplementedException();
    }
}
