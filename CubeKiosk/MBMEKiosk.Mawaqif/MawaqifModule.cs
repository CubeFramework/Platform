using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.BaseClasses;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.Mawaqif.Presenters;

namespace MBMEKiosk.Mawaqif
{
        public class MawaqifModule : ModuleBase
        {
            public MawaqifModule(IDeviceAgent deviceAgent, string configPath)
                : base(deviceAgent)
            {
                this.ConfigPath = configPath;
            }

            protected override void IntializeTransaction()
            {
                CurrentTransactionContext = new MawaqifTransaction();
            }
        }
     
}
