using System;
using Accounting.Domain.Entities;

namespace Accounting.Infrastructure.Exporters
{
    public interface IExportVisitor
    {
        void Visit(BankAccount account);
        void Visit(Category category);
        void Visit(Operation operation);
        void SaveToFiles(string accountsFilePath, string categoriesFilePath, string operationsFilePath);
    }
}

