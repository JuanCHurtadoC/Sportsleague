using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
using SportsLeague.Domain.Enums;
using System.Net.Mail;

namespace SportsLeague.Domain.Services;

public class SponsorService : ISponsorService
{
    private readonly ISponsorRepository _sponsorRepository;
    private readonly ILogger<SponsorService> _logger;
    private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private void ValidateEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);

            if (addr.Address != email)
            {
                throw new InvalidOperationException("El formato del email no es válido");
            }
        }
        catch
        {
            throw new InvalidOperationException("El formato del email no es válido");
        }
    }


    public SponsorService(ISponsorRepository sponsorRepository, ILogger<SponsorService> logger,
        ITournamentSponsorRepository tournamentSponsorRepository, ITournamentRepository tournamentRepository)
    {
        _sponsorRepository = sponsorRepository;
        _logger = logger;
        _tournamentSponsorRepository = tournamentSponsorRepository;
        _tournamentRepository = tournamentRepository;
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
    {   // Validar email
        ValidateEmail(sponsor.ContactEmail);
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
        // Validar email
        ValidateEmail(sponsor.ContactEmail);
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
        existingSponsor.SponsorCategory = sponsor.SponsorCategory;

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

    public async Task<TournamentSponsor?> GetByTournamentAndSponsorAsync(int tournamentId, int sponsorId)
    {
        _logger.LogInformation("Retrieving TournamentSponsor with Tournament ID: {TournamentId} and Sponsor ID: {SponsorId}", tournamentId, sponsorId);
        var tournamentSponsor = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
        if (tournamentSponsor == null)
            _logger.LogWarning("TournamentSponsor with Tournament ID {TournamentId} and Sponsor ID {SponsorId} not found", tournamentId, sponsorId);
        return tournamentSponsor;
    }

    public async Task<IEnumerable<TournamentSponsor>> GetByTournamentAsync(int TournamentId)
    {
        _logger.LogInformation("Retrieving TournamentSponsors for Tournament ID: {TournamentId}", TournamentId);
        return await _tournamentSponsorRepository.GetByTournamentAsync(TournamentId);
    }

    public async Task<IEnumerable<TournamentSponsor>> GetBySponsorAsync(int SponsorId)
    {
        _logger.LogInformation("Retrieving TournamentSponsors for Sponsor ID: {SponsorId}", SponsorId);
        return await _tournamentSponsorRepository.GetBySponsorAsync(SponsorId);
    }

    public async Task RegisterSponsorAsync(int tournamentId, int sponsorId, decimal contractAmount)
    {
        // Validar que el torneo existe
        _logger.LogInformation("Registering Sponsor ID: {SponsorId} for Tournament ID: {TournamentId}", sponsorId, tournamentId);
        var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
        if (tournament == null)
        {
            _logger.LogWarning("Tournament with ID {TournamentId} not found for sponsor registration", tournamentId);
            throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}");
        }

        // Validar que el Sponsor existe
        var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
        if (sponsor == null)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found for registration", sponsorId);
            throw new KeyNotFoundException($"No se encontró el patrocinador con ID {sponsorId}");
        }
        // Validar que el monto del contrato sea positivo mayor a 0
        if (contractAmount <= 0)
        {
            _logger.LogWarning("Invalid contract amount {ContractAmount} for Sponsor ID {SponsorId} and Tournament ID {TournamentId}", contractAmount, sponsorId, tournamentId);
            throw new InvalidOperationException("El monto del contrato debe ser mayor a cero");
        }
        // Validar que el que no haya doble registro
        var existingRegistration = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
        if (existingRegistration != null)
        {
            _logger.LogWarning("Sponsor ID {SponsorId} is already registered for Tournament ID {TournamentId}", sponsorId, tournamentId);
            throw new InvalidOperationException($"El patrocinador ya está registrado para este torneo");
        }


        var tournamentSponsor = new TournamentSponsor
        {
            TournamentId = tournamentId,
            SponsorId = sponsorId,
            ContractAmount = contractAmount
        };
        await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);
    }
    public async Task DeleteAsync(int tournamentId, int sponsorId)
    {
        var existingRegistration = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
        if (existingRegistration == null)
        {
            _logger.LogWarning("No registration found for Sponsor ID {SponsorId} and Tournament ID {TournamentId} to delete", sponsorId, tournamentId);
            throw new KeyNotFoundException($"No se encontró una inscripción para el patrocinador con ID {sponsorId} en el torneo con ID {tournamentId}");
        }
        _logger.LogInformation("Deleting registration for Sponsor ID: {SponsorId} and Tournament ID: {TournamentId}", sponsorId, tournamentId);
        await _tournamentSponsorRepository.DeleteAsync(existingRegistration.Id);
    }


}


