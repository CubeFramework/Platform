using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEDevices.CashDevices
{
    public struct ArdacEliteEventCode
    {
        public int code;
        public string desc;
    }
    public class ArdacEliteEventCodes
    {

         static ArdacEliteEventCode[] eventcodes =
            new ArdacEliteEventCode[]{
                //new ArdacEliteEventCode() {code = 0, desc ="Bill or coupon validated correctly and sent to cashbox / stacker"},
                //new ArdacEliteEventCode() {code = 1, desc ="Bill or coupon validated correctly and held in escrow"},
                new ArdacEliteEventCode() {code = 0x00, desc ="Master inhibit active"},
                new ArdacEliteEventCode() {code = 0x01, desc ="Bill returned from escrow"},
                new ArdacEliteEventCode() {code = 0x02, desc ="Invalid bill ( due to validation fail )"},
                new ArdacEliteEventCode() {code = 0x03, desc ="Invalid bill ( due to transport problem )"},
                new ArdacEliteEventCode() {code = 0x04, desc ="Inhibited bill ( on serial )"},
                new ArdacEliteEventCode() {code = 0x05, desc ="Inhibited bill ( on DIP switches )"},
                new ArdacEliteEventCode() {code = 0x06, desc ="Bill jammed in transport ( unsafe mode )"},
                new ArdacEliteEventCode() {code = 0x07, desc ="Bill jammed in stacker"},
                new ArdacEliteEventCode() {code = 0x08, desc ="Bill Pulled Backwards"},
                new ArdacEliteEventCode() {code = 0x09, desc ="Bill tamper"},
                new ArdacEliteEventCode() {code = 0x0A, desc ="Stacker OK"},
                new ArdacEliteEventCode() {code = 0x0B, desc ="Stacker removed"},
                new ArdacEliteEventCode() {code = 0x0C, desc ="Stacker inserted"},
                new ArdacEliteEventCode() {code = 0x0D, desc ="Stacker faulty"},
                new ArdacEliteEventCode() {code = 0x0E, desc ="Stacker full"},
                new ArdacEliteEventCode() {code = 0x0F, desc ="Stacker jammed"},
                new ArdacEliteEventCode() {code = 0x11, desc ="Bill jammed in transport ( safe mode )"},
                new ArdacEliteEventCode() {code = 0x12, desc ="Opto fraud detected"},
                new ArdacEliteEventCode() {code = 0x13, desc ="String fraud detected"},
                new ArdacEliteEventCode() {code = 0x14, desc ="Anti-string mechanism faulty"},
                new ArdacEliteEventCode() {code = 0x15, desc ="Barcode detected"},
                new ArdacEliteEventCode() {code = 0x16, desc ="Unknown Bill type Stacked"},
                new ArdacEliteEventCode() {code = 0x17, desc ="Bill held in Escrow"},
                new ArdacEliteEventCode() {code = 0x18, desc ="Bill stacked"},
                new ArdacEliteEventCode() {code = 0x19, desc ="Power failure"}
        };

         public static string GetEventDesc(short eventcode)
         {
             var q = from str in eventcodes
                     where str.code == eventcode
                     select str.desc.ToString();
             return q.ToString();
         }
    }

    
}
