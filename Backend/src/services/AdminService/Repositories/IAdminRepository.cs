using AdminService.Models;

namespace AdminService.Repositories;

/// <summary>
/// Defines persistence operations for administrative audit actions.
/// </summary>
public interface IAdminRepository
{
    Task AddActionAsync(AdminAction action);
    Task<List<AdminAction>> GetRecentActionsAsync(int take);
    Task SaveChangesAsync();
}