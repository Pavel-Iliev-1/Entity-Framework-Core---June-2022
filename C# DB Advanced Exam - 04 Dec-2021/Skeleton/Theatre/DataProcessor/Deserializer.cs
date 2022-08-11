namespace Theatre.DataProcessor
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;
    using Theatre.Data;
    using Theatre.Data.Models;
    using Theatre.Data.Models.Enums;
    using Theatre.DataProcessor.ExportDto;
    using Theatre.DataProcessor.ImportDto;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfulImportPlay
            = "Successfully imported {0} with genre {1} and a rating of {2}!";

        private const string SuccessfulImportActor
            = "Successfully imported actor {0} as a {1} character!";

        private const string SuccessfulImportTheatre
            = "Successfully imported theatre {0} with #{1} tickets!";

        public static string ImportPlays(TheatreContext context, string xmlString)
        {
            XmlRootAttribute rootAtr = new XmlRootAttribute("Plays");
            XmlSerializer serializer = new XmlSerializer(typeof(ImportPlaysDTO[]), rootAtr);

            using StringReader streamReader = new StringReader(xmlString);

            ImportPlaysDTO[] playsDTO = (ImportPlaysDTO[])serializer.Deserialize(streamReader);

            var playsDB = new List<Play>();

            StringBuilder sb = new StringBuilder();

            foreach (var pDto in playsDTO)
            {
                if (!IsValid(pDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                TimeSpan duration = TimeSpan.ParseExact(pDto.Duration, "c", CultureInfo.InvariantCulture);

                if (duration.Hours < 1)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                };

                if (!Enum.IsDefined(typeof(Genre), pDto.Genre))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Play currentPlay = new Play()
                {
                    Title = pDto.Title,
                    Duration = duration,
                    Rating = pDto.Rating,
                    Genre = (Genre)Enum.Parse(typeof(Genre), pDto.Genre), // input Genre STRING
                    Description = pDto.Description,
                    Screenwriter = pDto.Screenwriter
                };

                playsDB.Add(currentPlay);

                sb.AppendLine(String.Format(SuccessfulImportPlay, pDto.Title, pDto.Genre, pDto.Rating));

            };

            context.Plays.AddRange(playsDB);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportCasts(TheatreContext context, string xmlString)
        {
            XmlRootAttribute rootAtr = new XmlRootAttribute("Casts");
            XmlSerializer serializer = new XmlSerializer(typeof(ImportCastDTO[]), rootAtr);

            using StringReader streamReader = new StringReader(xmlString);

            ImportCastDTO[] castDTO = (ImportCastDTO[])serializer.Deserialize(streamReader);

            var castsDB = new List<Cast>();

            StringBuilder sb = new StringBuilder();

            foreach (var cDto in castDTO)
            {
                if (!IsValid(cDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Cast currentCast = new Cast()
                {
                    FullName = cDto.FullName,
                    IsMainCharacter = cDto.IsMainCharacter,
                    PhoneNumber = cDto.PhoneNumber,
                    PlayId = cDto.PlayId
                };

                castsDB.Add(currentCast);

                var charackter = cDto.IsMainCharacter ? "main" : "lesser";

                sb.AppendLine($"Successfully imported actor {cDto.FullName} as a {charackter} character!");

            };

            context.Casts.AddRange(castsDB);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportTtheatersTickets(TheatreContext context, string jsonString)
        {
            ImportTeathreTicketsDTO[] theatreDTO = JsonConvert.DeserializeObject<ImportTeathreTicketsDTO[]>(jsonString);

            var theatreDB = new List<Theatre>();

            StringBuilder sb = new StringBuilder();

            foreach (var tDto in theatreDTO)
            {
                if (!IsValid(tDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var currentTheatre = new Theatre()
                {
                    Name = tDto.Name,
                    NumberOfHalls = (sbyte)tDto.NumberOfHalls,
                    Director = tDto.Director
                };

                foreach (var ticket in tDto.Tickets)
                {
                    if (!IsValid(ticket))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var currentTicket = new Ticket()
                    {
                        Price = ticket.Price,
                        RowNumber = ticket.RowNumber,
                        PlayId = ticket.PlayId
                    };

                    currentTheatre.Tickets.Add(currentTicket);
                }
                
                theatreDB.Add(currentTheatre);

                sb.AppendLine($"Successfully imported theatre {tDto.Name} with #{currentTheatre.Tickets.Count} tickets!");
            }

            context.Theatres.AddRange(theatreDB);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

        }


        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
        }
    }
}
