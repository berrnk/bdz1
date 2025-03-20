using Accounting.Domain.Entities;
using Accounting.Domain.Enums;

namespace Accounting.Domain.Factories
{
    public static class DomainFactory
    {
        // Хранение последних присвоенных идентификаторов для сущностей.
        private static long lastBankAccountId = 0;
        private static long lastCategoryId = 0;
        private static long lastOperationId = 0;

        public static BankAccount CreateBankAccount(string name, decimal initialBalance)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя счёта не должно быть пустым.");
            if (initialBalance < 0)
                throw new ArgumentException("Начальный баланс должен быть неотрицательным.");

            // Инкремент идентификатора и создание нового банковского счёта.
            long newId = ++lastBankAccountId;
            return new BankAccount(newId, name, initialBalance);
        }

        public static Category CreateCategory(CategoryType type, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя категории не должно быть пустым.");

            // Генерация нового идентификатора для категории.
            long newId = ++lastCategoryId;
            return new Category(newId, type, name);
        }

        public static Operation CreateOperation(OperationType type, long bankAccountId, decimal amount, DateTime date, string description, long categoryId)
        {
            // Генерация идентификатора для новой операции.
            long newId = ++lastOperationId;
            return new Operation(newId, type, bankAccountId, amount, date, description, categoryId);
        }
    }
}
