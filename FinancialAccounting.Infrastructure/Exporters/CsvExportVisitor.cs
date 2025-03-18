using System;
using System.Globalization;
using FinancialAccounting.Domain.Entities;

namespace FinancialAccounting.Infrastructure.Exporters
{
    public class CsvExportVisitor : IExportVisitor
    {
        private readonly List<string> _accountRows = new() { "Id,Name,Balance" };
        private readonly List<string> _categoryRows = new() { "Id,Name,Type" };
        private readonly List<string> _operationRows = new() { "Id,Type,Amount,Date,Description,CategoryId,BankAccountId" };



        public void Visit(BankAccount account)
        {
            // Формирование строки CSV для счета с использованием экранирования
            _accountRows.Add($"{account.Id},{EscapeCsv(account.Name)},{account.Balance.ToString(CultureInfo.InvariantCulture)}");
        }

        public void Visit(Category category)
        {
            _categoryRows.Add($"{category.Id},{EscapeCsv(category.Name)},{category.Type}");
        }

        public void Visit(Operation operation)
        {
            _operationRows.Add($"{operation.Id},{operation.Type},{operation.Amount.ToString(CultureInfo.InvariantCulture)},{operation.Date:yyyy-MM-dd},{EscapeCsv(operation.Description)},{operation.CategoryId},{operation.BankAccountId}");
        }

        // Метод экранирования строк для CSV: если значение содержит запятые, кавычки или переносы строк, оборачиваем его в кавычки
        private static string EscapeCsv(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            if (input.Contains(",") || input.Contains("\"") || input.Contains("\n"))
            {
                input = input.Replace("\"", "\"\""); // заменяем каждую кавычку на две
                return $"\"{input}\"";
            }
            return input;
        }

        // Метод для сохранения данных в отдельные CSV файлы
        public void SaveToFiles(string accountsFilePath, string categoriesFilePath, string operationsFilePath)
        {
            File.WriteAllLines(accountsFilePath, _accountRows);
            File.WriteAllLines(categoriesFilePath, _categoryRows);
            File.WriteAllLines(operationsFilePath, _operationRows);
        }
    }
}

