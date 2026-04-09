using Newtonsoft.Json.Converters;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SeedPlan.Shared.Models
{
    [Table("dahlia_varieties")]
    public class Dahlia : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = string.Empty;
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        [Column("height")]
        public int? Height { get; set; } = null;

        [Column("color")]
        public string Color { get; set; } = string.Empty;
        [Column("flower_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DahliaType Type { get; set; } = DahliaType.Decorative;
        [Column("photo_urls")]
        public string? PhotoUrls { get; set; }
        
        [Column("flower_size")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DahliaSize? Size { get; set; } = null;

        [Column("is_approved")]
        public bool IsApproved { get; set; }

        public string GetTypeName() => this.Type switch
        {
            DahliaType.Decorative => "Dekorativ",
            DahliaType.DecorativeGiants => "Dekorativ Jätte",
            DahliaType.DecorativeDwarfs => "Dekorativ Dvärg",
            DahliaType.DecorativeSmall => "Dekorativ Liten",
            DahliaType.DecorativeLarge => "Dekorativ Stor",
            DahliaType.Anemone => "Anemon",
            DahliaType.Ball => "Boll",
            DahliaType.Cactus => "Kaktus",
            DahliaType.SemiCactus => "Semikaktus",
            DahliaType.DwarfCactus => "Dvärgkaktus",
            DahliaType.Pompon => "Pompon",
            DahliaType.Waterlily => "Näckros",
            DahliaType.Mignon => "Mignon",
            DahliaType.Collarette => "Halskrås",
            DahliaType.Exclusive => "Esklusiva",
            DahliaType.Fringed => "Fransade",
            DahliaType.Orchid => "Orkide",
            DahliaType.Other => "Övriga",
            DahliaType.Peony => "Pion",
            DahliaType.Single => "Enkla",
            DahliaType.Stellar => "Stellar",
            _ => Type.ToString()
        };

        public string GetSize() => this.Size switch
        {
            DahliaSize.Under5 => "5 <",
            DahliaSize.Size5to10 => "5 - 10",
            DahliaSize.Size10to15 => "10 - 15",
            DahliaSize.Size15to20 => "15 - 20",
            DahliaSize.Size20to25 => "20 - 25",
            DahliaSize.Over25 => "25 <",
            _ => Size.ToString()
        };

    }
    }

