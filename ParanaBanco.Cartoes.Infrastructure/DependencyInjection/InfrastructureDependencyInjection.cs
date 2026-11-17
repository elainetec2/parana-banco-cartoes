using System;
using System.Collections.Generic;
using System.Text;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParanaBanco.Cartoes.Infrastructure.Consumers;
using ParanaBanco.Cartoes.Infrastructure.Persistence;

namespace ParanaBanco.Cartoes.Infrastructure.DependencyInjection
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<CartoesDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("CartoesDb")));

            services.AddMassTransit(x =>
            {
                x.AddConsumer<PropostaGeradaConsumer>();

                x.AddEntityFrameworkOutbox<CartoesDbContext>(options =>
                {
                    options.UseSqlServer();
                    options.UseBusOutbox();
                });

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ReceiveEndpoint("cartoes-proposta-gerada", e =>
                    {
                        e.UseMessageRetry(r =>
                            r.Intervals(
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(3),
                                TimeSpan.FromSeconds(5)));

                        e.UseEntityFrameworkOutbox<CartoesDbContext>(
                            context);

                        e.ConfigureConsumer<PropostaGeradaConsumer>(
                            context);
                    });
                });
            });

            return services;
        }
    }
}
