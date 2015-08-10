using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKiosk.Infrastructure.Interfaces
{
    public interface IPackDispenser
    {
        void InitAsync(bool simulatePackDispenser);

        // Reset Method
        // Resets the dispenser controller to it's initial state.
        //bool Reset();

        // Vend Method
        // Dispense n products.
        bool VendAsync(short n);
        
        bool IsReady();

        string GetDetails(out int state, out int status);
        event Action<short> InitiateVendEvent;
        event Action<bool> CardVendStatusEvent;
        event Action VendCompletionStatusEvent;
    }
}
