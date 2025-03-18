using System;
using FinancialAccounting.Application.Facades;
using FinancialAccounting.Domain.Enums;
using FinancialAccounting.Infrastructure.Data;
using Xunit;

namespace FinancialAccounting.Tests;

public class FinancialFacadeTests
{
    private static DataContext GetEmptyDataContext() => new();

    [Fact]
    public void CreateBankAccount_ShouldAddAccountToDataContext()
    {
        // Arrange
        var dataContext = GetEmptyDataContext();
        var facade = new FinancialFacade(dataContext);
        string name = "Test Account";
        decimal balance = 500m;

        // Act
        var account = facade.CreateBankAccount(name, balance);

        // Assert
        Assert.Single(dataContext.BankAccounts);
        Assert.Equal(name, account.Name);
        Assert.Equal(balance, account.Balance);
    }

    [Fact]
    public void EditBankAccount_ShouldModifyAccount()
    {
        // Arrange
        var dataContext = GetEmptyDataContext();
        var facade = new FinancialFacade(dataContext);
        var account = facade.CreateBankAccount("Original", 100m);

        // Act
        facade.EditBankAccount(account.Id, "Modified", 200m);

        // Assert
        var edited = dataContext.BankAccounts.FirstOrDefault(a => a.Id == account.Id);
        Assert.NotNull(edited);
        Assert.Equal("Modified", edited.Name);
        Assert.Equal(200m, edited.Balance);
    }

    [Fact]
    public void DeleteBankAccount_ShouldRemoveAccountFromDataContext()
    {
        // Arrange
        var dataContext = GetEmptyDataContext();
        var facade = new FinancialFacade(dataContext);
        var account = facade.CreateBankAccount("To Delete", 300m);

        // Act
        facade.DeleteBankAccount(account.Id);

        // Assert
        Assert.Empty(dataContext.BankAccounts);
    }

    [Fact]
    public void CreateCategory_ShouldAddCategory()
    {
        // Arrange
        var dataContext = GetEmptyDataContext();
        var facade = new FinancialFacade(dataContext);
        string name = "Food";

        // Act
        var category = facade.CreateCategory(CategoryType.Expense, name);

        // Assert
        Assert.Single(dataContext.Categories);
        Assert.Equal(name, category.Name);
        Assert.Equal(CategoryType.Expense, category.Type);
    }

    [Fact]
    public void EditCategory_ShouldModifyCategory()
    {
        // Arrange
        var dataContext = GetEmptyDataContext();
        var facade = new FinancialFacade(dataContext);
        var category = facade.CreateCategory(CategoryType.Income, "Old Name");

        // Act
        facade.EditCategory(category.Id, "New Name");

        // Assert
        var edited = dataContext.Categories.FirstOrDefault(c => c.Id == category.Id);
        Assert.NotNull(edited);
        Assert.Equal("New Name", edited.Name);
    }

    [Fact]
    public void DeleteCategory_ShouldRemoveCategory()
    {
        // Arrange
        var dataContext = GetEmptyDataContext();
        var facade = new FinancialFacade(dataContext);
        var category = facade.CreateCategory(CategoryType.Income, "To Remove");

        // Act
        facade.DeleteCategory(category.Id);

        // Assert
        Assert.Empty(dataContext.Categories);
    }

    [Fact]
    public void CreateOperation_ShouldAddOperationAndUpdateBalance()
    {
        // Arrange
        var dataContext = GetEmptyDataContext();
        var facade = new FinancialFacade(dataContext);
        var account = facade.CreateBankAccount("Operation Test", 1000m);
        var category = facade.CreateCategory(CategoryType.Expense, "Food");

        // Act: создаём операцию расхода
        var operation = facade.CreateOperation(OperationType.Expense, account.Id, 200m, DateTime.Now, "Dinner", category.Id);

        // Assert
        Assert.Single(dataContext.Operations);
        Assert.Equal(OperationType.Expense, operation.Type);
        // Баланс должен уменьшиться на 200
        var updatedAccount = dataContext.BankAccounts.First(a => a.Id == account.Id);
        Assert.Equal(800m, updatedAccount.Balance);
    }

    [Fact]
    public void GetIncomeExpenseDifference_ShouldReturnCorrectDifference()
    {
        // Arrange
        var dataContext = GetEmptyDataContext();
        var facade = new FinancialFacade(dataContext);
        var account = facade.CreateBankAccount("Test", 1000m);
        var incomeCategory = facade.CreateCategory(CategoryType.Income, "Salary");
        var expenseCategory = facade.CreateCategory(CategoryType.Expense, "Food");

        // Создаём операции
        facade.CreateOperation(OperationType.Income, account.Id, 2000m, DateTime.Now, "Salary Payment", incomeCategory.Id);
        facade.CreateOperation(OperationType.Expense, account.Id, 500m, DateTime.Now, "Dinner", expenseCategory.Id);

        // Act
        decimal difference = facade.GetIncomeExpenseDifference(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));

        // Assert
        Assert.Equal(1500m, difference); // 2000 - 500
    }

    [Fact]
    public void GroupOperationsByCategory_ShouldReturnGroupedValues()
    {
        // Arrange
        var dataContext = GetEmptyDataContext();
        var facade = new FinancialFacade(dataContext);
        var account = facade.CreateBankAccount("Test", 1000m);
        var incomeCategory = facade.CreateCategory(CategoryType.Income, "Salary");
        var expenseCategory = facade.CreateCategory(CategoryType.Expense, "Food");

        facade.CreateOperation(OperationType.Income, account.Id, 2000m, DateTime.Now, "Salary Payment", incomeCategory.Id);
        facade.CreateOperation(OperationType.Expense, account.Id, 300m, DateTime.Now, "Lunch", expenseCategory.Id);
        facade.CreateOperation(OperationType.Expense, account.Id, 200m, DateTime.Now, "Snack", expenseCategory.Id);

        // Act
        var groups = facade.GroupOperationsByCategory();

        // Assert
        Assert.True(groups.ContainsKey("Salary"));
        Assert.True(groups.ContainsKey("Food"));
        Assert.Equal(2000m, groups["Salary"].Income);
        Assert.Equal(500m, groups["Food"].Expense);
    }
}
