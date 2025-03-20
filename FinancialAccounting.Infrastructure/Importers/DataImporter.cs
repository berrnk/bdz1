using Accounting.Domain.Entities;
using Accounting.Infrastructure.Data;

namespace Accounting.Infrastructure.Importers
{
    public abstract class DataImporter
    {
        protected DataContext dataContext;

        protected DataImporter(DataContext context)
        {
            dataContext = context;
        }

        // Основной метод импорта: читает файл, обрабатывает содержимое и сохраняет данные.
        public void ImportData(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                var items = ParseData(content);
                SaveData(items);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при импорте: " + ex.Message);
            }
        }

        // Абстрактный метод для анализа и разбора содержимого файла. Реализуется в производных классах.
        protected abstract List<object> ParseData(string fileContent);

        // Метод для сохранения разобранных объектов в DataContext.
        protected virtual void SaveData(List<object> items)
        {
            foreach (var item in items)
            {
                if (item is BankAccount account)
                {
                    dataContext.BankAccounts.Add(account);
                }
                else if (item is Category category)
                {
                    dataContext.Categories.Add(category);
                }
                else if (item is Operation op)
                {
                    dataContext.Operations.Add(op);
                }
            }
            Console.WriteLine($"Импортировано объектов: {items.Count}");
        }
    }
}
