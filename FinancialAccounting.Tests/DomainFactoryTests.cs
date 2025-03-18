using FinancialAccounting.Domain.Enums;
using FinancialAccounting.Domain.Factories;
using Xunit;

namespace FinancialAccounting.Tests;

public class DomainFactoryTests
{
    [Fact]
    public void CreateBankAccount_ValidInput_ShouldReturnBankAccount()
    {
        // Arrange
        string name = "Main Account";
        decimal balance = 1000m;

        // Act
        var account = DomainFactory.CreateBankAccount(name, balance);

        // Assert
        Assert.NotNull(account);
        Assert.Equal(name, account.Name);
        Assert.Equal(balance, account.Balance);
        Assert.True(account.Id > 0); // Проверка, что id больше 0 (при последовательном генерации)
    }

    [Fact]
    public void CreateBankAccount_InvalidName_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => DomainFactory.CreateBankAccount("", 1000m));
    }

    [Fact]
    public void CreateBankAccount_NegativeBalance_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => DomainFactory.CreateBankAccount("Account", -100m));
    }

    [Fact]
    public void CreateCategory_ValidInput_ShouldReturnCategory()
    {
        string name = "Salary";
        var category = DomainFactory.CreateCategory(CategoryType.Income, name);
        Assert.NotNull(category);
        Assert.Equal(name, category.Name);
        Assert.Equal(CategoryType.Income, category.Type);
        Assert.True(category.Id > 0);
    }
}
