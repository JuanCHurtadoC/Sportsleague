using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
using SportsLeague.Domain.Enums;

namespace SportsLeague.Domain.Services;

public class SponsorService : ISponsorService
{
    private readonly ISponsorRepository _sponsorRepository;
    private readonly ILogger<SponsorService> _logger;

    public SponsorService(ISponsorRepository sponsorRepository, ILogger<SponsorService> logger, 
        ITournamentSponsorRepository tournamentSponsorRepository, ITournamentRepository tournamentRepository)
    {
        _sponsorRepository = sponsorRepository;
        _logger = logger;

    }

    public async Task<IEnumerable<Sponsor>> GetAllAsync() 
    {
        _logger.LogInformation("Retrieving all Sponsors");
        return await _sponsorRepository.GetAllAsync();
    }

    public async Task<Sponsor?> GetByIdAsync(int id) 
    {
        _logger.LogInformation("Retrieving Sponsor with ID: {SponsorId}", id);
        var sponsor = await _sponsorRepository.GetByIdAsync(id);
        if (sponsor == null)
            _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);
        return sponsor;
    }

    public async Task<Sponsor> CreateAsync(Sponsor sponsor) 
    {
        // Validación de negocio: nombre único
        var existingSponsor = await _sponsorRepository.GetByNameAsync(sponsor.SponsorName);
        if (existingSponsor != null)
        {
            _logger.LogWarning("Sponsor with name '{SponsorName}' already exists", sponsor.SponsorName);
            throw new InvalidOperationException(
                $"Ya existe un patrocinador con el nombre '{sponsor.SponsorName}'");
        }
        _logger.LogInformation("Creating Sponsor: {SponsorName}", sponsor.SponsorName);
        return await _sponsorRepository.CreateAsync(sponsor);
    }

    public async Task UpdateAsync(int id, Sponsor sponsor) 
    {
        var existingSponsor = await _sponsorRepository.GetByIdAsync(id);
        if (existingSponsor == null)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found for update", id);
            throw new KeyNotFoundException(
                $"No se encontró el patrocinador con ID {id}");
        }
        // Validar nombre único (si cambió)
        if (existingSponsor.SponsorName != sponsor.SponsorName)
        {
            var sponsorWithSameName = await _sponsorRepository.GetByNameAsync(sponsor.SponsorName);
            if (sponsorWithSameName != null)
            {
                throw new InvalidOperationException(
                    $"Ya existe un patrocinador con el nombre '{sponsor.SponsorName}'");
            }
        }

        existingSponsor.SponsorName = sponsor.SponsorName;
        existingSponsor.ContactEmail = sponsor.ContactEmail;
        existingSponsor.Phone = sponsor.Phone;
        existingSponsor.WebsiteUrl = sponsor.WebsiteUrl;
        existingSponsor.Category = sponsor.Category;

        _logger.LogInformation("Updating Sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.UpdateAsync(existingSponsor);
    }

    public async Task DeleteAsync(int id) 
    {
        var existingSponsor = await _sponsorRepository.GetByIdAsync(id);
        if (existingSponsor == null)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found for deletion", id);
            throw new KeyNotFoundException(
                $"No se encontró el patrocinador con ID {id}");
        }
        _logger.LogInformation("Deleting Sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Sponsor>> GetByCategoryAsync(SponsorCategory category) 
    {
        _logger.LogInformation("Retrieving Sponsors with category: {Category}", category);
        return await _sponsorRepository.GetByCategoryAsync(category);
    }

    public async Task<Sponsor> GetByNameAsync(string sponsorName)
    {
        _logger.LogInformation("Retrieving Sponsor with name: {SponsorName}", sponsorName);
        var sponsor = await _sponsorRepository.GetByNameAsync(sponsorName);
        if (sponsor == null)
            _logger.LogWarning("Sponsor with name {SponsorName} not found", sponsorName);
        return sponsor;
    }
}

