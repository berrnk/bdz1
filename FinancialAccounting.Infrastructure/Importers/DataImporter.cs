using FinancialAccounting.Domain.Entities;
using FinancialAccounting.Infrastructure.Data;

namespace FinancialAccounting.Infrastructure.Importers
{
    public abstract class DataImporter
    {
        protected DataContext _dataContext;

        protected DataImporter(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        // Общая логика импорта: чтение файла, парсинг и сохранение
        public void ImportData(string filePath)
        {
            try
            {
                string fileContent = File.ReadAllText(filePath);
                var data = ParseData(fileContent);
                SaveData(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка импорта: " + ex.Message);
            }
        }

        // Абстрактный метод для разбора содержимого файла – реализуется в наследниках
        protected abstract List<object> ParseData(string fileContent);

        // Общий метод сохранения разобранных объектов в DataContext
        protected virtual void SaveData(List<object> data)
        {
            foreach (var obj in data)
            {
                if (obj is BankAccount account)
                {
                    _dataContext.BankAccounts.Add(account);
                }
                else if (obj is Category category)
                {
                    _dataContext.Categories.Add(category);
                }
                else if (obj is Operation op)
                {
                    _dataContext.Operations.Add(op);
                }
            }
            Console.WriteLine($"Импортировано объектов: {data.Count}");
        }
    }
}

