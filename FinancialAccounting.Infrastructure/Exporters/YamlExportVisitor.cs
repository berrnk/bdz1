using System;
using System.Globalization;
using FinancialAccounting.Domain.Entities;

namespace FinancialAccounting.Infrastructure.Exporters;

public class YamlExportVisitor : IExportVisitor
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
        // Формирование YAML для счетов
        var accountsYaml = new List<string>();
        accountsYaml.Add("Accounts:");
        foreach (var account in _accounts)
        {
            accountsYaml.Add($"  - Id: {account.Id}");
            accountsYaml.Add($"    Name: \"{EscapeYaml(account.Name)}\"");
            accountsYaml.Add($"    Balance: {account.Balance.ToString(CultureInfo.InvariantCulture)}");
        }
        File.WriteAllLines(accountsFilePath, accountsYaml);

        // Формирование YAML для категорий
        var categoriesYaml = new List<string>();
        categoriesYaml.Add("Categories:");
        foreach (var category in _categories)
        {
            categoriesYaml.Add($"  - Id: {category.Id}");
            categoriesYaml.Add($"    Name: \"{EscapeYaml(category.Name)}\"");
            categoriesYaml.Add($"    Type: {category.Type}");
        }
        File.WriteAllLines(categoriesFilePath, categoriesYaml);

        // Формирование YAML для операций
        var operationsYaml = new List<string>();
        operationsYaml.Add("Operations:");
        foreach (var operation in _operations)
        {
            operationsYaml.Add($"  - Id: {operation.Id}");
            operationsYaml.Add($"    Type: {operation.Type}");
            operationsYaml.Add($"    Amount: {operation.Amount.ToString(CultureInfo.InvariantCulture)}");
            operationsYaml.Add($"    Date: {operation.Date:yyyy-MM-dd}");
            operationsYaml.Add($"    Description: \"{EscapeYaml(operation.Description)}\"");
            operationsYaml.Add($"    CategoryId: {operation.CategoryId}");
            operationsYaml.Add($"    BankAccountId: {operation.BankAccountId}");
        }
        File.WriteAllLines(operationsFilePath, operationsYaml);
    }

    // Простой метод экранирования строк для YAML (заменяет двойные кавычки)
    private string EscapeYaml(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";
        return input.Replace("\"", "\\\"");
    }
}

