using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedPlan.Shared.Helpers
{
   public class OpenMeteoGeocodingResponse
    {
        public List<GeocodingResult>? Results { get; set; }
    }
    public class GeocodingResult
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
