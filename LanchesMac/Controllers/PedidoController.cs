using LanchesMac.Models;
using LanchesMac.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net.Mail;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using static System.Net.WebRequestMethods;
using LanchesMac.Services;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace LanchesMac.Controllers
{
    public class PedidoController : Controller
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly CarrinhoCompra _carrinhoCompra;
        private readonly IEmailSendercs _emailSendercs;

        public PedidoController(IPedidoRepository pedidoRepository, CarrinhoCompra carrinhoCompra, IEmailSendercs emailSendercs)
        {
            _pedidoRepository = pedidoRepository;
            _carrinhoCompra = carrinhoCompra;
            _emailSendercs = emailSendercs;
        }
        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public IActionResult Checkout(Pedido pedido)
        {
            int totalItensPedido = 0;
            decimal precoTotalPedido = 0.0m;

            //obtem os itens do carrinho de compra do cliente
            List<CarrinhoCompraItem> items = _carrinhoCompra.GetCarrinhoCompraItens();
            _carrinhoCompra.CarrinhoCompraItems = items;

            //verifica se existem itens de pedido
            if (_carrinhoCompra.CarrinhoCompraItems.Count == 0)
            {
                ModelState.AddModelError("", "Seu carrinho esta vazio, que tal incluir um lanche...");
            }

            //calcula o total de itens e o total do pedido
            foreach (var item in items)
            {
                totalItensPedido += item.Quantidade;
                precoTotalPedido += (item.Lanche.Preco * item.Quantidade);
            }

            //atribui os valores obtidos ao pedido
            pedido.TotalItensPedido = totalItensPedido;
            pedido.PedidoTotal = precoTotalPedido;

            //valida os dados do pedido
            if (ModelState.IsValid)
            {
                //cria o pedido e os detalhes
                _pedidoRepository.CriarPedido(pedido);

                //define mensagens ao cliente
                ViewBag.CheckoutCompletoMensagem = "Obrigado pelo seu pedido :)";
                ViewBag.TotalPedido = _carrinhoCompra.GetCarrinhoCompraTotal();
                EnviarEmailCheckout(items, pedido, pedido.Email);

                //limpa o carrinho do cliente
                _carrinhoCompra.LimparCarrinho();

                //exibe a view com dados do cliente e do pedido
                return View("~/Views/Pedido/CheckoutCompleto.cshtml", pedido);
            }
            return View(pedido);
        }

        private string substituiTextosEmail(List<CarrinhoCompraItem> items, Pedido p, string assunto)
        {
            string textoAlterado = assunto;

            if (assunto.Contains("[PEDIDOID]"))
            {
                textoAlterado = textoAlterado.Replace("[PEDIDOID]", p.PedidoId.ToString());
            }
            if (assunto.Contains("[PRECOTOTAL]"))
            {
                textoAlterado = textoAlterado.Replace("[PRECOTOTAL]", p.PedidoTotal.ToString("C", CultureInfo.CurrentCulture));
            }
            if (assunto.Contains("[NOMECLIENTE]"))
            {
                textoAlterado = textoAlterado.Replace("[NOMECLIENTE]", p.Nome.ToString());
            }
            if (assunto.Contains("[SOBRENOMECLIENTE]"))
            {
                textoAlterado = textoAlterado.Replace("[SOBRENOMECLIENTE]", p.Sobrenome.ToString());
            }
            if (assunto.Contains("[DTPEDIDOENVIADO]"))
            {
                textoAlterado = textoAlterado.Replace("[DTPEDIDOENVIADO]", p.PedidoEnviado.ToString());
            }
            if (assunto.Contains("[ITENS]"))
            {
                string textoItens = @"<tr>
                                            <td><img src=""[IMGURL]"" width=""40"" height=""40"" /></td>
                                            <td style=""text-align: center"">[QTD]</td>
                                            <td>[NOMELANCHE]</td>
                                            <td>[DESCRICAOLANCHE]</td>
                                            <td>[PRECOLANCHE]</td>
                                          </tr> ";
                string textoAlteradoItens = "";
                foreach (var item in items)
                {
                    textoAlteradoItens = textoAlteradoItens + textoItens;

                    textoAlteradoItens = textoAlteradoItens.Replace("[IMGURL]", item.Lanche.ImagemUrl.ToString());
                    textoAlteradoItens = textoAlteradoItens.Replace("[QTD]", item.Quantidade.ToString());
                    textoAlteradoItens = textoAlteradoItens.Replace("[NOMELANCHE]", item.Lanche.Nome.ToString());
                    textoAlteradoItens = textoAlteradoItens.Replace("[DESCRICAOLANCHE]", item.Lanche.DescricaoDetalhada.ToString());
                    textoAlteradoItens = textoAlteradoItens.Replace("[PRECOLANCHE]", item.Lanche.Preco.ToString("C", CultureInfo.CurrentCulture));
                    //textoAlteradoItens = textoAlteradoItens.Replace("[QTD]", gerador.quantidade.ToString());
                    //textoAlteradoItens = textoAlteradoItens.Replace("[TOTAL]", gerador.totalItem.ToString("#,##0.00"));

                }

                textoAlterado = textoAlterado.Replace("[ITENS]", textoAlteradoItens);

            }

            return textoAlterado;
        }
        public void EnviarEmailCheckout(List<CarrinhoCompraItem> items, Pedido p, string email)
        {
            try
            {
                string corpo = "";
                string assunto = "";
                string caminho = @"C:/ModeloEmail/Index.html";
                assunto = "Hobs Lanches- Pagamento do pedido [PEDIDOID] efetuado com sucesso!";
                corpo = System.IO.File.ReadAllText(caminho);                
                assunto = this.substituiTextosEmail(items, p, assunto);
                corpo = this.substituiTextosEmail(items, p, corpo);
                _emailSendercs.SendEmailAsync(email, assunto, corpo);               
            }
            catch (Exception ex)
            {
                Console.Write("Ocorreram problemas no envio do e-mail. " + ex.ToString());
            }
        }

    }
}
