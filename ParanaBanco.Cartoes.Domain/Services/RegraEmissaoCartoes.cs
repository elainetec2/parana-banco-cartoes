using System;
using System.Collections.Generic;
using System.Text;

namespace ParanaBanco.Cartoes.Domain.Services
{
    public static class RegraEmissaoCartoes
    {
        public static ResultadoEmissao Avaliar(int score)
        {
            if (score < 0 || score > 1000)
                throw new ArgumentOutOfRangeException(
                    nameof(score),
                    "O score deve estar entre 0 e 1000.");

            if (score <= 100)
                return new ResultadoEmissao(
                    Quantidade: 0,
                    LimitePorCartao: 0m);

            if (score <= 500)
                return new ResultadoEmissao(
                    Quantidade: 1,
                    LimitePorCartao: 1000m);

            return new ResultadoEmissao(
                Quantidade: 2,
                LimitePorCartao: 5000m);
        }
    }

    public sealed record ResultadoEmissao(
        int Quantidade,
        decimal LimitePorCartao);
}
