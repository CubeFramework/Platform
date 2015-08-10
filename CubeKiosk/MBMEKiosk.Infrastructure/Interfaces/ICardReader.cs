using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKiosk.Infrastructure.Interfaces
{
    public interface ICardReader
    {
        void InitAsync(bool simulateCashAcceptor);

        bool PaymentAsync(double expectedAmount);

        //bool SettlementAsync(bool simulateCashAcceptor);

        bool IsReady();

        string GetDetails(out int state, out int status);

        //Used for CardReader
        event Action<string> SwipeCardEvent;
        event Action<double> PaymentAuthorizedEvent;
        event Action<double> PaymentConfirmationEvent;
        event Action<double> PaymentConfirmedEvent;
        event Action<double> PaymentFailedEvent;

        /// <summary>
        /// Event to publish the action of Unknown/not Correctly inserted Card.
        /// </summary>
        ////public event Action<double> CardInValid;
        event Action<short> CardInValid;
        
        event Action<string, string, string, string, string, string, string, string, string, string> ReceiptEvent;
    }
}
