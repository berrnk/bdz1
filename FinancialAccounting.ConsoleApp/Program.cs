using FinancialAccounting.Application.Commands;
using FinancialAccounting.Application.Facades;
using FinancialAccounting.Domain.Enums;
using FinancialAccounting.Infrastructure.Data;
using FinancialAccounting.Infrastructure.Exporters;
using FinancialAccounting.Infrastructure.Importers;
using Microsoft.Extensions.DependencyInjection;
using ICommand = FinancialAccounting.Application.Commands.ICommand;

namespace FinancialAccounting.ConsoleApp;

class Program
{
    static void Main(string[] args)
    {
        // Настройка DI-контейнера
        var serviceProvider = new ServiceCollection()
            // Регистрируем единый экземпляр DataContext для всего приложения
            .AddSingleton<DataContext>()
            // FinancialFacade зависит от DataContext, поэтому регистрируем как Singleton
            .AddSingleton<FinancialFacade>()
            // Регистрируем команды, импортёры и экспортёры как Transient (если нужно)
            .AddTransient<JsonExportVisitor>()
            .AddTransient<YamlExportVisitor>()
            .AddTransient<CsvExportVisitor>()
            .AddTransient<JsonDataImporter>()
            .AddTransient<YamlDataImporter>()
            .AddTransient<CsvDataImporter>()
            .BuildServiceProvider();

        // Разрешаем зависимости из контейнера
        var facade = serviceProvider.GetRequiredService<FinancialFacade>();
        var dataContext = serviceProvider.GetRequiredService<DataContext>();

        // Запускаем главное меню приложения
        RunMenu(facade, dataContext);
    }

    static void RunMenu(FinancialFacade facade, DataContext dataContext)
    {
        bool exit = false;
        while (!exit)
        {

            Console.WriteLine("\n\nМодуль 'Учет финансов' - Главное меню");
            Console.WriteLine("1. Создать счет");
            Console.WriteLine("2. Редактировать счет");
            Console.WriteLine("3. Удалить счет");
            Console.WriteLine("4. Создать категорию");
            Console.WriteLine("5. Редактировать категорию");
            Console.WriteLine("6. Удалить категорию");
            Console.WriteLine("7. Создать операцию");
            Console.WriteLine("8. Редактировать операцию");
            Console.WriteLine("9. Удалить операцию");
            Console.WriteLine("10. Аналитика: Разница доходов и расходов");
            Console.WriteLine("11. Аналитика: Группировка по категориям");
            Console.WriteLine("12. Экспорт данных");
            Console.WriteLine("13. Импорт данных");
            Console.WriteLine("0. Выход\n\n");
            Console.Write("Выберите опцию: ");
            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    CreateAccount(facade);
                    break;
                case "2":
                    EditAccount(facade);
                    break;
                case "3":
                    DeleteAccount(facade);
                    break;
                case "4":
                    CreateCategory(facade);
                    break;
                case "5":
                    EditCategory(facade);
                    break;
                case "6":
                    DeleteCategory(facade);
                    break;
                case "7":
                    CreateOperation(facade);
                    break;
                case "8":
                    EditOperation(facade);
                    break;
                case "9":
                    DeleteOperation(facade);
                    break;
                case "10":
                    ShowIncomeExpenseDifference(facade);
                    break;
                case "11":
                    ShowGroupedOperations(facade);
                    break;
                case "12":
                    ExportData(facade);
                    break;
                case "13":
                    ImportData(dataContext);
                    break;
                case "0":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Неверная опция. Нажмите любую клавишу для продолжения...\n\n");
                    Console.ReadKey();
                    break;
            }
        }
    }

    static void CreateAccount(FinancialFacade facade)
    {

        Console.WriteLine("Создание нового счета");
        Console.Write("Введите название счета: ");
        string name = Console.ReadLine();
        Console.Write("Введите начальный баланс: ");
        string balanceInput = Console.ReadLine();
        if (!decimal.TryParse(balanceInput, out decimal balance))
        {
            Console.WriteLine("Некорректный формат баланса. Пожалуйста, введите число.");
            Console.WriteLine("Нажмите любую клавишу для продолжения...\n\n");
            Console.ReadKey();
            return;
        }

        // Применяем команду с декоратором для измерения времени выполнения
        ICommand createAccountCmd = new CreateAccountCommand(facade, name, balance);
        ICommand timedCmd = new TimeMeasuredCommandDecorator(createAccountCmd);
        timedCmd.Execute();

        Console.WriteLine("Нажмите любую клавишу для продолжения...\n\n");
        Console.ReadKey();
    }

    static void CreateCategory(FinancialFacade facade)
    {

        Console.WriteLine("Создание категории");
        Console.Write("Введите название категории: ");
        string name = Console.ReadLine();
        Console.Write("Введите тип категории (Income/Expense): ");
        string typeInput = Console.ReadLine();
        if (!Enum.TryParse(typeInput, true, out CategoryType type))
        {
            Console.WriteLine("Неверный тип. Используйте 'Income' или 'Expense'.");
            Console.ReadKey();
            return;
        }
        try
        {
            var category = facade.CreateCategory(type, name);
            Console.WriteLine("Категория создана: " + category);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
        Console.WriteLine("Нажмите любую клавишу для продолжения...\n\n");
        Console.ReadKey();
    }

    static void CreateOperation(FinancialFacade facade)
    {

        Console.WriteLine("Создание операции");
        Console.Write("Введите ID счета: ");
        string accountIdInput = Console.ReadLine();
        if (!long.TryParse(accountIdInput, out long accountId))
        {
            Console.WriteLine("Некорректный формат ID счета. Пожалуйста, введите число.");
            Console.WriteLine("Нажмите любую клавишу для продолжения...\n\n");
            Console.ReadKey();
            return;
        }
        Console.Write("Введите ID категории: ");
        string cateogoryIdInput = Console.ReadLine();
        if (!long.TryParse(cateogoryIdInput, out long categoryId))
        {
            Console.WriteLine("Некорректный формат ID категории. Пожалуйста, введите число.");
            Console.WriteLine("Нажмите любую клавишу для продолжения...\n\n");
            Console.ReadKey();
            return;
        }
        Console.Write("Введите тип операции (Income/Expense): ");
        string typeInput = Console.ReadLine();
        if (!Enum.TryParse(typeInput, true, out OperationType opType))
        {
            Console.WriteLine("Неверный тип операции.\n\n");
            Console.ReadKey();
            return;
        }
        Console.Write("Введите сумму операции: ");
        string amountInput = Console.ReadLine();
        if (!decimal.TryParse(amountInput, out decimal amount))
        {
            Console.WriteLine("Некорректный формат суммы операции. Пожалуйста, введите число.");
            Console.WriteLine("Нажмите любую клавишу для продолжения...\n\n");
            Console.ReadKey();
            return;
        }
        Console.Write("Введите дату операции (например, 2025-03-14): ");
        DateTime date = DateTime.Parse(Console.ReadLine());
        Console.Write("Введите описание операции: ");
        string description = Console.ReadLine();
        try
        {
            var operation = facade.CreateOperation(opType, accountId, amount, date, description, categoryId);
            Console.WriteLine("Операция создана: " + operation);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
        Console.WriteLine("Нажмите любую клавишу для продолжения...\n\n");
        Console.ReadKey();
    }

    static void EditAccount(FinancialFacade facade)
    {
        Console.Clear();
        Console.WriteLine("Редактирование счета");

        var accounts = facade.GetBankAccounts().ToList();
        if (accounts.Count == 0)
        {
            Console.WriteLine("Нет созданных счетов.");
            return;
        }

        Console.WriteLine("Список счетов:");
        foreach (var account in accounts)
        {
            Console.WriteLine($"ID: {account.Id}, Название: {account.Name}, Баланс: {account.Balance}");
        }

        long accountId;
        while (true)
        {
            Console.Write("Введите ID счета для редактирования (или 'exit' для возврата): ");
            string input = Console.ReadLine();
            if (input.Trim().ToLower() == "exit") return;
            if (!long.TryParse(input, out accountId))
            {
                Console.WriteLine("Некорректный формат ID. Попробуйте снова.");
                continue;
            }
            if (!accounts.Any(a => a.Id == accountId))
            {
                Console.WriteLine("Счет с таким ID не найден. Попробуйте снова.");
                continue;
            }
            break;
        }

        Console.Write("Введите новое название счета (или 'exit' для возврата): ");
        string newName = Console.ReadLine();
        if (newName.Trim().ToLower() == "exit") return;

        decimal newBalance;
        while (true)
        {
            Console.Write("Введите новый баланс (или 'exit' для возврата): ");
            string balInput = Console.ReadLine();
            if (balInput.Trim().ToLower() == "exit") return;
            if (!decimal.TryParse(balInput, out newBalance))
            {
                Console.WriteLine("Некорректный формат баланса. Попробуйте снова.");
                continue;
            }
            break;
        }

        try
        {
            facade.EditBankAccount(accountId, newName, newBalance);
            Console.WriteLine("Счет успешно изменен.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }

    static void DeleteAccount(FinancialFacade facade)
    {
        Console.Clear();
        Console.WriteLine("Удаление счета");

        var accounts = facade.GetBankAccounts().ToList();
        if (accounts.Count == 0)
        {
            Console.WriteLine("Нет созданных счетов.");
            return;
        }

        Console.WriteLine("Список счетов:");
        foreach (var account in accounts)
        {
            Console.WriteLine($"ID: {account.Id}, Название: {account.Name}, Баланс: {account.Balance}");
        }

        long accountId;
        while (true)
        {
            Console.Write("Введите ID счета для удаления (или 'exit' для возврата): ");
            string input = Console.ReadLine();
            if (input.Trim().ToLower() == "exit") return;
            if (!long.TryParse(input, out accountId))
            {
                Console.WriteLine("Некорректный формат ID. Попробуйте снова.");
                continue;
            }
            if (!accounts.Any(a => a.Id == accountId))
            {
                Console.WriteLine("Счет с таким ID не найден. Попробуйте снова.");
                continue;
            }
            break;
        }

        try
        {
            facade.DeleteBankAccount(accountId);
            Console.WriteLine("Счет успешно удален.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }

    static void EditCategory(FinancialFacade facade)
    {
        Console.Clear();
        Console.WriteLine("Редактирование категории");

        var categories = facade.GetCategories().ToList();
        if (categories.Count == 0)
        {
            Console.WriteLine("Нет созданных категорий.");
            return;
        }

        Console.WriteLine("Список категорий:");
        foreach (var cat in categories)
        {
            Console.WriteLine($"ID: {cat.Id}, Название: {cat.Name}, Тип: {cat.Type}");
        }

        long categoryId;
        while (true)
        {
            Console.Write("Введите ID категории для редактирования (или 'exit' для возврата): ");
            string input = Console.ReadLine();
            if (input.Trim().ToLower() == "exit") return;
            if (!long.TryParse(input, out categoryId))
            {
                Console.WriteLine("Некорректный формат ID. Попробуйте снова.");
                continue;
            }
            if (!categories.Any(c => c.Id == categoryId))
            {
                Console.WriteLine("Категория с таким ID не найдена. Попробуйте снова.");
                continue;
            }
            break;
        }

        Console.Write("Введите новое название категории (или 'exit' для возврата): ");
        string newName = Console.ReadLine();
        if (newName.Trim().ToLower() == "exit") return;

        try
        {
            facade.EditCategory(categoryId, newName);
            Console.WriteLine("Категория успешно изменена.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }

    static void DeleteCategory(FinancialFacade facade)
    {
        Console.Clear();
        Console.WriteLine("Удаление категории");

        var categories = facade.GetCategories().ToList();
        if (categories.Count == 0)
        {
            Console.WriteLine("Нет созданных категорий.");
            return;
        }

        Console.WriteLine("Список категорий:");
        foreach (var cat in categories)
        {
            Console.WriteLine($"ID: {cat.Id}, Название: {cat.Name}, Тип: {cat.Type}");
        }

        long categoryId;
        while (true)
        {
            Console.Write("Введите ID категории для удаления (или 'exit' для возврата): ");
            string input = Console.ReadLine();
            if (input.Trim().ToLower() == "exit") return;
            if (!long.TryParse(input, out categoryId))
            {
                Console.WriteLine("Некорректный формат ID. Попробуйте снова.");
                continue;
            }
            if (!categories.Any(c => c.Id == categoryId))
            {
                Console.WriteLine("Категория с таким ID не найдена. Попробуйте снова.");
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
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }

    static void EditOperation(FinancialFacade facade)
    {
        Console.Clear();
        Console.WriteLine("Редактирование операции");

        var operations = facade.GetOperations().ToList();
        if (operations.Count == 0)
        {
            Console.WriteLine("Нет созданных операций.");
            return;
        }

        Console.WriteLine("Список операций:");
        foreach (var op in operations)
        {
            Console.WriteLine($"ID: {op.Id}, Тип: {op.Type}, Сумма: {op.Amount}, Дата: {op.Date.ToShortDateString()}, Описание: {op.Description}");
        }

        long opId;
        while (true)
        {
            Console.Write("Введите ID операции для редактирования (или 'exit' для возврата): ");
            string input = Console.ReadLine();
            if (input.Trim().ToLower() == "exit") return;
            if (!long.TryParse(input, out opId))
            {
                Console.WriteLine("Некорректный формат ID. Попробуйте снова.");
                continue;
            }
            if (!operations.Any(o => o.Id == opId))
            {
                Console.WriteLine("Операция с таким ID не найдена. Попробуйте снова.");
                continue;
            }
            break;
        }

        decimal newAmount;
        while (true)
        {
            Console.Write("Введите новую сумму операции (или 'exit' для возврата): ");
            string input = Console.ReadLine();
            if (input.Trim().ToLower() == "exit") return;
            if (!decimal.TryParse(input, out newAmount))
            {
                Console.WriteLine("Некорректный формат суммы. Попробуйте снова.");
                continue;
            }
            break;
        }

        DateTime newDate;
        while (true)
        {
            Console.Write("Введите новую дату операции (формат: yyyy-MM-dd) (или 'exit' для возврата): ");
            string input = Console.ReadLine();
            if (input.Trim().ToLower() == "exit") return;
            if (!DateTime.TryParse(input, out newDate))
            {
                Console.WriteLine("Некорректный формат даты. Попробуйте снова.");
                continue;
            }
            break;
        }

        Console.Write("Введите новое описание операции (или 'exit' для возврата): ");
        string newDescription = Console.ReadLine();
        if (newDescription.Trim().ToLower() == "exit") return;

        try
        {
            facade.EditOperation(opId, newAmount, newDate, newDescription);
            Console.WriteLine("Операция успешно изменена.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }

    static void DeleteOperation(FinancialFacade facade)
    {
        Console.Clear();
        Console.WriteLine("Удаление операции");

        var operations = facade.GetOperations().ToList();
        if (operations.Count == 0)
        {
            Console.WriteLine("Нет созданных операций.");
            return;
        }

        Console.WriteLine("Список операций:");
        foreach (var op in operations)
        {
            Console.WriteLine($"ID: {op.Id}, Тип: {op.Type}, Сумма: {op.Amount}, Дата: {op.Date.ToShortDateString()}, Описание: {op.Description}");
        }

        long opId;
        while (true)
        {
            Console.Write("Введите ID операции для удаления (или 'exit' для возврата): ");
            string input = Console.ReadLine();
            if (input.Trim().ToLower() == "exit") return;
            if (!long.TryParse(input, out opId))
            {
                Console.WriteLine("Некорректный формат ID. Попробуйте снова.");
                continue;
            }
            if (!operations.Any(o => o.Id == opId))
            {
                Console.WriteLine("Операция с таким ID не найдена. Попробуйте снова.");
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
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }

    static void ShowIncomeExpenseDifference(FinancialFacade facade)
    {

        Console.WriteLine("Аналитика: Разница доходов и расходов");
        Console.Write("Введите начальную дату (например, 2025-03-01): ");
        DateTime startDate = DateTime.Parse(Console.ReadLine());
        Console.Write("Введите конечную дату (например, 2025-03-31): ");
        DateTime endDate = DateTime.Parse(Console.ReadLine());
        decimal diff = facade.GetIncomeExpenseDifference(startDate, endDate);
        Console.WriteLine("Разница доходов и расходов: " + diff);
        Console.WriteLine("Нажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    static void ShowGroupedOperations(FinancialFacade facade)
    {
        Console.Clear();
        Console.WriteLine("Группировка доходов и расходов по категориям");

        var groups = facade.GroupOperationsByCategory();
        if (groups.Count == 0)
        {
            Console.WriteLine("Нет операций для группировки.");
        }
        else
        {
            foreach (var group in groups)
            {
                Console.WriteLine($"Категория: {group.Key}");
                Console.WriteLine($"  Доходы: {group.Value.Income}");
                Console.WriteLine($"  Расходы: {group.Value.Expense}");
                Console.WriteLine();
            }
        }

        Console.WriteLine("Нажмите любую клавишу для возврата в главное меню...");
        Console.ReadKey();
    }


    static void ExportData(FinancialFacade facade)
    {

        Console.WriteLine("Экспорт данных");
        Console.WriteLine("Выберите формат экспорта:");
        Console.WriteLine("1. CSV");
        Console.WriteLine("2. JSON");
        Console.WriteLine("3. YAML");
        string formatOption = Console.ReadLine();

        string accountsFilePath = "accounts";
        string categoriesFilePath = "categories";
        string operationsFilePath = "operations";
        IExportVisitor visitor = null;

        switch (formatOption)
        {
            case "1":
                visitor = new CsvExportVisitor();
                accountsFilePath += ".csv";
                categoriesFilePath += ".csv";
                operationsFilePath += ".csv";
                break;
            case "2":
                visitor = new JsonExportVisitor();
                accountsFilePath += ".json";
                categoriesFilePath += ".json";
                operationsFilePath += ".json";
                break;
            case "3":
                visitor = new YamlExportVisitor();
                accountsFilePath += ".yaml";
                categoriesFilePath += ".yaml";
                operationsFilePath += ".yaml";
                break;


        }

        if (visitor == null)
        {
            Console.WriteLine("Неверная опция.");
            Console.ReadKey();
            return;
        }


        facade.ExportData(visitor);


        visitor.SaveToFiles(accountsFilePath, categoriesFilePath, operationsFilePath);
        Console.WriteLine("Нажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    static void ImportData(DataContext dataContext)
    {

        Console.WriteLine("Импорт данных");
        Console.WriteLine("Выберите формат импорта:");
        Console.WriteLine("1. CSV");
        Console.WriteLine("2. JSON");
        Console.WriteLine("3. YAML");
        string formatOption = Console.ReadLine();
        Console.Write("Введите путь к файлу: ");
        string filePath = Console.ReadLine();

        DataImporter importer = formatOption switch
        {
            "1" => new CsvDataImporter(dataContext),
            "2" => new JsonDataImporter(dataContext),
            "3" => new YamlDataImporter(dataContext),
            _ => null
        };

        if (importer == null)
        {
            Console.WriteLine("Неверная опция.");
            Console.ReadKey();
            return;
        }

        try
        {
            importer.ImportData(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при импорте: " + ex.Message);
        }
        Console.WriteLine("Нажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }
}