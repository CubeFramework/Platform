using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKiosk.Infrastructure.Interfaces
{
    public interface ICashAcceptor
    {
        void InitAsync(bool simulateCashAcceptor);

        void EnableAsync(List<int> denominations, double amount, double minAmount, double maxAmount, bool isAmountFixed);

        void DisableAsync();

        event Action<string> CashCycleInitiatedEvent;

        event Action<int, bool> CashInsertedEvent;

        event Action<int, string> NoteStackedEvent;

        event Action<int> NoteReturnedEvent;

        ////event Action<int> CashCycleCompletedEvent;

        ////event Action<int> CashCycleCompletedEvent;
        bool IssueStack();
        //bool IssueHold();
        bool IssueReturn();

        bool IsEnabled();

        bool IsReady();

        string GetDetails(out int state,out int status);

        double GetAmountStackedInLastCycle();
    }
}
