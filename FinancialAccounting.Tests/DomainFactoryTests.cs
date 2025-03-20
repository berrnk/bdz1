using Accounting.Domain.Enums;
using Accounting.Domain.Factories;
using Xunit;

namespace Accounting.Tests
{
    public class DomainFactoryTests
    {
        [Fact]
        public void TestCreateBankAccountWithValidData_ReturnsValidAccount()
        {
            // Подготовка входных параметров.
            string accountName = "Primary Account";
            decimal startingBalance = 1000m;

            // Вызов фабричного метода для создания банковского счёта.
            var bankAccount = DomainFactory.CreateBankAccount(accountName, startingBalance);

            // Выполнение проверок: объект должен существовать, свойства совпадают с заданными и идентификатор положительный.
            Assert.NotNull(bankAccount);
            Assert.Equal(accountName, bankAccount.Name);
            Assert.Equal(startingBalance, bankAccount.Balance);
            Assert.True(bankAccount.Id > 0, "Идентификатор должен быть больше нуля.");
        }

        [Fact]
        public void TestCreateBankAccountWithEmptyName_ThrowsArgumentException()
        {
            // Проверка: создание счета с пустым именем должно приводить к исключению.
            Assert.Throws<ArgumentException>(() => DomainFactory.CreateBankAccount(string.Empty, 1000m));
        }

        [Fact]
        public void TestCreateBankAccountWithNegativeBalance_ThrowsArgumentException()
        {
            // Проверка: попытка создать счёт с отрицательным балансом вызывает исключение.
            Assert.Throws<ArgumentException>(() => DomainFactory.CreateBankAccount("Account", -100m));
        }

        [Fact]
        public void TestCreateCategoryWithValidData_ReturnsValidCategory()
        {
            // Подготовка: указываем имя категории и её тип.
            string categoryName = "Salary";
            CategoryType categoryType = CategoryType.Income;

            // Вызов метода создания категории.
            var category = DomainFactory.CreateCategory(categoryType, categoryName);

            // Проверка: объект не должен быть null, а его свойства должны совпадать с ожидаемыми.
            Assert.NotNull(category);
            Assert.Equal(categoryName, category.Name);
            Assert.Equal(categoryType, category.Type);
            Assert.True(category.Id > 0, "Идентификатор категории должен быть положительным.");
        }
    }
}
