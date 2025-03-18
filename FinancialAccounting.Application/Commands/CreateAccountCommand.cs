using System;
using FinancialAccounting.Application.Facades;

namespace FinancialAccounting.Application.Commands
{
    public class CreateAccountCommand : ICommand
    {
        private readonly FinancialFacade _facade;
        private readonly string _name;
        private readonly decimal _initialBalance;
        public CreateAccountCommand(FinancialFacade facade, string name, decimal initialBalance)
        {
            _facade = facade;
            _name = name;
            _initialBalance = initialBalance;
        }
        public void Execute()
        {
            var account = _facade.CreateBankAccount(_name, _initialBalance);
            Console.WriteLine("Создан счет: " + account);
        }
    }
}

