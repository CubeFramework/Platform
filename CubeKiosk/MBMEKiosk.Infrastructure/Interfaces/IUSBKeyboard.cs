using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MBMEKiosk.Infrastructure.Interfaces
{
    public interface IUSBKeyboard
    {
        void InitAsync(bool simulateDecoder);
        bool RegisterRawInputDevices();
        bool DeRegisterRawInputDevices();
        void ProcessMessage(Message msg);

        bool IsReady();

        string GetDetails(out int state, out int status);

        event Action<byte> KeyPressedEvent;
    }
}