using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEDevices.CardReader
{
    public struct ArtemaModularFaultCode
    {
        public int code;
        public string desc;
    }
    
    public class ArtemaModularFaultCodes
    {
        static ArtemaModularFaultCode[] faultcodes =
            new ArtemaModularFaultCode[]{
                new ArtemaModularFaultCode() {code = (int)ErrorCode.OK, desc ="Operation Successful."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_NOT_SUPPORTED, desc ="Operation Not Supported."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_CONNECT_FAILED, desc ="Connection Attempt Failed."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_LOGIN_FAILED, desc ="Login Failed."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_LOST_CONNECTION, desc ="Connection Lost."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_PARAMETER, desc ="Parameter Error."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_TIMEOUT, desc ="Operation Timed out."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_BUFFER_TOO_SMALL, desc ="Buffer Too Small."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_PINPAD, desc ="PINPAD Error."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_CARD_READER, desc ="CardReader Error."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_PRINTER, desc ="Printer Error."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_NO_CARD, desc ="No Card Error."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_CARD_READER, desc ="CardReader Error."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_CARD_NOT_READABLE, desc ="Card Not Readable."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_CARD_INVALID, desc ="Card Invalid."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_CARD_EXPIRED, desc ="Card Expired."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_CARD_LIMIT_EXCEEDED, desc ="Card Limit Exceeded."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_RETAILER_CARD, desc ="Retailer Card."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_WRONG_PIN, desc ="Wrong PIN."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_PIN_DISABLED, desc ="PIN Disabled."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_BREAK, desc ="Operation Cancelled."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_NO_CONFIRM, desc ="No Confirmation."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_BUSY, desc ="Busy."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_PROTOCOL, desc ="Protocol Error."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_AMOUNT, desc ="Invalid Amount."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_CURRENCY, desc ="Invalid Currency."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_REGISTRATION, desc ="Registration Error."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_TERMINAL_MEMORY, desc ="Terminal Memory Error."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_WRONG_PASSWD, desc ="Invalid Password."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_NOT_ALLOWED, desc ="Operation not allowed."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_NOT_IN_SERVICE, desc ="Device not in service."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_COMMUNICATION, desc ="Communication Error."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_CARD_NOT_REMOVED, desc ="Card not removed."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_UNKNOWN, desc ="Unknown Error."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_OP_NOT_POSSIBLE, desc ="Operation not possible."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_DEVICE_NOT_SUPPORTED, desc ="Device not supported."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_TIPTRANS_CONFLICT, desc ="TIP transaction conflict."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_CONNECTION_INTERRUPTED, desc ="Connection Interrupted."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_REQUEST_FAILED, desc ="Request Failed."},
                new ArtemaModularFaultCode() {code = (int)ErrorCode.ERR_PRIVATE_ERROR_START, desc ="Private Error Start."},

        };

        public static string GetFaultDesc(int faultcode)
        {
            var q = from str in faultcodes
                    where str.code == faultcode
                    select str.desc.ToString();
            return q.ToString();
        }
    }
}
