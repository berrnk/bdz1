using System;
using System.Globalization;
using Accounting.Domain.Entities;
using Accounting.Domain.Enums;
using Accounting.Infrastructure.Data;

namespace Accounting.Infrastructure.Importers
{
    public class YamlDataImporter : DataImporter
    {
        public YamlDataImporter(DataContext dataContext) : base(dataContext) { }

        protected override List<object> ParseData(string fileContent)
        {
            var parsedObjects = new List<object>();
            string[] lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            string currentSection = "";
            Dictionary<string, string> currentEntry = null;

            var accountEntries = new List<Dictionary<string, string>>();
            var categoryEntries = new List<Dictionary<string, string>>();
            var operationEntries = new List<Dictionary<string, string>>();

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                // Определяем название секции.
                if (!line.StartsWith("-") && line.EndsWith(":"))
                {
                    currentSection = line.TrimEnd(':');
                    continue;
                }

                // Начало нового объекта.
                if (line.StartsWith("-"))
                {
                    currentEntry = new Dictionary<string, string>();
                    if (currentSection == "Accounts")
                        accountEntries.Add(currentEntry);
                    else if (currentSection == "Categories")
                        categoryEntries.Add(currentEntry);
                    else if (currentSection == "Operations")
                        operationEntries.Add(currentEntry);

                    var content = line.Substring(1).Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        var parts = content.Split(new[] { ':' }, 2);
                        if (parts.Length == 2)
                            currentEntry[parts[0].Trim()] = parts[1].Trim().Trim('"');
                    }
                }
                else if (currentEntry != null)
                {
                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                        currentEntry[parts[0].Trim()] = parts[1].Trim().Trim('"');
                }
            }

            // Создание объектов из разобранных данных.
            foreach (var dict in accountEntries)
            {
                long id = long.Parse(dict["Id"]);
                string name = dict["Name"];
                decimal balance = decimal.Parse(dict["Balance"], CultureInfo.InvariantCulture);
                parsedObjects.Add(new BankAccount(id, name, balance));
            }

            foreach (var dict in categoryEntries)
            {
                long id = long.Parse(dict["Id"]);
                string name = dict["Name"];
                CategoryType type = (CategoryType)Enum.Parse(typeof(CategoryType), dict["Type"]);
                parsedObjects.Add(new Category(id, type, name));
            }

            foreach (var dict in operationEntries)
            {
                long id = long.Parse(dict["Id"]);
                OperationType opType = (OperationType)Enum.Parse(typeof(OperationType), dict["Type"]);
                decimal amount = decimal.Parse(dict["Amount"], CultureInfo.InvariantCulture);
                DateTime date = DateTime.Parse(dict["Date"]);
                string description = dict.ContainsKey("Description") ? dict["Description"] : "";
                long categoryId = long.Parse(dict["CategoryId"]);
                long bankAccountId = long.Parse(dict["BankAccountId"]);
                parsedObjects.Add(new Operation(id, opType, bankAccountId, amount, date, description, categoryId));
            }

            return parsedObjects;
        }
    }
}
