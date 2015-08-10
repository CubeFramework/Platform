using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.BaseClasses;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using MBMEKiosk.Infrastructure.Events;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.ObjectModel;
using MBMEKiosk.Infrastructure.ObjectModel;
using MawaqifBackendProxy;
using System.Configuration;
using MBMEKiosk.Infrastructure.Utils;
using System.Diagnostics;
using log4net;


namespace MBMEKiosk.Mawaqif.Presenters
{
    internal delegate void DGetFee();
    internal delegate void DGetFeeProcessed(string trigger);

    public class ProcessingGetFeePresenter : MawaqifPresenterBase
    {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override FrameworkElement LoadXaml(IDeviceAgent devices, KioskState state, TransactionContextBase transactionContext)
        {
            FrameworkElement viewGrid = base.LoadXaml(devices, state, transactionContext);
            DGetFee getFee = new DGetFee(this.GetFee);
            getFee.BeginInvoke(null, null);
            return viewGrid;
        }

        private void GetFee()
        {
            string outputTrigger = "error";

            // Reset respective transaction context fields.

            try
            {
                if (Boolean.Parse(ConfigurationManager.AppSettings["StandAloneMode"]))
                {
                    Thread.Sleep(1000);
                    this.Transaction.AppliedFee = 2.35;
                    
                    
                }
                else
                {

                    using (MBMEMawaqifServiceClient client = new MBMEMawaqifServiceClient())
                    {
                        this.Transaction.AppliedFee = client.GetAppliedFee(Convert.ToInt32(this.Transaction.MachineId));
                    }
                    
                  
                }

                switch (this.Transaction.ServiceType)
                {
                    case MawaqifServiceType.AccountTopUp:
                        outputTrigger = "topupsubmit";
                        break;
                    case MawaqifServiceType.PermitRenewal:
                        outputTrigger = "rpsubmit";
                        break;
                    case MawaqifServiceType.ViolationPayment:
                        outputTrigger = "pvtsubmit";
                        break;
                }

                if (this.Transaction.AppliedFee == 0)
                {
                    outputTrigger = ERRORACTION;
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("DU,caught exception GetAppliedFee try  block for Transactionid:" + this.Transaction.Id + "." + ex.Message);
            }
            finally
            {
                ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DGetBalanceProcessed(ChangeState), outputTrigger);
            }

        }
        
        protected void ChangeState(string trigger)
        {
            OnKioskStateChanged(new KioskStateChangedEventArgs(trigger));
        }

       
    }
}
