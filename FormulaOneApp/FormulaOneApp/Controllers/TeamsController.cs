using FormulaOneApp.Data;
using FormulaOneApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormulaOneApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TeamsController : ControllerBase
{
    private static AppDbContext _appContext;

    public TeamsController(AppDbContext appContext)
    {
        _appContext = appContext;
    }

    [HttpGet]
    [Route("teams")]
    public async Task<IActionResult> Get()
    {
        List<Team> teams = await _appContext.Teams.ToListAsync();
        return Ok(teams);
    }

    [HttpGet("team/{id}")]
    public async Task<IActionResult> Get(int id)
    {
        List<Team> teams = await _appContext.Teams.ToListAsync();
        var team = teams.FirstOrDefault(t => t.Id == id);
        if (team == null) return BadRequest("invalid id");
        return Ok(team);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Team team)
    {
        await _appContext.Teams.AddAsync(team);
        await _appContext.SaveChangesAsync();
        return CreatedAtAction("Get", team.Id, team);
    }

    [HttpPost("AddPilot/{teamId}")]
    public async Task<IActionResult> AddPilot(int teamId, [FromBody] Pilot pilot)
    {
        List<Team> teams = await _appContext.Teams.ToListAsync();
        Team? team = teams.FirstOrDefault(t => t.Id == teamId);

        await _appContext.Pilots.AddAsync(pilot);
        team.Pilots.Add(pilot);
        await _appContext.SaveChangesAsync();
        return Ok(team);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Team team)
    {
        Team teamToModify = await _appContext.Teams.FirstOrDefaultAsync(t => t.Id == id);
        if (teamToModify == null) return BadRequest("invalid id");
        teamToModify.Country = team.Country;
        teamToModify.TeamPrinciple = team.TeamPrinciple;
        teamToModify.Name = team.Name;
        await _appContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        Team team = await _appContext.Teams.FirstOrDefaultAsync(t => t.Id == id);
        if (team == null) return BadRequest("invalid id");
        _appContext.Teams.Remove(team);
        await _appContext.SaveChangesAsync();
        return NoContent();
    }
}
