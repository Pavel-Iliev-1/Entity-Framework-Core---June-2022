namespace TeisterMask.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using TeisterMask.Data.Models.Enums;
    using TeisterMask.DataProcessor.ExportDto;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportProjectWithTheirTasks(TeisterMaskContext context)
        {
            var projects = context.Projects
                .Where(p => p.Tasks.Any())
                .ToArray()
                .Select(x => new ExportProjectDTO()
                {
                    TasksCount = x.Tasks.Count.ToString(),
                    ProjectName = x.Name,
                    HasEndDate = x.DueDate == null ? "No" : "Yes",
                    Tasks = x.Tasks.Select(t => new ExportTaskDTO()
                    {
                        Name = t.Name,
                        Label = t.LabelType.ToString()
                    })
                    .OrderBy(x => x.Name)
                    .ToArray()


                })
                .OrderByDescending(t => t.Tasks.Length)
                .ThenBy(x => x.ProjectName)
                .ToArray();

            var result = SerializeXml(projects, "Projects");

            return result;
        }

        public static string ExportMostBusiestEmployees(TeisterMaskContext context, DateTime date)
        {
            var employees = context.Employees
               .ToArray()
               .Where(et => et.EmployeesTasks.Count > 0)
               .Select(e => new
               {
                   e.Username,
                   Tasks = e.EmployeesTasks
                   .Where(od => od.Task.OpenDate >= date)
                   .OrderByDescending(x => x.Task.DueDate)
                   .ThenBy(x => x.Task.Name)
                   .Select(x => new
                   {
                       TaskName = x.Task.Name,
                       OpenDate = x.Task.OpenDate.ToString("d", CultureInfo.InvariantCulture),
                       DueDate = x.Task.DueDate.ToString("d", CultureInfo.InvariantCulture),
                       LabelType = Enum.GetName(typeof(LabelType), x.Task.LabelType),
                       ExecutionType = Enum.GetName(typeof(ExecutionType), x.Task.ExecutionType)

                   })
                   
                   .ToArray()
               })
               .OrderByDescending(x => x.Tasks.Length)
               .ThenBy(x => x.Username)
               .ToArray();

            string json = JsonConvert.SerializeObject(employees, Formatting.Indented);

            return json;
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