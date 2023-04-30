using FormulaOneApp.Data;
using FormulaOneApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormulaOneApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PilotController : ControllerBase
    {
        private static AppDbContext _appContext;

        public PilotController(AppDbContext appContext)
        {
            _appContext = appContext;
        }

        [HttpGet]
        [Route("pilots")]
        public async Task<IActionResult> Get()
        {
            List<Pilot> Pilots = await _appContext.Pilots.ToListAsync();
            return Ok(Pilots);
        }

        [HttpGet("pilot/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            List<Pilot> pilots = await _appContext.Pilots.ToListAsync();
            var pilot = pilots.FirstOrDefault(t => t.Id == id);
            if (pilot == null) return BadRequest("invalid id");
            return Ok(pilot);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Pilot pilot)
        {
            await _appContext.Pilots.AddAsync(pilot);
            await _appContext.SaveChangesAsync();
            return CreatedAtAction("Get", pilot.Id, pilot);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Pilot pilot)
        {
            Pilot pilotToModify = await _appContext.Pilots.FirstOrDefaultAsync(t => t.Id == id);
            if (pilotToModify == null) return BadRequest("invalid id");
            pilotToModify.Name = pilot.Name;
            await _appContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Pilot pilot = await _appContext.Pilots.FirstOrDefaultAsync(t => t.Id == id);
            if (pilot == null) return BadRequest("invalid id");
            _appContext.Pilots.Remove(pilot);
            await _appContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
