using System;
using Accounting.Domain.Entities;
using Accounting.Domain.Enums;
using Accounting.Domain.Factories;
using Accounting.Infrastructure.Data;
using Accounting.Infrastructure.Exporters;

namespace Accounting.Application.Facades
{
    public class FinancialFacade
    {
        private readonly DataContext _dataContext;
        public FinancialFacade(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        // Методы для работы со счетами.
        public BankAccount CreateBankAccount(string name, decimal initialBalance)
        {
            var account = DomainFactory.CreateBankAccount(name, initialBalance);
            _dataContext.BankAccounts.Add(account);
            return account;
        }

        public void EditBankAccount(long accountId, string newName, decimal newBalance)
        {
            var account = _dataContext.BankAccounts.FirstOrDefault(a => a.Id == accountId);
            if (account == null)
                throw new Exception("Счет не найден.");
            account.Name = newName;
            account.Balance = newBalance;
        }

        public void DeleteBankAccount(long accountId)
        {
            var account = _dataContext.BankAccounts.FirstOrDefault(a => a.Id == accountId);
            if (account != null)
                _dataContext.BankAccounts.Remove(account);
        }

        public IEnumerable<BankAccount> GetBankAccounts() => _dataContext.BankAccounts;

        // Методы для работы с категориями
        public Category CreateCategory(CategoryType type, string name)
        {
            var category = DomainFactory.CreateCategory(type, name);
            _dataContext.Categories.Add(category);
            return category;
        }

        public void EditCategory(long categoryId, string newName)
        {
            var category = _dataContext.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (category == null)
                throw new Exception("Категория не найдена.");
            category.Name = newName;
        }

        public void DeleteCategory(long categoryId)
        {
            var category = _dataContext.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (category != null)
                _dataContext.Categories.Remove(category);
        }

        public IEnumerable<Category> GetCategories() => _dataContext.Categories;

        // Методы для работы с операциями
        public Operation CreateOperation(OperationType type, long bankAccountId, decimal amount, DateTime date, string description, long categoryId)
        {
            var operation = DomainFactory.CreateOperation(type, bankAccountId, amount, date, description, categoryId);
            _dataContext.Operations.Add(operation);

            // Автоматическое обновление баланса счета
            var account = _dataContext.BankAccounts.FirstOrDefault(a => a.Id == bankAccountId);
            if (account != null)
            {
                account.Balance += (type == OperationType.Income ? amount : -amount);
            }

            return operation;
        }

        public void EditOperation(long operationId, decimal newAmount, DateTime newDate, string newDescription)
        {
            var operation = _dataContext.Operations.FirstOrDefault(o => o.Id == operationId);
            if (operation == null)
                throw new Exception("Операция не найдена.");
            // В простом примере редактирование не приводит к пересчету баланса.
            // При необходимости можно добавить логику пересчета.
        }

        public void DeleteOperation(long operationId)
        {
            var operation = _dataContext.Operations.FirstOrDefault(o => o.Id == operationId);
            if (operation != null)
            {
                _dataContext.Operations.Remove(operation);
                // Пересчет баланса после удаления операции
                var account = _dataContext.BankAccounts.FirstOrDefault(a => a.Id == operation.BankAccountId);
                if (account != null)
                {
                    account.Balance -= (operation.Type == OperationType.Income ? operation.Amount : -operation.Amount);
                }
            }
        }
        public IEnumerable<Operation> GetOperations() => _dataContext.Operations;

        // Аналитика: разница доходов и расходов за выбранный период
        public decimal GetIncomeExpenseDifference(DateTime startDate, DateTime endDate)
        {
            var income = _dataContext.Operations
                .Where(o => o.Date >= startDate && o.Date <= endDate && o.Type == OperationType.Income)
                .Sum(o => o.Amount);
            var expense = _dataContext.Operations
                .Where(o => o.Date >= startDate && o.Date <= endDate && o.Type == OperationType.Expense)
                .Sum(o => o.Amount);
            return income - expense;
        }

        // Группировка операций по категориям.
        public Dictionary<string, (decimal Income, decimal Expense)> GroupOperationsByCategory()
        {
            var result = new Dictionary<string, (decimal Income, decimal Expense)>();

            foreach (var category in _dataContext.Categories)
            {
                decimal totalIncome = _dataContext.Operations
                    .Where(op => op.CategoryId == category.Id && op.Type == OperationType.Income)
                    .Sum(op => op.Amount);

                decimal totalExpense = _dataContext.Operations
                    .Where(op => op.CategoryId == category.Id && op.Type == OperationType.Expense)
                    .Sum(op => op.Amount);

                result[category.Name] = (totalIncome, totalExpense);
            }

            return result;
        }


        // Экспорт данных с использованием паттерна Посетитель.
        public void ExportData(IExportVisitor visitor)
        {
            foreach (var account in _dataContext.BankAccounts)
                visitor.Visit(account);
            foreach (var category in _dataContext.Categories)
                visitor.Visit(category);
            foreach (var operation in _dataContext.Operations)
                visitor.Visit(operation);
        }
    }
}

