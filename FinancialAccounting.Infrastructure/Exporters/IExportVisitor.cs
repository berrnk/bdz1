using System;
using FinancialAccounting.Domain.Entities;

namespace FinancialAccounting.Infrastructure.Exporters
{
    public interface IExportVisitor
    {
        void Visit(BankAccount account);
        void Visit(Category category);
        void Visit(Operation operation);
        void SaveToFiles(string accountsFilePath, string categoriesFilePath, string operationsFilePath);
    }
}

