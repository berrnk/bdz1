using System;
using FinancialAccounting.Domain.Entities;
using System.Text.Json;
using FinancialAccounting.Infrastructure.Data;

namespace FinancialAccounting.Infrastructure.Importers
{
    public class JsonDataImporter : DataImporter
    {
        public JsonDataImporter(DataContext dataContext) : base(dataContext) { }

        protected override List<object> ParseData(string fileContent)
        {
            var result = new List<object>();

            // Ожидаем, что JSON имеет вид:
                // {
                //   "Accounts": [ { ... }, ... ],
                //   "Categories": [ { ... }, ... ],
                //   "Operations": [ { ... }, ... ]
                // }
            var importWrapper = JsonSerializer.Deserialize<ImportWrapper>(fileContent);
            if (importWrapper.Accounts != null)
                result.AddRange(importWrapper.Accounts);
            if (importWrapper.Categories != null)
                result.AddRange(importWrapper.Categories);
            if (importWrapper.Operations != null)
                result.AddRange(importWrapper.Operations);

            return result;
        }
    }

    // Класс-обёртка для десериализации JSON
    public class ImportWrapper
    {
        public List<BankAccount> Accounts { get; set; }
        public List<Category> Categories { get; set; }
        public List<Operation> Operations { get; set; }
    }
}

