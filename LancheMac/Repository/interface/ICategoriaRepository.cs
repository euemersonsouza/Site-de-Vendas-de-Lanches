using LancheMac.Models;
using System.Collections.Generic;

namespace LancheMac.Repository
{
    public interface ICategoriaRepository
    {
        IEnumerable<Categoria> Categorias { get; }
    }
}
