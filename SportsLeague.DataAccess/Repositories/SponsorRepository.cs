using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Enums;

namespace SportsLeague.DataAccess.Repositories;

    public class SponsorRepository : GenericRepository<Sponsor>, ISponsorRepository
    {
        public SponsorRepository(LeagueDbContext context) : base(context)
        {
        }

        public async Task<Sponsor?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.SponsorName.ToLower() == name.ToLower());
        }
        public async Task<IEnumerable<Sponsor>> GetByCategoryAsync(SponsorCategory category)
        {
            return await _dbSet
                .Where(s => s.Category == category)
                .ToListAsync();
        }

}

