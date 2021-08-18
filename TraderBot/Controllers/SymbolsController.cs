using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TraderBot.Models;

namespace TraderBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SymbolsController : ControllerBase
    {
        private readonly TradingContext context;

        public SymbolsController(TradingContext context)
        {
            this.context = context;
        }

        [HttpGet()]
        public async Task<IEnumerable<Symbol>> GetSymbols() 
        {
            return await context.Symbols.ToListAsync();
        }
    }
}
