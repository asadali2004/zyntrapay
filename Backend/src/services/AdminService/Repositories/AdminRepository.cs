using AdminService.Data;
using AdminService.Models;

namespace AdminService.Repositories;

/// <summary>
/// Provides Entity Framework Core data access for admin action audit records.
/// </summary>
public class AdminRepository : IAdminRepository
{
    private readonly AdminDbContext _context;

    public AdminRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task AddActionAsync(AdminAction action)
        => await _context.AdminActions.AddAsync(action);

    public async Task<List<AdminAction>> GetRecentActionsAsync(int take)
        => await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking(_context.AdminActions)
                .OrderByDescending(a => a.PerformedAt)
                .Take(take));

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}