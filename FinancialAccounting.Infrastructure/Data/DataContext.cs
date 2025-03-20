using System;
using Accounting.Domain.Entities;

namespace Accounting.Infrastructure.Data
{
    public class DataContext
    {
        public List<BankAccount> BankAccounts { get; private set; } = new List<BankAccount>();
        public List<Category> Categories { get; private set; } = new List<Category>();
        public List<Operation> Operations { get; private set; } = new List<Operation>();

    }
}

