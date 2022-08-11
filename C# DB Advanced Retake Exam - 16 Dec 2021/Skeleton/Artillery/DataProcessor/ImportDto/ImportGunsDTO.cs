using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Artillery.DataProcessor.ImportDto
{
    public class ImportGunsDTO
    {

        [Required]
        [JsonPropertyName("ManufacturerId")]
        public int ManufacturerId { get; set; }

        [Required]
        [Range(100, 1350000)]
        [JsonPropertyName("GunWeight")]
        public int GunWeight { get; set; }

        [Required]
        [Range(2.00, 35.00)]
        [JsonPropertyName("BarrelLength")]
        public double BarrelLength { get; set; }

        [JsonPropertyName("NumberBuild")]
        public int? NumberBuild { get; set; }

        [Required]
        [Range(1, 100000)]
        [JsonPropertyName("Range")]
        public int Range { get; set; }

        [Required]
        [JsonPropertyName("GunType")]
        public string GunType { get; set; }

        [Required]
        [JsonPropertyName("ShellId")]
        public int ShellId { get; set; }

        [JsonPropertyName("Countries")]
        public ImportGunsCountriesDTO[] Countries { get; set; }

    }
}
