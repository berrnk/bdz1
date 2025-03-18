using System;
using FinancialAccounting.Domain.Entities;

namespace FinancialAccounting.Infrastructure.Data
{
    public class DataContext
    {
        public List<BankAccount> BankAccounts { get; private set; } = new List<BankAccount>();
        public List<Category> Categories { get; private set; } = new List<Category>();
        public List<Operation> Operations { get; private set; } = new List<Operation>();

        // Здесь можно добавить методы синхронизации с реальной базой данных
    }
}

