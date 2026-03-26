using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedPlan.Shared.Models
{
    public class UpdateSowingStatusRequest
    {
        public int SowingId { get; set; }
        public int TargetStatus { get; set; }
        public int?  SeedlingCount { get; set; }
        public int? HarvestWeightG { get; set; }
        public int? HarvestCount { get; set; }
        public string? Notes { get; set; }
        public DateTime? EventDate { get; set; }
    }
}
