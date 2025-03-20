using System;
using System.Text.Json;
using Accounting.Domain.Entities;
using Accounting.Infrastructure.Data;

namespace Accounting.Infrastructure.Importers
{
    public class JsonDataImporter : DataImporter
    {
        public JsonDataImporter(DataContext dataContext) : base(dataContext) { }

        protected override List<object> ParseData(string fileContent)
        {
            var items = new List<object>();
            var wrapper = JsonSerializer.Deserialize<ImportWrapper>(fileContent);
            if (wrapper.Accounts != null)
                items.AddRange(wrapper.Accounts);
            if (wrapper.Categories != null)
                items.AddRange(wrapper.Categories);
            if (wrapper.Operations != null)
                items.AddRange(wrapper.Operations);

            return items;
        }
    }

    // Обёртка для десериализации JSON-структуры.
    public class ImportWrapper
    {
        public List<BankAccount> Accounts { get; set; }
        public List<Category> Categories { get; set; }
        public List<Operation> Operations { get; set; }
    }
}
