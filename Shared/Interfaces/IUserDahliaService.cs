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

        Task<List<Dahlia>> SearchDahliasAsync(string searchTerm);
    }
}