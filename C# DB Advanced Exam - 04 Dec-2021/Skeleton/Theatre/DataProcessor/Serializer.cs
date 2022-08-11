namespace Theatre.DataProcessor
{
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Theatre.Data;
    using Theatre.Data.Models.Enums;
    using Theatre.DataProcessor.ExportDto;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportTheatres(TheatreContext context, int numbersOfHalls)
        {
            var theathres = context.Theatres
                .ToArray()
                .Where(hc => hc.NumberOfHalls >= numbersOfHalls && hc.Tickets.Count >= 20)
                .Select(t => new
                {
                    Name = t.Name,
                    Halls = t.NumberOfHalls,
                    TotalIncome = t.Tickets
                    .Where(r => r.RowNumber >= 1 && r.RowNumber <= 5)
                    .Sum(p => p.Price),
                    Tickets = t.Tickets
                    .Where(r => r.RowNumber >= 1 && r.RowNumber <= 5)
                    .OrderByDescending(p => p.Price)
                    .Select(t => new
                    {
                        Price = Decimal.Parse(t.Price.ToString("f2")),
                        RowNumber = t.RowNumber
                    })
                })
                .OrderByDescending(nh => nh.Halls)
                .ThenBy(tn => tn.Name)
                .ToArray();

            string json = JsonConvert.SerializeObject(theathres, Formatting.Indented);

            return json;    
        }

        public static string ExportPlays(TheatreContext context, double rating)
        {
            var plays = context.Plays
                .ToArray()
                .Where(r => r.Rating <= rating)
                .Select(p => new ExportPlaysDTO()
                {
                    Title = p.Title,
                    Duration = p.Duration.ToString("c"),
                    Rating = p.Rating == 0 ? "Premier" : p.Rating.ToString(),
                    Genre = Enum.GetName(typeof(Genre), p.Genre),
                    Actors = p.Casts
                    .Where(a => a.IsMainCharacter == true)
                    .Select(c => new ExportActorsDTO()
                    {
                        FullName = c.FullName,
                        MainCharacter = $"Plays main character in '{c.Play.Title}'."
                    })
                    .OrderByDescending(a => a.FullName)
                    .ToArray()
                })
                .OrderBy(p => p.Title)
                .ThenByDescending(g => g.Genre)
                .ToArray();

            string resultXML = SerializeXml(plays, "Plays");

            return resultXML;
        }

        private static string SerializeXml<T>(T[] objects, string root)
        {
            var serializer = new XmlSerializer(typeof(T[]), new XmlRootAttribute(root));
            var namespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName() });

            var sb = new StringBuilder();

            serializer.Serialize(new StringWriter(sb), objects, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}
