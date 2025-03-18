using System;
using System.Diagnostics;

namespace FinancialAccounting.Application.Commands
{
    public class TimeMeasuredCommandDecorator : ICommand
    {
        private readonly ICommand _innerCommand;
        public TimeMeasuredCommandDecorator(ICommand command)
        {
            _innerCommand = command;
        }
        public void Execute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            _innerCommand.Execute();
            stopwatch.Stop();
            Console.WriteLine("Время выполнения команды: " + stopwatch.ElapsedMilliseconds + " мс");
        }
    }
}

