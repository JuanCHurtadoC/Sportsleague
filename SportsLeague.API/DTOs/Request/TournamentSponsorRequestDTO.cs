namespace SportsLeague.API.DTOs.Request
{
    public class TournamentSponsorRequestDTO
    {
        public int SponsorId { get; set; }
        public int TournamentId { get; set; }
        public decimal ContractAmount { get; set; }
    }
}
