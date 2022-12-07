using LanchesMac.Models;
using LanchesMac.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net.Mail;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using static System.Net.WebRequestMethods;
using LanchesMac.Services;

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

        [HttpGet]
        public IActionResult Checkout()
        {
            return View();
        }
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
            if (assunto.Contains("[ITENS]"))
            {
                string textoItens = @"<html>
                                <head>
                                <style>
                                table, th, td {
                                    border: 1px solid white;
                                    border-collapse: collapse;
                                }
                                th, td {
                                    background-color: #96D4D4;
                                }
                                </style>
                                </head>
                                <body>

                                <table style=""width:100%"">
                                      <tr>
                                        <th>Nome</th>
                                        <th>Descrição</th> 
                                        <th>Preço</th>
                                      </tr>
                                      <tr>
                                        <td>[NOMELANCHE]</td>
                                        <td>[DESCRICAOLANCHE]</td>
                                        <td>[PRECOTOTAL]</td>
                                      </tr>                              
                            </table>
                            </body>
                            </html>";
                string textoAlteradoItens = "";
                foreach (var item in items)
                {
                    textoAlteradoItens = textoAlteradoItens + textoItens;

                    //textoAlteradoItens = textoAlteradoItens.Replace("[QTD]", item.Lanche.ImagemUrl.ToString());
                    textoAlteradoItens = textoAlteradoItens.Replace("[NOMELANCHE]", item.Lanche.Nome.ToString());
                    textoAlteradoItens = textoAlteradoItens.Replace("[DESCRICAOLANCHE]", item.Lanche.DescricaoDetalhada.ToString());
                    textoAlteradoItens = textoAlteradoItens.Replace("[PRECOTOTAL]", p.PedidoTotal.ToString("#,##0.00"));
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
