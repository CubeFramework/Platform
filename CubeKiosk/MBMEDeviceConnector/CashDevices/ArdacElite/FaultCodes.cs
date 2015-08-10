using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEDevices.CashDevices
{
    public struct ArdacEliteFaultCode
    {
        public int code;
        public string desc;
    }
    public class ArdacEliteFaultCodes
    {
        static ArdacEliteFaultCode[] faultcodes =
            new ArdacEliteFaultCode[]{
                new ArdacEliteFaultCode() {code = 1, desc ="EEPROM Checksum corrupted"},
                new ArdacEliteFaultCode() {code = 30, desc ="ROM Checksum mismatch"},
                new ArdacEliteFaultCode() {code = 36, desc ="Fault on bill validation sensor"},
                new ArdacEliteFaultCode() {code = 37, desc ="Fault on bill transport motor"},
                new ArdacEliteFaultCode() {code = 38, desc ="Fault on stacker"},
                new ArdacEliteFaultCode() {code = 39, desc ="Bill jammed"},
                new ArdacEliteFaultCode() {code = 40, desc ="RAM test fail"},
                new ArdacEliteFaultCode() {code = 41, desc ="Fault on string sensor"},
                new ArdacEliteFaultCode() {code = 44, desc ="Stacker missing"},
                new ArdacEliteFaultCode() {code = 45, desc ="Stacker Full"},
        };

        public static string GetFaultDesc(short faultcode)
        {
            string strResult = null;
            var q = from str in faultcodes
                    where str.code == faultcode
                    select str.desc.ToString();
            //return q.WhereA<

            foreach (string item in q)
            {
                strResult = item.ToString();
                break;
            }

            return strResult;
        }

         
    }
}

