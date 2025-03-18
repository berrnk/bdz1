using System;
using FinancialAccounting.Domain.Entities;
using FinancialAccounting.Domain.Enums;
using System.Globalization;
using FinancialAccounting.Infrastructure.Data;

namespace FinancialAccounting.Infrastructure.Importers
{
    public class YamlDataImporter : DataImporter
    {
        public YamlDataImporter(DataContext dataContext) : base(dataContext) { }

        protected override List<object> ParseData(string fileContent)
        {
            var result = new List<object>();

            // Разбиваем файл на строки
            string[] lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            string currentSection = "";
            Dictionary<string, string> temp = null;

            var accounts = new List<Dictionary<string, string>>();
            var categories = new List<Dictionary<string, string>>();
            var operations = new List<Dictionary<string, string>>();

            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                // Определяем название секции (Accounts, Categories, Operations)
                if (!line.StartsWith("-") && line.EndsWith(":"))
                {
                    currentSection = line.Substring(0, line.Length - 1);
                    continue;
                }

                // Начало нового объекта
                if (line.StartsWith("-"))
                {
                    temp = new Dictionary<string, string>();
                    if (currentSection == "Accounts")
                        accounts.Add(temp);
                    else if (currentSection == "Categories")
                        categories.Add(temp);
                    else if (currentSection == "Operations")
                        operations.Add(temp);

                    // Если после '-' сразу идет пара ключ:значение
                    string content = line.Substring(1).Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        var parts = content.Split(new[] { ':' }, 2);
                        if (parts.Length == 2)
                        {
                            temp[parts[0].Trim()] = parts[1].Trim().Trim('"');
                        }
                    }
                }
                else if (temp != null)
                {
                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        temp[parts[0].Trim()] = parts[1].Trim().Trim('"');
                    }
                }
            }

            // Создание объектов из полученных словарей

            foreach (var dict in accounts)
            {
                long id = long.Parse(dict["Id"]);
                string name = dict["Name"];
                decimal balance = decimal.Parse(dict["Balance"], CultureInfo.InvariantCulture);
                result.Add(new BankAccount(id, name, balance));
            }

            foreach (var dict in categories)
            {
                long id = long.Parse(dict["Id"]);
                string name = dict["Name"];
                CategoryType type = (CategoryType)Enum.Parse(typeof(CategoryType), dict["Type"]);
                result.Add(new Category(id, type, name));
            }

            foreach (var dict in operations)
            {
                long id = long.Parse(dict["Id"]);
                OperationType opType = (OperationType)Enum.Parse(typeof(OperationType), dict["Type"]);
                decimal amount = decimal.Parse(dict["Amount"], CultureInfo.InvariantCulture);
                DateTime date = DateTime.Parse(dict["Date"]);
                string description = dict.ContainsKey("Description") ? dict["Description"] : "";
                long categoryId = long.Parse(dict["CategoryId"]);
                long bankAccountId = long.Parse(dict["BankAccountId"]);
                result.Add(new Operation(id, opType, bankAccountId, amount, date, description, categoryId));
            }

            return result;
        }
    }
}

