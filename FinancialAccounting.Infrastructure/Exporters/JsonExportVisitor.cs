using System;
using System.Text.Json;
using Accounting.Domain.Entities;

namespace Accounting.Infrastructure.Exporters
{
    public class JsonExportVisitor : IExportVisitor
    {
        private List<BankAccount> _accounts = new List<BankAccount>();
        private List<Category> _categories = new List<Category>();
        private List<Operation> _operations = new List<Operation>();

        public void Visit(BankAccount account)
        {
            _accounts.Add(account);
        }

        public void Visit(Category category)
        {
            _categories.Add(category);
        }

        public void Visit(Operation operation)
        {
            _operations.Add(operation);
        }

        public void SaveToFiles(string accountsFilePath, string categoriesFilePath, string operationsFilePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            string accountsJson = JsonSerializer.Serialize(_accounts, options);
            string categoriesJson = JsonSerializer.Serialize(_categories, options);
            string operationsJson = JsonSerializer.Serialize(_operations, options);

            File.WriteAllText(accountsFilePath, accountsJson);
            File.WriteAllText(categoriesFilePath, categoriesJson);
            File.WriteAllText(operationsFilePath, operationsJson);
        }
    }
}


