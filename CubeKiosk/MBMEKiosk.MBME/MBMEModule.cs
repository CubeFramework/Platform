using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.BaseClasses;
using MBMEKiosk.Infrastructure.Utils;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.MBME.Presenters;

namespace MBMEKiosk.MBME
{
    public class MBMEModule : ModuleBase
    {
        public MBMEModule(IDeviceAgent deviceAgent, string configPath)
            : base(deviceAgent)
        {
            this.ConfigPath = configPath;
        }

        protected override void IntializeTransaction()
        {
            CurrentTransactionContext = new MBMETransaction();
        }
    }
}
