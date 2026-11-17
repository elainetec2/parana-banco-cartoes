using System;
using System.Collections.Generic;
using System.Text;
using ParanaBanco.Cartoes.Domain.Services;

namespace ParanaBanco.Cartoes.UnitTests
{
    public class RegraEmissaoCartoesTests
    {
        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(100, 0, 0)]
        [InlineData(101, 1, 1000)]
        [InlineData(500, 1, 1000)]
        [InlineData(501, 2, 5000)]
        [InlineData(567, 2, 5000)]
        [InlineData(1000, 2, 5000)]
        public void Avaliar_DeveRetornarQuantidadeELimiteCorretos(
            int score,
            int quantidadeEsperada,
            int limiteEsperado)
        {
            // Arrange
            var scoreInformado = score;

            // Act
            var resultado = RegraEmissaoCartoes.Avaliar(scoreInformado);

            // Assert
            Assert.Equal(quantidadeEsperada, resultado.Quantidade);
            Assert.Equal(limiteEsperado, resultado.LimitePorCartao);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(1001)]
        public void Avaliar_DeveRejeitarScoreForaDoIntervalo(int score)
        {
            // Arrange
            var scoreInvalido = score;

            // Act
            Action acao = () => RegraEmissaoCartoes.Avaliar(scoreInvalido);

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(acao);
        }
    }
}
