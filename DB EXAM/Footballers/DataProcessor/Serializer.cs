namespace Footballers.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Footballers.DataProcessor.ExportDto;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportCoachesWithTheirFootballers(FootballersContext context)
        {
            StringBuilder sb = new StringBuilder();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportCachesDTO[]), new XmlRootAttribute("Coaches"));

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);


            var coaches = context.Coaches
                .Where(c => c.Footballers.Count > 0)
                .Select(c => new ExportCachesDTO
                {
                    FootballersCount = c.Footballers.Count.ToString(),
                    CoachName = c.Name,
                    Footballers = c.Footballers.Select(f => new ExportFootbalerDTO()
                    {
                        Name = f.Name,
                        Position = f.PositionType.ToString()
                    })
                    .OrderBy(f => f.Name)
                    .ToArray()
                })
                .OrderByDescending(fc => fc.FootballersCount)
                .ThenBy(n => n.CoachName)
                .ToArray();

            xmlSerializer.Serialize(sw, coaches, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string ExportTeamsWithMostFootballers(FootballersContext context, DateTime date)
        {
            var teams = context.Teams
                .Where(f => f.TeamsFootballers.Any())
                .ToArray()
                .Select(f => new
                {
                    Name = f.Name,
                    Footballers = f.TeamsFootballers
                    .Where(f => f.Footballer.ContractStartDate >= date)
                    .OrderByDescending(f => f.Footballer.ContractEndDate)
                    .ThenBy(f => f.Footballer.Name)
                    .Select(ft => new
                    {
                        FootballerName = ft.Footballer.Name,
                        ContractStartDate = ft.Footballer.ContractStartDate.ToString("d", CultureInfo.InvariantCulture),
                        ContractEndDate = ft.Footballer.ContractEndDate.ToString("d", CultureInfo.InvariantCulture),
                        BestSkillType = ft.Footballer.BestSkillType.ToString(),
                        PositionType = ft.Footballer.PositionType.ToString()
                    })
                })
                .OrderByDescending(x => x.Footballers.Count())
                .ThenBy(x => x.Name)
                .Take(5)
                .ToArray();

            return JsonConvert.SerializeObject(teams, Formatting.Indented);
        }
    }
}
