using AdminService.Data;
using AdminService.Models;

namespace AdminService.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly AdminDbContext _context;

    public AdminRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task AddActionAsync(AdminAction action)
        => await _context.AdminActions.AddAsync(action);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}