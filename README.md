# Paraná Banco — Microsserviço de Cartões de Crédito

Microsserviço responsável pela emissão e consulta de cartões de crédito, desenvolvido em **.NET 10**, utilizando **RabbitMQ, MassTransit, Entity Framework Core e SQL Server**.

Este projeto faz parte de uma solução composta por três microsserviços independentes: Clientes, Propostas e Cartões.

## 1. Responsabilidades

- Consumir o evento `PropostaGerada` publicado pelo microsserviço de Propostas.
- Avaliar as regras de emissão de cartões conforme o score de crédito.
- Persistir os cartões emitidos em banco de dados próprio.
- Disponibilizar uma API REST para consultar os cartões de um cliente.
- Aplicar mecanismos de retry e Consumer Outbox no processamento de mensagens.

## 2. Regras de negócio

| Score | Situação | Quantidade de cartões | Limite por cartão |
|---|---|---|---|
| 0–100 | Negado | 0 | — |
| 101–500 | Aprovado | 1 | R$ 1.000 |
| 501–1000 | Aprovado | 2 | R$ 5.000 |

Na faixa de 501 a 1000, a implementação emite dois cartões, respeitando o máximo permitido pelo desafio.

Cada cartão possui identificador próprio, referência ao cliente e à proposta, limite individual e data de emissão.

## 3. Arquitetura

A solução utiliza separação em camadas:

- **Domain:** entidade `Cartao` e regras de emissão.
- **Application:** contrato do evento `PropostaGerada`.
- **Infrastructure:** consumidor MassTransit, persistência EF Core, migrations e configuração do RabbitMQ.
- **API:** endpoint HTTP para consulta de cartões.
- **UnitTests:** testes unitários das regras de negócio com xUnit.

O microsserviço mantém seu próprio banco de dados e não consulta diretamente os bancos de Clientes ou Propostas.

## 4. Tecnologias

- .NET 10 / ASP.NET Core
- C#
- Entity Framework Core 10
- SQL Server LocalDB
- RabbitMQ 4
- MassTransit 8.5.7
- xUnit
- Docker Desktop, para execução do RabbitMQ

## 5. Pré-requisitos

Para executar localmente no Windows:

- .NET SDK 10
- SQL Server LocalDB
- Docker Desktop com Docker Compose
- RabbitMQ em execução
- Ferramenta `dotnet-ef` compatível com EF Core 10

O RabbitMQ pode ser iniciado pelo `docker-compose.yml` disponível no repositório do microsserviço de Clientes.

```powershell
docker compose up -d
```

O RabbitMQ Management fica disponível em `http://localhost:15672`, com credenciais locais `guest` / `guest`.

## 6. Configuração

A conexão com o banco de dados é definida em `ParanaBanco.Cartoes.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "CartoesDb": "Server=(localdb)\\MSSQLLocalDB;Database=ParanaBancoCartoes;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

A configuração do RabbitMQ utiliza `localhost:5672`, virtual host `/` e credenciais locais `guest` / `guest`.

Essas configurações são destinadas ao ambiente de desenvolvimento. Em outros ambientes, as credenciais e conexões devem ser fornecidas por configuração externa ou variáveis de ambiente.

## 7. Como executar

Na raiz do repositório, restaure as dependências:

```powershell
dotnet restore ParanaBanco.Cartoes.slnx
```

Aplique as migrations:

```powershell
dotnet ef database update --project ParanaBanco.Cartoes.Infrastructure --startup-project ParanaBanco.Cartoes.API --context CartoesDbContext
```

Inicie a API:

```powershell
dotnet run --project ParanaBanco.Cartoes.API
```

No perfil HTTP utilizado durante o desenvolvimento, a API fica disponível em:

`http://localhost:5224`

O serviço deve estar conectado ao RabbitMQ para consumir os eventos publicados pelo microsserviço de Propostas.

## 8. Endpoint REST

### GET /api/v1/cartoes/{clienteId}

Consulta todos os cartões emitidos para um cliente.

Exemplo:

```http
GET http://localhost:5224/api/v1/cartoes/70cfa10e-5d17-4494-93f2-b2c9ee1b0a5f
```

Resposta de exemplo:

```json
[
  {
    "id": "e8c76db8-d1c2-4dae-833d-30bf625902cf",
    "clienteId": "70cfa10e-5d17-4494-93f2-b2c9ee1b0a5f",
    "propostaId": "83afcca5-cd06-4260-bf5a-df779474ad10",
    "limite": 1000.00,
    "emitidoEm": "2026-11-17T17:25:35.365863"
  }
]
```

Quando não existem cartões para o cliente, a API retorna HTTP 200 com uma lista vazia.

## 9. Mensageria e resiliência

O microsserviço consome o contrato `ParanaBanco.Propostas.Application.Events.PropostaGerada`.

Fila de consumo:

`cartoes-proposta-gerada`

Em caso de falha transitória, o MassTransit realiza três tentativas adicionais, com intervalos configurados de 1, 3 e 5 segundos.

Após esgotar as tentativas, mensagens com falha podem ser encaminhadas para:

`cartoes-proposta-gerada_error`

O Consumer Outbox utiliza tabelas `InboxState`, `OutboxMessage` e `OutboxState` para apoiar o controle transacional e o processamento idempotente das mensagens.

O consumidor também verifica a existência de cartões associados à proposta antes de realizar novas emissões. Essa verificação complementa o mecanismo de Inbox, mas não substitui uma restrição transacional de unicidade em cenários concorrentes.

## 10. Testes

Execute:

```powershell
dotnet test ParanaBanco.Cartoes.slnx
```

Os testes unitários verificam as faixas de score, os limites de crédito e a rejeição de scores inválidos.

Na validação local realizada em 17/11/2026, os 10 testes existentes passaram.

## 11. Integração com os demais microsserviços

O fluxo completo é:

1. Clientes recebe o cadastro e publica `ClienteCadastrado`.
2. Propostas consome o evento, calcula o score e publica `PropostaGerada`.
3. Cartões consome `PropostaGerada`, emite os cartões conforme a regra e persiste os registros.
4. O endpoint GET permite consultar os cartões emitidos.

Repositórios relacionados:

- [Cadastro de Clientes](https://github.com/elainetec2/parana-banco-clientes)
- [Propostas de Crédito](https://github.com/elainetec2/parana-banco-propostas)

## 12. Decisões técnicas e limitações

- Cada microsserviço possui banco de dados próprio.
- A comunicação entre serviços é assíncrona e baseada em eventos.
- O serviço de Cartões não precisa referenciar diretamente o projeto de Propostas, pois possui uma definição local compatível do contrato de evento.
- O ambiente local utiliza SQL Server LocalDB no Windows; o banco de dados não está containerizado.
- A emissão de dois cartões para scores acima de 500 é uma escolha de implementação dentro do máximo permitido.
- A resiliência utiliza retry e Consumer Outbox; testes automatizados de falhas concorrentes e de recuperação ainda podem ser ampliados.

## 13. Repositório

[GitHub — Paraná Banco Cartões](https://github.com/elainetec2/parana-banco-cartoes)
