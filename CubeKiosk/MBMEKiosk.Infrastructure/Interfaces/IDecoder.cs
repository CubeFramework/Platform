using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKiosk.Infrastructure.Interfaces
{
    public interface IDecoder
    {
        void InitAsync(bool simulateDecoder);

        void StartScanningAsync(short id);
        void StopScanningAsync();

        bool IsReady();

        string GetDetails(out int state, out int status);

        event Action ScanEnabledEvent;
        event Action TriggerPulledEvent;
        event Action TriggerReleasedEvent;
        event Action<string> BarcodeScannedEvent;

    }
}
