using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEDevices.PackDispenser
{
        //enum DispenserStatus
        //{
        //    OK,
        //    SOLD_OUT,
        //    DOWN,
        //    ERROR
        //}

        public enum DispenserStatus
        {
            NoError = 1, // Dispenser Status OK = 0
            Warning = 2, // Stock Low 
            Error = 3, // Dispenser Status 1= Sold Out, 2 = Unable To Dispense/ Down
            Initializing = 4 // Initialization Sequence Happening
        }

        public enum ColumnStatus
        {
            DOES_NOT_EXIST=-1,
            OK=0,
            SOLD_OUT=1,
            DOWN=2
        }

        public enum DispenserCommState
        {
            ONLINE = 1,
            OFFLINE = 2
        }
    //}
}
