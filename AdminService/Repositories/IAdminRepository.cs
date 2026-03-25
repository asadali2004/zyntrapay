using AdminService.Models;

namespace AdminService.Repositories;

public interface IAdminRepository
{
    Task AddActionAsync(AdminAction action);
    Task SaveChangesAsync();
}