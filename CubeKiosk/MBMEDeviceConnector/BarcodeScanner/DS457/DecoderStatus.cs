using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEDevices.BarcodeScanner
{
    public enum DecoderStatus
    {
        NoError = 1,
        Warning = 2,
        Error = 3,
        Initializing = 4
    }

    public enum DecoderCommands
    {
        SCAN_ENABLE,
        SCAN_DISABLE,
        START_SESSION,
        STOP_SESSION,
        WAKEUP,
        SLEEP,
        CMD_ACK,
        CMD_NAK,
        PARAM_SEND,
        PARAM_REQUEST,
        REQUEST_REVISION,
        REPLY_REVISION,
        DECODE_DATA
    }
}
