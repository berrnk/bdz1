using Accounting.Application.Commands;
using Accounting.Application.Facades;
using Accounting.Domain.Enums;
using Accounting.Infrastructure.Data;
using Accounting.Infrastructure.Exporters;
using Accounting.Infrastructure.Importers;
using Microsoft.Extensions.DependencyInjection;
using ICommand = Accounting.Application.Commands.ICommand;

namespace Accounting.ConsoleApp;
class Program
{
    static void Main(string[] args)
    {
        // Инициализация DI-контейнера для разрешения зависимостей.
        var serviceProvider = new ServiceCollection()
            // Регистрируем единичный экземпляр DataContext для всего приложения.
            .AddSingleton<DataContext>()
            // Регистрируем FinancialFacade как Singleton, поскольку он использует DataContext.
            .AddSingleton<FinancialFacade>()
            // Регистрируем экспортёры и импортёры в виде Transient, если потребуется.
            .AddTransient<JsonExportVisitor>()
            .AddTransient<YamlExportVisitor>()
            .AddTransient<CsvExportVisitor>()
            .AddTransient<JsonDataImporter>()
            .AddTransient<YamlDataImporter>()
            .AddTransient<CsvDataImporter>()
            .BuildServiceProvider();

        // Извлекаем необходимые объекты из контейнера.
        var facade = serviceProvider.GetRequiredService<FinancialFacade>();
        var dataContext = serviceProvider.GetRequiredService<DataContext>();

        // Запуск основного меню приложения.
        LaunchMenu(facade, dataContext);
    }

    static void LaunchMenu(FinancialFacade facade, DataContext dataContext)
    {
        bool exitApp = false;
        while (!exitApp)
        {
            Console.WriteLine("\n\n*** Главное меню финансового учёта ***");
            Console.WriteLine("1. Добавить новый счёт.");
            Console.WriteLine("2. Изменить существующий счёт.");
            Console.WriteLine("3. Удалить счёт.");
            Console.WriteLine("4. Добавить новую категорию.");
            Console.WriteLine("5. Изменить категорию.");
            Console.WriteLine("6. Удалить категорию.");
            Console.WriteLine("7. Добавить новую операцию.");
            Console.WriteLine("8. Изменить операцию.");
            Console.WriteLine("9. Удалить операцию.");
            Console.WriteLine("10. Анализ: разница доходов и расходов.");
            Console.WriteLine("11. Анализ: группировка операций по категориям.");
            Console.WriteLine("12. Экспорт данных.");
            Console.WriteLine("13. Импорт данных.");
            Console.WriteLine("0. Завершить работу приложения.");
            Console.Write("\nВыберите пункт меню: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddAccount(facade);
                    break;
                case "2":
                    ModifyAccount(facade);
                    break;
                case "3":
                    RemoveAccount(facade);
                    break;
                case "4":
                    AddCategory(facade);
                    break;
                case "5":
                    ModifyCategory(facade);
                    break;
                case "6":
                    RemoveCategory(facade);
                    break;
                case "7":
                    AddOperation(facade);
                    break;
                case "8":
                    ModifyOperation(facade);
                    break;
                case "9":
                    RemoveOperation(facade);
                    break;
                case "10":
                    DisplayIncomeExpenseDifference(facade);
                    break;
                case "11":
                    DisplayGroupedOperations(facade);
                    break;
                case "12":
                    PerformDataExport(facade);
                    break;
                case "13":
                    PerformDataImport(dataContext);
                    break;
                case "0":
                    exitApp = true;
                    break;
                default:
                    Console.WriteLine("Пункт меню не распознан. Попробуйте ещё раз.");
                    Console.WriteLine("Нажмите любую клавишу для продолжения.");
                    Console.ReadKey();
                    break;
            }
        }
    }

    static void AddAccount(FinancialFacade facade)
    {
        Console.WriteLine("\nДобавление нового банковского счёта.");
        Console.Write("Укажите название счёта: ");
        string accountName = Console.ReadLine();
        Console.Write("Введите стартовый баланс: ");
        string balanceStr = Console.ReadLine();
        if (!decimal.TryParse(balanceStr, out decimal accountBalance))
        {
            Console.WriteLine("Ошибка: неверный формат баланса. Введите числовое значение.");
            Console.WriteLine("Нажмите любую клавишу для продолжения.");
            Console.ReadKey();
            return;
        }

        // Применяем команду, обернув её декоратором для измерения времени выполнения.
        ICommand addAccountCommand = new CreateAccountCommand(facade, accountName, accountBalance);
        ICommand timedCommand = new TimeMeasuredCommandDecorator(addAccountCommand);
        timedCommand.Execute();

        Console.WriteLine("Нажмите любую клавишу для продолжения.");
        Console.ReadKey();
    }

    static void AddCategory(FinancialFacade facade)
    {
        Console.WriteLine("\nДобавление новой категории.");
        Console.Write("Введите название категории: ");
        string catName = Console.ReadLine();
        Console.Write("Укажите тип категории (Income/Expense): ");
        string catTypeInput = Console.ReadLine();
        if (!Enum.TryParse(catTypeInput, true, out CategoryType catType))
        {
            Console.WriteLine("Ошибка: неверный тип. Используйте 'Income' или 'Expense'.");
            Console.ReadKey();
            return;
        }
        try
        {
            var category = facade.CreateCategory(catType, catName);
            Console.WriteLine("Создана категория: " + category);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка: " + ex.Message);
        }
        Console.WriteLine("Нажмите любую клавишу для продолжения.");
        Console.ReadKey();
    }

    static void AddOperation(FinancialFacade facade)
    {
        Console.WriteLine("\nДобавление новой операции.");
        Console.Write("Введите ID счёта: ");
        string accountIdInput = Console.ReadLine();
        if (!long.TryParse(accountIdInput, out long accountId))
        {
            Console.WriteLine("Ошибка: неверный формат ID счёта. Введите число.");
            Console.WriteLine("Нажмите любую клавишу для продолжения.");
            Console.ReadKey();
            return;
        }
        Console.Write("Введите ID категории: ");
        string categoryIdInput = Console.ReadLine();
        if (!long.TryParse(categoryIdInput, out long categoryId))
        {
            Console.WriteLine("Ошибка: неверный формат ID категории. Введите число.");
            Console.WriteLine("Нажмите любую клавишу для продолжения.");
            Console.ReadKey();
            return;
        }
        Console.Write("Укажите тип операции (Income/Expense): ");
        string opTypeInput = Console.ReadLine();
        if (!Enum.TryParse(opTypeInput, true, out OperationType operationType))
        {
            Console.WriteLine("Ошибка: неверный тип операции.");
            Console.ReadKey();
            return;
        }
        Console.Write("Введите сумму операции: ");
        string amountStr = Console.ReadLine();
        if (!decimal.TryParse(amountStr, out decimal amount))
        {
            Console.WriteLine("Ошибка: неверный формат суммы операции. Введите числовое значение.");
            Console.WriteLine("Нажмите любую клавишу для продолжения.");
            Console.ReadKey();
            return;
        }
        Console.Write("Введите дату операции (например, 2025-03-14): ");
        DateTime opDate = DateTime.Parse(Console.ReadLine());
        Console.Write("Введите описание операции: ");
        string opDescription = Console.ReadLine();
        try
        {
            var operation = facade.CreateOperation(operationType, accountId, amount, opDate, opDescription, categoryId);
            Console.WriteLine("Создана операция: " + operation);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка: " + ex.Message);
        }
        Console.WriteLine("Нажмите любую клавишу для продолжения.");
        Console.ReadKey();
    }

    static void ModifyAccount(FinancialFacade facade)
    {
        Console.Clear();
        Console.WriteLine("Изменение данных банковского счёта.");

        var accounts = facade.GetBankAccounts().ToList();
        if (accounts.Count == 0)
        {
            Console.WriteLine("Нет доступных счетов.");
            return;
        }

        Console.WriteLine("Доступные счета:");
        foreach (var account in accounts)
        {
            Console.WriteLine($"ID: {account.Id}, Название: {account.Name}, Баланс: {account.Balance}");
        }

        long accountId;
        while (true)
        {
            Console.Write("Введите ID счёта для изменения (или 'exit' для отмены): ");
            string input = Console.ReadLine();
            if (input.Trim().ToLower() == "exit") return;
            if (!long.TryParse(input, out accountId))
            {
                Console.WriteLine("Ошибка: неверный формат ID. Попробуйте ещё раз.");
                continue;
            }
            if (!accounts.Any(a => a.Id == accountId))
            {
                Console.WriteLine("Счёт с данным ID не найден. Попробуйте ещё раз.");
                continue;
            }
            break;
        }

        Console.Write("Введите новое название счёта (или 'exit' для отмены): ");
        string newName = Console.ReadLine();
        if (newName.Trim().ToLower() == "exit") return;

        decimal newBalance;
        while (true)
        {
            Console.Write("Введите новый баланс (или 'exit' для отмены): ");
            string balanceInput = Console.ReadLine();
            if (balanceInput.Trim().ToLower() == "exit") return;
            if (!decimal.TryParse(balanceInput, out newBalance))
            {
                Console.WriteLine("Ошибка: неверный формат баланса. Попробуйте ещё раз.");
                continue;
            }
            break;
        }

        try
        {
            facade.EditBankAccount(accountId, newName, newBalance);
            Console.WriteLine("Счёт успешно обновлён.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка: " + ex.Message);
        }
    }

    static void RemoveAccount(FinancialFacade facade)
    {
        Console.Clear();
        Console.WriteLine("Удаление банковского счёта.");

        var accounts = facade.GetBankAccounts().ToList();
        if (accounts.Count == 0)
        {
            Console.WriteLine("Счета отсутствуют.");
            return;
        }

        Console.WriteLine("Доступные счета:");
        foreach (var account in accounts)
        {
            Console.WriteLine($"ID: {account.Id}, Название: {account.Name}, Баланс: {account.Balance}");
        }

        long accountId;
        while (true)
        {
            Console.Write("Введите ID счёта для удаления (или 'exit' для отмены): ");
            string input = Console.ReadLine();
            if (input.Trim().ToLower() == "exit") return;
            if (!long.TryParse(input, out accountId))
            {
                Console.WriteLine("Ошибка: неверный формат ID. Попробуйте ещё раз.");
                continue;
            }
            if (!accounts.Any(a => a.Id == accountId))
            {
                Console.WriteLine("Счёт с данным ID не найден. Попробуйте ещё раз.");
                continue;
            }
            break;
        }

        try
        {
            facade.DeleteBankAccount(accountId);
            Console.WriteLine("Счёт успешно удалён.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка: " + ex.Message);
        }
    }

    static void ModifyCategory(FinancialFacade facade)
    {
        Console.Clear();
        Console.WriteLine("Изменение данных категории.");

        var categories = facade.GetCategories().ToList();
        if (categories.Count == 0)
        {
            Console.WriteLine("Нет доступных категорий.");
            return;
        }

        Console.WriteLine("Доступные категории:");
        foreach (var cat in categories)
        {
            Console.WriteLine($"ID: {cat.Id}, Название: {cat.Name}, Тип: {cat.Type}");
        }

        long categoryId;
        while (true)
        {
            Console.Write("Введите ID категории для изменения (или 'exit' для отмены): ");
            string input = Console.ReadLine();
            if (input.Trim().ToLower() == "exit") return;
            if (!long.TryParse(input, out categoryId))
            {
                Console.WriteLine("Ошибка: неверный формат ID. Попробуйте ещё раз.");
                continue;
            }
            if (!categories.Any(c => c.Id == categoryId))
            {
                Console.WriteLine("Категория с данным ID не найдена. Попробуйте ещё раз.");
                continue;
            }
            break;
        }

        Console.Write("Введите новое название категории (или 'exit' для отмены): ");
        string newName = Console.ReadLine();
        if (newName.Trim().ToLower() == "exit") return;

        try
        {
            facade.EditCategory(categoryId, newName);
            Console.WriteLine("Категория успешно обновлена.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка: " + ex.Message);
        }
    }

    static void RemoveCategory(FinancialFacade facade)
    {
        Console.Clear();
        Console.WriteLine("Удаление категории.");

        var categories = facade.GetCategories().ToList();
        if (categories.Count == 0)
        {
            Console.WriteLine("Категории отсутствуют.");
            return;
        }

        Console.WriteLine("Доступные категории:");
        foreach (var cat in categories)
        {
            Console.WriteLine($"ID: {cat.Id}, Название: {cat.Name}, Тип: {cat.Type}");
        }

        long categoryId;
        while (true)
        {
            Console.Write("Введите ID категории для удаления (или 'exit' для отмены): ");
            string input = Console.ReadLine();
            if (input.Trim().ToLower() == "exit") return;
            if (!long.TryParse(input, out categoryId))
            {
                Console.WriteLine("Ошибка: неверный формат ID. Попробуйте ещё раз.");
                continue;
            }
            if (!categories.Any(c => c.Id == categoryId))
            {
                Console.WriteLine("Категория с данным ID не найдена. Попробуйте ещё раз.");
                continue;
            }
            break;
        }

        try
        {
            facade.DeleteCategory(categoryId);
            Console.WriteLine("Категория успешно удалена.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка: " + ex.Message);
        }
    }

    static void ModifyOperation(FinancialFacade facade)
    {
        Console.Clear();
        Console.WriteLine("Изменение данных операции.");

        var operations = facade.GetOperations().ToList();
        if (operations.Count == 0)
        {
            Console.WriteLine("Операции отсутствуют.");
            return;
        }

        Console.WriteLine("Доступные операции:");
        foreach (var op in operations)
        {
            Console.WriteLine($"ID: {op.Id}, Тип: {op.Type}, Сумма: {op.Amount}, Дата: {op.Date.ToShortDateString()}, Описание: {op.Description}");
        }

        long opId;
        while (true)
        {
            Console.Write("Введите ID операции для изменения (или 'exit' для отмены): ");
            string input = Console.ReadLine();
            if (input.Trim().ToLower() == "exit") return;
            if (!long.TryParse(input, out opId))
            {
                Console.WriteLine("Ошибка: неверный формат ID. Попробуйте ещё раз.");
                continue;
            }
            if (!operations.Any(o => o.Id == opId))
            {
                Console.WriteLine("Операция с данным ID не найдена. Попробуйте ещё раз.");
                continue;
            }
            break;
        }

        decimal newAmount;
        while (true)
        {
            Console.Write("Введите новую сумму операции (или 'exit' для отмены): ");
            string amountInput = Console.ReadLine();
            if (amountInput.Trim().ToLower() == "exit") return;
            if (!decimal.TryParse(amountInput, out newAmount))
            {
                Console.WriteLine("Ошибка: неверный формат суммы. Попробуйте ещё раз.");
                continue;
            }
            break;
        }

        DateTime newDate;
        while (true)
        {
            Console.Write("Введите новую дату операции (формат: yyyy-MM-dd) (или 'exit' для отмены): ");
            string dateInput = Console.ReadLine();
            if (dateInput.Trim().ToLower() == "exit") return;
            if (!DateTime.TryParse(dateInput, out newDate))
            {
                Console.WriteLine("Ошибка: неверный формат даты. Попробуйте ещё раз.");
                continue;
            }
            break;
        }

        Console.Write("Введите новое описание операции (или 'exit' для отмены): ");
        string newDescription = Console.ReadLine();
        if (newDescription.Trim().ToLower() == "exit") return;

        try
        {
            facade.EditOperation(opId, newAmount, newDate, newDescription);
            Console.WriteLine("Операция успешно обновлена.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка: " + ex.Message);
        }
    }

    static void RemoveOperation(FinancialFacade facade)
    {
        Console.Clear();
        Console.WriteLine("Удаление операции.");

        var operations = facade.GetOperations().ToList();
        if (operations.Count == 0)
        {
            Console.WriteLine("Операции отсутствуют.");
            return;
        }

        Console.WriteLine("Доступные операции:");
        foreach (var op in operations)
        {
            Console.WriteLine($"ID: {op.Id}, Тип: {op.Type}, Сумма: {op.Amount}, Дата: {op.Date.ToShortDateString()}, Описание: {op.Description}");
        }

        long opId;
        while (true)
        {
            Console.Write("Введите ID операции для удаления (или 'exit' для отмены): ");
            string input = Console.ReadLine();
            if (input.Trim().ToLower() == "exit") return;
            if (!long.TryParse(input, out opId))
            {
                Console.WriteLine("Ошибка: неверный формат ID. Попробуйте ещё раз.");
                continue;
            }
            if (!operations.Any(o => o.Id == opId))
            {
                Console.WriteLine("Операция с данным ID не найдена. Попробуйте ещё раз.");
                continue;
            }
            break;
        }

        try
        {
            facade.DeleteOperation(opId);
            Console.WriteLine("Операция успешно удалена.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка: " + ex.Message);
        }
    }

    static void DisplayIncomeExpenseDifference(FinancialFacade facade)
    {
        Console.WriteLine("\nАнализ: разница между доходами и расходами.");
        Console.Write("Введите дату начала периода (например, 2025-03-01): ");
        DateTime startPeriod = DateTime.Parse(Console.ReadLine());
        Console.Write("Введите дату окончания периода (например, 2025-03-31): ");
        DateTime endPeriod = DateTime.Parse(Console.ReadLine());
        decimal difference = facade.GetIncomeExpenseDifference(startPeriod, endPeriod);
        Console.WriteLine("Разница доходов и расходов составляет: " + difference);
        Console.WriteLine("Нажмите любую клавишу для продолжения.");
        Console.ReadKey();
    }

    static void DisplayGroupedOperations(FinancialFacade facade)
    {
        Console.Clear();
        Console.WriteLine("Анализ: группировка операций по категориям.");

        var groupedData = facade.GroupOperationsByCategory();
        if (groupedData.Count == 0)
        {
            Console.WriteLine("Нет операций для анализа.");
        }
        else
        {
            foreach (var group in groupedData)
            {
                Console.WriteLine($"Категория: {group.Key}");
                Console.WriteLine($"  Доходы: {group.Value.Income}");
                Console.WriteLine($"  Расходы: {group.Value.Expense}");
                Console.WriteLine();
            }
        }

        Console.WriteLine("Нажмите любую клавишу, чтобы вернуться в меню.");
        Console.ReadKey();
    }

    static void PerformDataExport(FinancialFacade facade)
    {
        Console.WriteLine("Экспорт данных.");
        Console.WriteLine("Выберите формат экспорта:");
        Console.WriteLine("1. CSV.");
        Console.WriteLine("2. JSON.");
        Console.WriteLine("3. YAML.");
        string formatChoice = Console.ReadLine();

        string pathAccounts = "accounts";
        string pathCategories = "categories";
        string pathOperations = "operations";
        IExportVisitor exportVisitor = null;

        switch (formatChoice)
        {
            case "1":
                exportVisitor = new CsvExportVisitor();
                pathAccounts += ".csv";
                pathCategories += ".csv";
                pathOperations += ".csv";
                break;
            case "2":
                exportVisitor = new JsonExportVisitor();
                pathAccounts += ".json";
                pathCategories += ".json";
                pathOperations += ".json";
                break;
            case "3":
                exportVisitor = new YamlExportVisitor();
                pathAccounts += ".yaml";
                pathCategories += ".yaml";
                pathOperations += ".yaml";
                break;
        }

        if (exportVisitor == null)
        {
            Console.WriteLine("Выбран неверный формат. Операция отменена.");
            Console.ReadKey();
            return;
        }

        facade.ExportData(exportVisitor);
        exportVisitor.SaveToFiles(pathAccounts, pathCategories, pathOperations);
        Console.WriteLine("Экспорт завершён. Нажмите любую клавишу для продолжения.");
        Console.ReadKey();
    }

    static void PerformDataImport(DataContext dataContext)
    {
        Console.WriteLine("Импорт данных.");
        Console.WriteLine("Выберите формат импорта:");
        Console.WriteLine("1. CSV.");
        Console.WriteLine("2. JSON.");
        Console.WriteLine("3. YAML.");
        string formatChoice = Console.ReadLine();
        Console.Write("Укажите путь к файлу: ");
        string filePath = Console.ReadLine();

        DataImporter importer = formatChoice switch
        {
            "1" => new CsvDataImporter(dataContext),
            "2" => new JsonDataImporter(dataContext),
            "3" => new YamlDataImporter(dataContext),
            _ => null
        };

        if (importer == null)
        {
            Console.WriteLine("Выбран неверный формат. Операция отменена.");
            Console.ReadKey();
            return;
        }

        try
        {
            importer.ImportData(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при импорте данных: " + ex.Message);
        }
        Console.WriteLine("Импорт завершён. Нажмите любую клавишу для продолжения.");
        Console.ReadKey();
    }
}
