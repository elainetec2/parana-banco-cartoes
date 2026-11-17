using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParanaBanco.Cartoes.Infrastructure.Persistence;

namespace ParanaBanco.Cartoes.API.Controllers
{
    [ApiController]
    [Route("api/v1/cartoes")]
    public sealed class CartoesController : ControllerBase
    {
        private readonly CartoesDbContext _dbContext;

        public CartoesController(CartoesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{clienteId:guid}")]
        public async Task<IActionResult> ObterPorClienteId(
            Guid clienteId,
            CancellationToken cancellationToken)
        {
            var cartoes = await _dbContext.Cartoes
                .AsNoTracking()
                .Where(c => c.ClienteId == clienteId)
                .OrderBy(c => c.EmitidoEm)
                .Select(c => new
                {
                    c.Id,
                    c.ClienteId,
                    c.PropostaId,
                    c.Limite,
                    c.EmitidoEm
                })
                .ToListAsync(cancellationToken);

            return Ok(cartoes);
        }
    }
}
