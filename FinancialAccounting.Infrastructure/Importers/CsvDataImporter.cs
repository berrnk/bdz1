using System;
using FinancialAccounting.Domain.Entities;
using FinancialAccounting.Domain.Enums;
using System.Globalization;
using FinancialAccounting.Infrastructure.Data;

namespace FinancialAccounting.Infrastructure.Importers
{
    public class CsvDataImporter : DataImporter
    {
        public CsvDataImporter(DataContext dataContext) : base(dataContext) { }

        protected override List<object> ParseData(string fileContent)
        {
            var result = new List<object>();

            // Разбиваем файл на секции (Accounts, Categories, Operations) по двойным переводам строки
            string[] sections = fileContent.Split(new string[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (sections.Length < 3)
                throw new Exception("Неверный формат CSV файла. Ожидается три секции: Счета, Категории, Операции.");

            // --- Парсинг счетов ---
            var accountLines = sections[0].Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            // Пропускаем заголовок
            for (int i = 1; i < accountLines.Length; i++)
            {
                var fields = accountLines[i].Split(',');
                if (fields.Length < 3) continue;
                // Предполагается: Id,Name,Balance
                long id = long.Parse(fields[0]);
                string name = fields[1];
                decimal balance = decimal.Parse(fields[2], CultureInfo.InvariantCulture);
                result.Add(new BankAccount(id, name, balance));
            }

            // --- Парсинг категорий ---
            var categoryLines = sections[1].Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < categoryLines.Length; i++)
            {
                var fields = categoryLines[i].Split(',');
                if (fields.Length < 3) continue;
                // Предполагается: Id,Name,Type
                long id = long.Parse(fields[0]);
                string name = fields[1];
                CategoryType type = (CategoryType)Enum.Parse(typeof(CategoryType), fields[2]);
                result.Add(new Category(id, type, name));
            }

            // --- Парсинг операций ---
            var operationLines = sections[2].Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < operationLines.Length; i++)
            {
                var fields = operationLines[i].Split(',');
                if (fields.Length < 7) continue;
                // Предполагается: Id,Type,Amount,Date,Description,CategoryId,BankAccountId
                long id = long.Parse(fields[0]);
                OperationType opType = (OperationType)Enum.Parse(typeof(OperationType), fields[1]);
                decimal amount = decimal.Parse(fields[2], CultureInfo.InvariantCulture);
                DateTime date = DateTime.Parse(fields[3]);
                string description = fields[4];
                long categoryId = long.Parse(fields[5]);
                long bankAccountId = long.Parse(fields[6]);
                result.Add(new Operation(id, opType, bankAccountId, amount, date, description, categoryId));
            }

            return result;
        }
    }
}

