namespace TeisterMask.DataProcessor
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
    using Newtonsoft.Json;
    using TeisterMask.Data.Models;
    using TeisterMask.Data.Models.Enums;
    using TeisterMask.DataProcessor.ImportDto;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            XmlRootAttribute rootAtr = new XmlRootAttribute("Projects");
            XmlSerializer serializer = new XmlSerializer(typeof(ImportProjectsDTO[]), rootAtr);

            using StringReader streamReader = new StringReader(xmlString);

            ImportProjectsDTO[] projectDTO = (ImportProjectsDTO[])serializer.Deserialize(streamReader);

            var projectsDB = new List<Project>();

            StringBuilder sb = new StringBuilder();

            foreach (var pDto in projectDTO)
            {
                if (!IsValid(pDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime Odate;

                if (!DateTime.TryParseExact(pDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateResult))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                else
                {
                    Odate = dateResult;
                }

                DateTime? Ddate = null;

                if (!String.IsNullOrEmpty(pDto.DueDate))
                {
                    if (!DateTime.TryParseExact(pDto.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateResult1))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                    else
                    {
                        Ddate = dateResult1;
                    }

                }

                var currentProject = new Project()
                {
                    Name = pDto.Name,
                    OpenDate = Odate,
                    DueDate = Ddate,
                };

                var tasks = new List<Task>();

                foreach (var pt in pDto.Tasks)
                {
                    if (!IsValid(pt))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime TOdate;

                    if (!DateTime.TryParseExact(pt.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateResultTO))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                    else
                    {
                        TOdate = dateResultTO;
                    }

                    DateTime TDdate;

                    if (!DateTime.TryParseExact(pt.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateResultTD))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                    else
                    {
                        TDdate = dateResultTD;
                    }

                    if (TOdate < currentProject.OpenDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (TDdate > currentProject.DueDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var currentTask = new Task()
                    {
                        Name = pt.Name,
                        OpenDate = TOdate,
                        DueDate = TDdate,
                        ExecutionType = (ExecutionType)pt.ExecutionType,
                        LabelType = (LabelType)pt.LabelType
                    };

                    tasks.Add(currentTask);

                }

                currentProject.Tasks = tasks;

                projectsDB.Add(currentProject);

                sb.AppendLine($"Successfully imported project - {currentProject.Name} with {tasks.Count} tasks.");

            }

            context.AddRange(projectsDB);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            ImportEmployeesDTO[] employeeDTO = JsonConvert.DeserializeObject<ImportEmployeesDTO[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            var employeeDB = new List<Employee>();

            foreach (var eDto in employeeDTO)
            {
                if (!IsValid(eDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var currentEmployee = new Employee()
                {
                    Username = eDto.Username,
                    Email = eDto.Email,
                    Phone = eDto.Phone
                };

                var tasks = new HashSet<EmployeeTask>();

                foreach (var et in eDto.Tasks.Distinct())
                {
                    bool checkTaskExist = context.Tasks.Any(e => e.Id == et);

                    if (!checkTaskExist)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var currentTask = new EmployeeTask()
                    {
                        Employee = currentEmployee,
                        TaskId = et
                    };

                    tasks.Add(currentTask);

                }

                currentEmployee.EmployeesTasks = tasks;

                employeeDB.Add(currentEmployee);    

                sb.AppendLine($"Successfully imported employee - {currentEmployee.Username} with {tasks.Count} tasks.");

            }

            context.Employees.AddRange(employeeDB);
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