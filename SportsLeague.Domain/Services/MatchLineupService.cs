using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Helpers;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services;

public class MatchLineupService : IMatchLineupService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IMatchLineupRepository _lineupRepository;

    public MatchLineupService(
        IMatchRepository matchRepository,
        IPlayerRepository playerRepository,
        IMatchLineupRepository lineupRepository)
    {
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _lineupRepository = lineupRepository;
    }

    public async Task<MatchLineup> AddPlayerAsync(
        int matchId,
        MatchLineup lineup)
    {
        // Validaciones
        var match = await _matchRepository.GetByIdAsync(matchId);

        if (match == null)
            throw new KeyNotFoundException(
                $"No se encontró el partido con ID {matchId}");

        if (match.Status != MatchStatus.Scheduled)
            throw new InvalidOperationException(
                "Solo se pueden registrar alineaciones en partidos Scheduled");

        var player = await _playerRepository
            .GetByIdAsync(lineup.PlayerId);

        if (player == null)
            throw new KeyNotFoundException(
                $"No se encontró el jugador con ID {lineup.PlayerId}");

        if (player.TeamId != match.HomeTeamId &&
            player.TeamId != match.AwayTeamId)
            throw new InvalidOperationException(
                "El jugador no pertenece a ninguno de los equipos del partido");

        var exists = await _lineupRepository
            .ExistsByMatchAndPlayerAsync(
                matchId,
                lineup.PlayerId);

        if (exists)
            throw new InvalidOperationException(
                "El jugador ya está registrado en la alineación de este partido");

        if (lineup.IsStarter)
        {
            var teamPlayers = await _lineupRepository
                .GetByMatchAndTeamAsync(matchId, player.TeamId);

            var startersCount = teamPlayers
                .Count(p => p.IsStarter);

            if (startersCount >= 11)
                throw new InvalidOperationException(
                    "El equipo ya tiene 11 titulares registrados en este partido");
        }

        lineup.MatchId = matchId;

        return await _lineupRepository
            .CreateAsync(lineup);
    }

    public async Task<IEnumerable<MatchLineup>>
        GetByMatchAsync(int matchId)
    {
        var match = await _matchRepository
            .GetByIdAsync(matchId);

        if (match == null)
            throw new KeyNotFoundException(
                $"No se encontró el partido con ID {matchId}");

        var lineup = await _lineupRepository
            .GetByMatchAsync(matchId);

        if (!lineup.Any())
            throw new InvalidOperationException(
                "No se encontraron jugadores registrados en la alineación");

        return lineup;
    }

    public async Task<IEnumerable<MatchLineup>>
        GetByMatchAndTeamAsync(
            int matchId,
            int teamId)
    {
        var match = await _matchRepository
            .GetByIdAsync(matchId);

        if (match == null)
            throw new KeyNotFoundException(
                $"No se encontró el partido con ID {matchId}");

        if (teamId != match.HomeTeamId &&
            teamId != match.AwayTeamId)
        {
            throw new InvalidOperationException(
                "El equipo no pertenece al partido");
        }

        var lineup = await _lineupRepository
            .GetByMatchAndTeamAsync(matchId, teamId);

        if (!lineup.Any())
            throw new InvalidOperationException(
                "No se encontraron jugadores registrados para este equipo");

        return lineup;
    }

    public async Task DeleteAsync(int id)
    {
        var exists = await _lineupRepository
            .ExistsAsync(id);

        if (!exists)
            throw new KeyNotFoundException(
                $"No se encontró registro con ID {id}");

        await _lineupRepository.DeleteAsync(id);
    }
}
