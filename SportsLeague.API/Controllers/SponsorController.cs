using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SponsorController : ControllerBase
{
    private readonly ISponsorService _SponsorService;
    private readonly IMapper _mapper;
    private readonly ILogger<SponsorController> _logger;

    public SponsorController(
        ISponsorService SponsorService,
        IMapper mapper,
        ILogger<SponsorController> logger)
    {
        _SponsorService = SponsorService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SponsorResponseDTO>>> GetAll()
    {
        var Sponsors = await _SponsorService.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<SponsorResponseDTO>>(Sponsors));
    }

    [HttpGet("{id}")]

    public async Task<ActionResult<SponsorResponseDTO>> GetById(int id)
    {
        var Sponsor = await _SponsorService.GetByIdAsync(id);
        if (Sponsor == null)
            return NotFound(new { message = $"Sponsor con ID {id} no encontrado" });
        return Ok(_mapper.Map<SponsorResponseDTO>(Sponsor));
    }

    [HttpPost]

    public async Task<ActionResult<SponsorResponseDTO>> Create(SponsorRequestDTO dto)
    {
        try
        {
            var Sponsor = _mapper.Map<Sponsor>(dto);
            var created = await _SponsorService.CreateAsync(Sponsor);
            var responseDto = _mapper.Map<SponsorResponseDTO>(created);
            return CreatedAtAction(nameof(GetById), new { id = responseDto.Id }, responseDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]

    public async Task<ActionResult<SponsorResponseDTO>> Update(int id, SponsorRequestDTO dto)
    {
        try
        {
            var Sponsor = _mapper.Map<Sponsor>(dto);
            await _SponsorService.UpdateAsync(id, Sponsor);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpDelete("{id}")]

    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _SponsorService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpGet("Name/{SponsorName}")]

    public async Task<ActionResult<SponsorResponseDTO>> GetByName(string SponsorName)
    {
        var Sponsor = await _SponsorService.GetByNameAsync(SponsorName);
        if (Sponsor == null)
            return NotFound(new { message = $"Sponsor con nombre {SponsorName} no encontrado" });
        return Ok(_mapper.Map<SponsorResponseDTO>(Sponsor));
    }

    [HttpPost("Register/{tournamentId}/{sponsorId}")]
    public async Task<ActionResult> RegisterSponsor(int tournamentId, int sponsorId, decimal contractAmount)
    {
        try
        {
            await _SponsorService.RegisterSponsorAsync(tournamentId, sponsorId, contractAmount);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpDelete("{TournamentId},{SponsorId}")]
    public async Task<ActionResult> UnregisterSponsor(int TournamentId, int SponsorId)
    {
        try
        {
            await _SponsorService.DeleteAsync(TournamentId, SponsorId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpGet("{id}/Sponsor")]
    public async Task<ActionResult<IEnumerable<TournamentSponsorResponseDTO>>> GetBySponsor(int id)
    {
        var tournamentSponsors = await _SponsorService.GetBySponsorAsync(id);
        if (tournamentSponsors == null || !tournamentSponsors.Any())
            return NotFound(new { message = $"No se encontraron torneos para el patrocinador con ID {id}" });
        return Ok(_mapper.Map<IEnumerable<TournamentSponsorResponseDTO>>(tournamentSponsors));
    }

    [HttpGet("Tournament/{tournamentId}")]
    public async Task<ActionResult<IEnumerable<TournamentSponsorResponseDTO>>> GetByTournament(int tournamentId)
    {
        var tournamentSponsors = await _SponsorService.GetByTournamentAsync(tournamentId);
        if (tournamentSponsors == null || !tournamentSponsors.Any())
            return NotFound(new { message = $"No se encontraron patrocinadores para el torneo con ID {tournamentId}" });
        return Ok(_mapper.Map<IEnumerable<TournamentSponsorResponseDTO>>(tournamentSponsors));


    }
}

