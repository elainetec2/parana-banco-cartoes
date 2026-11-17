using System;
using System.Collections.Generic;
using System.Text;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ParanaBanco.Cartoes.Domain.Entities;
using ParanaBanco.Cartoes.Domain.Services;
using ParanaBanco.Cartoes.Infrastructure.Persistence;
using ParanaBanco.Propostas.Application.Events;

namespace ParanaBanco.Cartoes.Infrastructure.Consumers
{
    public sealed class PropostaGeradaConsumer
        : IConsumer<PropostaGerada>
    {
        private readonly CartoesDbContext _dbContext;

        public PropostaGeradaConsumer(CartoesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(
            ConsumeContext<PropostaGerada> context)
        {
            var evento = context.Message;

            // Verifica se a proposta já gerou cartões.
            var cartoesExistentes = await _dbContext.Cartoes
                .AnyAsync(
                    c => c.PropostaId == evento.PropostaId,
                    context.CancellationToken);

            if (cartoesExistentes)
                return;

            // Propostas negadas não geram cartões.
            if (!evento.Aprovada)
                return;

            // Aplica as regras de emissão.
            var resultado = RegraEmissaoCartoes.Avaliar(evento.Score);

            for (var i = 0; i < resultado.Quantidade; i++)
            {
                var cartao = new Cartao(
                    evento.ClienteId,
                    evento.PropostaId,
                    resultado.LimitePorCartao);

                _dbContext.Cartoes.Add(cartao);
            }

            // Persiste os cartões em uma única operação.
            await _dbContext.SaveChangesAsync(
                context.CancellationToken);
        }
    }
}
