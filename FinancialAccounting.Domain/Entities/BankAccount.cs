namespace FinancialAccounting.Domain.Entities;
public class BankAccount
{
    public long Id { get; private set; }
    public string Name { get; set; }
    public decimal Balance { get; set; }

    public BankAccount(long id, string name, decimal balance)
    {
        Id = id;
        Name = name;
        Balance = balance;
    }

    public override string ToString()
    {
        return $"Id: {Id}, Счет: {Name}, Баланс: {Balance}";
    }
}

