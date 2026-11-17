using System;
using System.Collections.Generic;
using System.Text;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using ParanaBanco.Cartoes.Domain.Entities;

namespace ParanaBanco.Cartoes.Infrastructure.Persistence
{
    public sealed class CartoesDbContext : DbContext
    {
        public CartoesDbContext(
            DbContextOptions<CartoesDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cartao> Cartoes => Set<Cartao>();

        public DbSet<InboxState> InboxState
            => Set<InboxState>();

        public DbSet<OutboxMessage> OutboxMessage
            => Set<OutboxMessage>();

        public DbSet<OutboxState> OutboxState
            => Set<OutboxState>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tabelas de controle do MassTransit
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            // Tabela de cartões
            modelBuilder.Entity<Cartao>(entity =>
            {
                entity.ToTable("Cartoes");

                entity.HasKey(c => c.Id);

                entity.HasIndex(c => c.ClienteId);

                entity.HasIndex(c => c.PropostaId);

                entity.Property(c => c.Limite)
                    .HasPrecision(18, 2);

                entity.Property(c => c.EmitidoEm)
                    .IsRequired();
            });
        }
    }
}
