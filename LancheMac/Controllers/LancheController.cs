using LancheMac.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace LancheMac.Controllers
{
    public class LancheController : Controller
    {
        private readonly ILancheRepository _lancheRepository;

        public LancheController(ILancheRepository lancheRepository)
        {
            _lancheRepository = lancheRepository;
        }

        public IActionResult List(string filtro)
        {
            var lanches = _lancheRepository.Lanches;
            ViewData["filtro"] = filtro;
            ViewData["filtro"] = filtro;
            if (!String.IsNullOrEmpty(filtro))
            {
                lanches = lanches.Where(s => s.Nome.Contains(filtro)
                                       || s.DescricaoDetalhada.Contains(filtro));
            }
            return View(lanches);
        }
    }
}
