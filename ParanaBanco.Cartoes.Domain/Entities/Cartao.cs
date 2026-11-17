using System;
using System.Collections.Generic;
using System.Text;

namespace ParanaBanco.Cartoes.Domain.Entities
{
    public sealed class Cartao
    {
        public Guid Id { get; private set; }
        public Guid ClienteId { get; private set; }
        public Guid PropostaId { get; private set; }
        public decimal Limite { get; private set; }
        public DateTime EmitidoEm { get; private set; }

        private Cartao() { }

        public Cartao(
            Guid clienteId,
            Guid propostaId,
            decimal limite)
        {
            if (clienteId == Guid.Empty)
                throw new ArgumentException(
                    "O identificador do cliente é obrigatório.",
                    nameof(clienteId));

            if (propostaId == Guid.Empty)
                throw new ArgumentException(
                    "O identificador da proposta é obrigatório.",
                    nameof(propostaId));

            if (limite <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(limite),
                    "O limite do cartão deve ser maior que zero.");

            Id = Guid.NewGuid();
            ClienteId = clienteId;
            PropostaId = propostaId;
            Limite = limite;
            EmitidoEm = DateTime.UtcNow;
        }
    }
}
