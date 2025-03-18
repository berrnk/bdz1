using FinancialAccounting.Domain.Entities;
using FinancialAccounting.Domain.Enums;

namespace FinancialAccounting.Domain.Factories;
public static class DomainFactory
{
    // Статическое поле для хранения последнего использованного идентификатора
    private static long _lastBankAccountId = 0;
    private static long _lastCategoryId = 0;
    private static long _lastOperationId = 0;

    public static BankAccount CreateBankAccount(string name, decimal initialBalance)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название счета не может быть пустым.");
        if (initialBalance < 0)
            throw new ArgumentException("Начальный баланс не может быть отрицательным.");

        // Увеличиваем последний id на 1 для нового счета
        long newId = ++_lastBankAccountId;

        return new BankAccount(newId, name, initialBalance);
    }

    public static Category CreateCategory(CategoryType type, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название категории не может быть пустым.");

        // Увеличиваем последний id на 1 для новой категории
        long newId = ++_lastCategoryId;
        return new Category(newId, type, name);
    }

    public static Operation CreateOperation(OperationType type, long bankAccountId, decimal amount, DateTime date, string description, long categoryId)
    {
        long newId = ++_lastOperationId;
        return new Operation(newId, type, bankAccountId, amount, date, description, categoryId);
    }
}

