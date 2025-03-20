using System;
using System.Linq;
using Accounting.Application.Facades;
using Accounting.Domain.Enums;
using Accounting.Infrastructure.Data;
using Xunit;

namespace Accounting.Tests
{
    public class FinancialFacadeTests
    {
        // Возвращает новый пустой контекст данных.
        private static DataContext CreateEmptyDataContext() => new();

        [Fact]
        public void Test_AddingBankAccount_UpdatesDataContext()
        {
            var context = CreateEmptyDataContext();
            var facade = new FinancialFacade(context);
            string accountName = "Test Account";
            decimal initialBalance = 500m;
            // Выполнение действия: создание нового банковского счёта.
            var account = facade.CreateBankAccount(accountName, initialBalance);

            // Проверка: в контексте должна появиться единственная запись, а свойства счета соответствуют заданным значениям.
            Assert.Single(context.BankAccounts);
            Assert.Equal(accountName, account.Name);
            Assert.Equal(initialBalance, account.Balance);
        }

        [Fact]
        public void Test_EditingBankAccount_ChangesProperties()
        {
            // Подготовка: создаём контекст и добавляем счёт.
            var context = CreateEmptyDataContext();
            var facade = new FinancialFacade(context);
            var account = facade.CreateBankAccount("Original", 100m);
            facade.EditBankAccount(account.Id, "Modified", 200m);

            // Проверка: найденный счёт должен иметь обновлённые значения.
            var updatedAccount = context.BankAccounts.FirstOrDefault(a => a.Id == account.Id);
            Assert.NotNull(updatedAccount);
            Assert.Equal("Modified", updatedAccount.Name);
            Assert.Equal(200m, updatedAccount.Balance);
        }

        [Fact]
        public void Test_DeletingBankAccount_RemovesItFromContext()
        {
            // Подготовка: создаём контекст и добавляем счёт.
            var context = CreateEmptyDataContext();
            var facade = new FinancialFacade(context);
            var account = facade.CreateBankAccount("To Delete", 300m);
            facade.DeleteBankAccount(account.Id);

            // Проверка: список счетов должен оказаться пустым.
            Assert.Empty(context.BankAccounts);
        }

        [Fact]
        public void Test_AddingCategory_SetsPropertiesCorrectly()
        {
            // Подготовка: создаём пустой контекст.
            var context = CreateEmptyDataContext();
            var facade = new FinancialFacade(context);
            string categoryName = "Food";

            // Выполнение: создаём новую категорию расходов.
            var category = facade.CreateCategory(CategoryType.Expense, categoryName);

            // Проверка: в контексте должна быть одна категория с соответствующими свойствами.
            Assert.Single(context.Categories);
            Assert.Equal(categoryName, category.Name);
            Assert.Equal(CategoryType.Expense, category.Type);
        }

        [Fact]
        public void Test_EditingCategory_UpdatesName()
        {
            // Подготовка: создаём контекст и добавляем категорию.
            var context = CreateEmptyDataContext();
            var facade = new FinancialFacade(context);
            var category = facade.CreateCategory(CategoryType.Income, "Old Name");

            // Выполнение: изменяем название категории.
            facade.EditCategory(category.Id, "New Name");

            // Проверка: у найденной категории должно быть новое имя.
            var updatedCategory = context.Categories.FirstOrDefault(c => c.Id == category.Id);
            Assert.NotNull(updatedCategory);
            Assert.Equal("New Name", updatedCategory.Name);
        }

        [Fact]
        public void Test_DeletingCategory_RemovesItFromContext()
        {
            // Подготовка: создаём контекст и добавляем категорию.
            var context = CreateEmptyDataContext();
            var facade = new FinancialFacade(context);
            var category = facade.CreateCategory(CategoryType.Income, "To Remove");

            // Выполнение: удаляем категорию.
            facade.DeleteCategory(category.Id);

            // Проверка: список категорий должен быть пустым.
            Assert.Empty(context.Categories);
        }

        [Fact]
        public void Test_AddingExpenseOperation_UpdatesAccountBalance()
        {
            // Подготовка: создаём контекст, счёт и категорию для расходной операции.
            var context = CreateEmptyDataContext();
            var facade = new FinancialFacade(context);
            var account = facade.CreateBankAccount("Operation Test", 1000m);
            var category = facade.CreateCategory(CategoryType.Expense, "Food");

            // Выполнение: добавляем операцию расхода.
            var operation = facade.CreateOperation(OperationType.Expense, account.Id, 200m, DateTime.Now, "Dinner", category.Id);

            // Проверка: должна быть добавлена одна операция, тип операции соответствует расходу, а баланс счёта уменьшился на сумму операции.
            Assert.Single(context.Operations);
            Assert.Equal(OperationType.Expense, operation.Type);
            var updatedAccount = context.BankAccounts.First(a => a.Id == account.Id);
            Assert.Equal(800m, updatedAccount.Balance);
        }

        [Fact]
        public void Test_GetIncomeExpenseDifference_ReturnsAccurateResult()
        {
            // Подготовка: создаём контекст, счёт и необходимые категории.
            var context = CreateEmptyDataContext();
            var facade = new FinancialFacade(context);
            var account = facade.CreateBankAccount("Test", 1000m);
            var incomeCategory = facade.CreateCategory(CategoryType.Income, "Salary");
            var expenseCategory = facade.CreateCategory(CategoryType.Expense, "Food");
            facade.CreateOperation(OperationType.Income, account.Id, 2000m, DateTime.Now, "Salary Payment", incomeCategory.Id);
            facade.CreateOperation(OperationType.Expense, account.Id, 500m, DateTime.Now, "Dinner", expenseCategory.Id);

            // Вычисление разницы между доходами и расходами за заданный период.
            decimal netDifference = facade.GetIncomeExpenseDifference(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
            Assert.Equal(1500m, netDifference);
        }

        [Fact]
        public void Test_GroupingOperationsByCategory_ReturnsCorrectAggregation()
        {
            // Подготовка: создаём контекст, счёт и категории для операций.
            var context = CreateEmptyDataContext();
            var facade = new FinancialFacade(context);
            var account = facade.CreateBankAccount("Test", 1000m);
            var incomeCategory = facade.CreateCategory(CategoryType.Income, "Salary");
            var expenseCategory = facade.CreateCategory(CategoryType.Expense, "Food");
            facade.CreateOperation(OperationType.Income, account.Id, 2000m, DateTime.Now, "Salary Payment", incomeCategory.Id);
            facade.CreateOperation(OperationType.Expense, account.Id, 300m, DateTime.Now, "Lunch", expenseCategory.Id);
            facade.CreateOperation(OperationType.Expense, account.Id, 200m, DateTime.Now, "Snack", expenseCategory.Id);
            var groupedResult = facade.GroupOperationsByCategory();
            // Проверка: в результате должны присутствовать группы для каждой категории с правильными агрегированными суммами.
            Assert.True(groupedResult.ContainsKey("Salary"));
            Assert.True(groupedResult.ContainsKey("Food"));
            Assert.Equal(2000m, groupedResult["Salary"].Income);
            Assert.Equal(500m, groupedResult["Food"].Expense);
        }
    }
}
