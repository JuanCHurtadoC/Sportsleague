using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

[ApiController]
[Route("api/match/{matchId}/lineup")]
public class MatchLineupController : ControllerBase
{
    private readonly IMatchLineupService _service;
    private readonly IMapper _mapper;

    public MatchLineupController(
        IMatchLineupService service,
        IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MatchLineupResponseDTO>>
        Create(int matchId, MatchLineupRequestDTO dto)
    {
        try
        {
            var lineup = _mapper.Map<MatchLineup>(dto);

            var created = await _service
                .AddPlayerAsync(matchId, lineup);

            var result = await _service
                .GetByMatchAsync(matchId);

            var createdLineup = result
                .First(x => x.Id == created.Id);

            return Created("",
                _mapper.Map<MatchLineupResponseDTO>(createdLineup));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>>
        GetAll(int matchId)
    {
        try
        {
            var lineup = await _service
                .GetByMatchAsync(matchId);

            return Ok(
                _mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineup));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("team/{teamId}")]
    public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>>
        GetByTeam(int matchId, int teamId)
    {
        try
        {
            var lineup = await _service
                .GetByMatchAndTeamAsync(matchId, teamId);

            return Ok(
                _mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineup));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(
        int matchId,
        int id)
    {
        try
        {
            await _service.DeleteAsync(id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}