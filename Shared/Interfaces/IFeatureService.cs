using SeedPlan.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedPlan.Shared.Interfaces
{
    public interface IFeatureService
    {
        Task<AppFeature> GetLatestActiveFeatureAsync();
        Task<bool> HasUserSeenFeatureAsync(int featureId);
        Task MarkFeatureAsSeenAsync(int featureId);

    }
}
