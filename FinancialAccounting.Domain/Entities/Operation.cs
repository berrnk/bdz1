using Accounting.Domain.Enums;

namespace Accounting.Domain.Entities;
public class Operation
{
    public long Id { get; private set; }
    public OperationType Type { get; private set; }
    public long BankAccountId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime Date { get; private set; }
    public string Description { get; private set; }
    public long CategoryId { get; private set; }

    public Operation(long id, OperationType type, long bankAccountId, decimal amount, DateTime date, string description, long categoryId)
    {
        Id = id;
        Type = type;
        BankAccountId = bankAccountId;
        // Валидация – отрицательная сумма недопустима
        if (amount < 0)
            throw new ArgumentException("Сумма операции не может быть отрицательной.");
        Amount = amount;
        Date = date;
        Description = description;
        CategoryId = categoryId;
    }

    public override string ToString()
    {
        return $"Операция: ID: {Id}, {Type}, Сумма: {Amount}, Дата: {Date.ToShortDateString()}";
    }
}

