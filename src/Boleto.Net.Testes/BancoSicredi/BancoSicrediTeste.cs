﻿using BoletoNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Boleto.Net.Testes.BancoSicredi
{
    [TestClass]
    public class BancoSicrediTeste
    {
        [TestMethod]
        public void ValidarBoleto()
        {
            BoletoBancario boletoBancario = GerarBoleto();
            boletoBancario.Boleto.Valida();
        }

        [TestMethod]
        public void GerarRemessaCNAB400()
        {
            var boletos = Enumerable.Range(0, 3).Select(o => GerarBoleto());

            Boletos itensRemessa = new Boletos();
            itensRemessa.AddRange(boletos.Select(o => o.Boleto));

            var banco = itensRemessa.First().Banco;
            var cedente = itensRemessa.First().Cedente;

            ArquivoRemessa arquivoRemessa = new ArquivoRemessa(TipoArquivo.CNAB400);
            arquivoRemessa.LinhaDeArquivoGerada += (object sender, LinhaDeArquivoGeradaArgs e) =>
            {
                Debug.WriteLine(e.Linha);
            };

            using (var stream = new MemoryStream())
            {                
                arquivoRemessa.GerarArquivoRemessa("08111081111", banco, cedente, itensRemessa, stream, 1);
                var conteudo = Encoding.ASCII.GetString(stream.ToArray());
                Debug.WriteLine(conteudo);
            }
        }

        private static BoletoBancario GerarBoleto()
        {
            Thread.Sleep(500);
            DateTime vencimento = DateTime.Now.AddDays(5);

            var agencia = "0811";
            var conta = "81111";

            var cedente = new Cedente("35.683.343/0001-82", "Empresa Teste", agencia, string.Empty, conta, "0");
            cedente.Codigo = "08111081111";
            
            BoletoNet.Boleto boleto = new BoletoNet.Boleto(vencimento, GerarValor(), "1", GerarNossoNumero(), cedente);

            boleto.NumeroDocumento = GerarNumero();
            boleto.DataDocumento = DateTime.Now.AddDays(-15);
            boleto.DataProcessamento = DateTime.Now;

            boleto.Remessa = new Remessa(TipoOcorrenciaRemessa.EntradaDeTitulos);
            //boleto.EspecieDocumento = new EspecieDocumento_Sicredi("A");

            boleto.Sacado = new Sacado("87425264188", "Sacado teste", new Endereco()
            {
                CEP = "78945612",
                Cidade = "Teste",
                End = "End teste",
            });

            var boletoBancario = new BoletoBancario();
            boletoBancario.CodigoBanco = 748;
            boletoBancario.Boleto = boleto;

            return boletoBancario;
        }

        private static decimal GerarValor()
        {
            Random r = new Random();
            int rInt = r.Next(0, 100);
            int range = 100;
            return Convert.ToDecimal(r.NextDouble() * range);
        }

        private static string GerarNumero()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            return rnd.Next(0, 99999).ToString();
        }

        private static string GerarNossoNumero()
        {
            var prefix = DateTime.Now.Year.ToString().Substring(2) + "20";

            var rnd = new Random(DateTime.Now.Millisecond);
            return prefix + rnd.Next(0, 9999).ToString();
        }
    }
}
