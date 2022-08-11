namespace Footballers.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Footballers.Data.Models;
    using Footballers.Data.Models.Enums;
    using Footballers.DataProcessor.ImportDto;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedCoach
            = "Successfully imported coach - {0} with {1} footballers.";

        private const string SuccessfullyImportedTeam
            = "Successfully imported team - {0} with {1} footballers.";

        public static string ImportCoaches(FootballersContext context, string xmlString)
        {

            StringBuilder sb = new StringBuilder();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportCoachesDTO[]), new XmlRootAttribute("Coaches"));

            using StringReader stringReader = new StringReader(xmlString);

            ImportCoachesDTO[] coachesDtos = (ImportCoachesDTO[])xmlSerializer.Deserialize(stringReader);

            List<Coach> coachesDB = new List<Coach>();

            foreach (var cDto in coachesDtos)
            {

                if (!IsValid(cDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (String.IsNullOrEmpty(cDto.Nationality))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                var coach = new Coach()
                {
                    Name = cDto.Name,
                    Nationality = cDto.Nationality
                };

                var footballersCount = new List<Footballer>();

                foreach (var fDto in cDto.Footballers)
                {
                    if (!IsValid(fDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime contractStartDate;

                    if (String.IsNullOrWhiteSpace(fDto.ContractStartDate))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                    else
                    {
                        bool isDueDateValid = DateTime.TryParseExact(fDto.ContractStartDate, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate);

                        if (!isDueDateValid)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        contractStartDate = startDate;
                    }

                    DateTime contractEndDate;

                    if (String.IsNullOrWhiteSpace(fDto.ContractEndDate))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                    else
                    {
                        bool isDueDateValid = DateTime.TryParseExact(fDto.ContractEndDate, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endtDate);

                        if (!isDueDateValid)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        contractEndDate = endtDate;
                    }

                    if (contractStartDate > contractEndDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Footballer currentFootbaler = new Footballer()
                    {
                        Name = fDto.Name,
                        ContractStartDate = contractStartDate,
                        ContractEndDate = contractEndDate,
                        BestSkillType = (BestSkillType)fDto.BestSkillType,
                        PositionType = (PositionType)fDto.PositionType
                    };

                    footballersCount.Add(currentFootbaler);
                }

                coach.Footballers = footballersCount;

                coachesDB.Add(coach);

                sb.AppendLine($"Successfully imported coach - {coach.Name} with {footballersCount.Count} footballers.");

            }

            context.Coaches.AddRange(coachesDB);
            context.SaveChanges();

            return sb.ToString().TrimEnd();



        }
        public static string ImportTeams(FootballersContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            ImportTeamsDTO[] teamsDtos = JsonConvert.DeserializeObject<ImportTeamsDTO[]>(jsonString);

            List<Team> teamDB = new List<Team>();

            foreach (var tDto in teamsDtos)
            {
                if (!IsValid(tDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (String.IsNullOrEmpty(tDto.Nationality))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                if (String.IsNullOrEmpty(tDto.Trophies))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                int trophiesResult;

                if (int.TryParse(tDto.Trophies, out int trophies))
                {
                    trophiesResult = trophies;
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (trophiesResult <= 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Team currentTeam = new Team()
                {
                    Name = tDto.Name,
                    Nationality = tDto.Nationality,
                    Trophies = trophiesResult
                };


                List<TeamFootballer> footbalrCount = new List<TeamFootballer>();

                foreach (int fbl in tDto.Footballers.Distinct())
                {
                    Footballer f = context.Footballers.Find(fbl);

                    if (f == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var currentFootbl = new TeamFootballer()
                    {
                        Team = currentTeam,
                        FootballerId = fbl
                    };


                    footbalrCount.Add(currentFootbl);
                    //currentCoach.Footballers.Add(f);
                }

               
                currentTeam.TeamsFootballers = footbalrCount;

                teamDB.Add(currentTeam);

                sb.AppendLine($"Successfully imported team - {currentTeam.Name} with {footbalrCount.Count} footballers.");

            }


            context.Teams.AddRange(teamDB);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}
