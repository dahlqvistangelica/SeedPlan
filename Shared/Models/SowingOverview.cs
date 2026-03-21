using System;
using System.Collections.Generic;
using System.Text;

namespace SeedPlan.Shared.Models
{
    public class SowingOverview
    {
        public List<Plant> Past { get; set; } = new();
        public List<Plant> Current { get; set; } = new();
        public List<Plant> Upcoming { get; set; } = new();
    }
}
