
namespace Artillery.DataProcessor
{
    using Artillery.Data;
    using Artillery.Data.Models.Enums;
    using Newtonsoft.Json;
    using System;
    using System.Linq;

    public class Serializer
    {
        public static string ExportShells(ArtilleryContext context, double shellWeight)
        {
            var shells = context.Shells
                .Where(s => s.ShellWeight > shellWeight)
                .Select(s => new
                {
                    ShellWeight = s.ShellWeight,
                    Caliber = s.Caliber,
                    Guns = s.Guns.Where(g => g.GunType == GunType.AntiAircraftGun)
                    .Select(gt => new
                    {
                        GunType = Enum.GetName(typeof(GunType), 3),
                        GunWeight = gt.GunWeight,
                        BarrelLength = gt.BarrelLength,
                        Range = gt.Range > 3000 ? "Long-range" : "Regular range",
                    }).OrderByDescending(gw => gw.GunWeight)
                    .ToArray()
                })
                .OrderBy(sw => sw.ShellWeight)
                .ToArray();

            string jsonString = JsonConvert.SerializeObject(shells, Formatting.Indented);

            return jsonString;
        }

        public static string ExportGuns(ArtilleryContext context, string manufacturer)
        {
            throw new NotImplementedException();
        }
    }
}
