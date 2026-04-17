using SeedPlan.Shared.Models;
using SeedPlan.Shared.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedPlan.Shared.Interfaces
{
    public interface IUserDahliaService
    {
        Task<List<UserDahlia>> GetUserDahliasAsync();
        Task UpdateUserDahliaAsync(UserDahlia userDahlia);
        Task AddUserDahliaAsync(UserDahlia userDahlia);
        Task DeleteUserDahliaAsync(int id);

    }

    public interface IDahliaService
    {
        Task<List<Dahlia>> GetAllDahliasAsync();

        Task<Dahlia> AddDahliaVarietyAsync(Dahlia newDahlia);

        Task<List<string>> GetAvailableColorsAsync();
        Task<List<Dahlia>> SearchDahliasAsync(string searchTerm);
        Task<(List<Dahlia> Dahlias, int TotalCount)> GetPagedDahliasAsync(
 int pageNumber,
 int pageSize,
 string searchTerm = "",
 List<DahliaType>? activeTypes = null,
 List<DahliaSize>? activeSizes = null,
 List<string>? activeColors = null,
 int? maxHeight = null,
 int? minHeight = null);

        Task<List<Dahlia>> GetPendingDahliasAsync();
        Task ApproveDahliaAsync(string id);
        Task DeleteDahliaAsync(string id);
    }
}