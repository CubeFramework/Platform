using System;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using MBMEKiosk.Infrastructure.Events;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.Infrastructure.Proxies.ProxyCardService;
using MBMEKiosk.ObjectModel;
using MBMEKiosk.Infrastructure.ObjectModel;

namespace MBMEKiosk.Infrastructure.BaseClasses
{
    internal delegate void DGetFee();
    internal delegate void DGetFeeProcessed(string trigger);

    public class ProcessingGetFeePresenter : PresenterBase
    {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected string outputTrigger = ERRORACTION;

        public override FrameworkElement LoadXaml(IDeviceAgent devices, KioskState state, TransactionContextBase transactionContext)
        {
            FrameworkElement viewGrid = base.LoadXaml(devices, state, transactionContext);
            DGetFee getFee = new DGetFee(this.GetFee);
            getFee.BeginInvoke(null, null);
            return viewGrid;
        }

        protected virtual void GetFee()
        {
            outputTrigger = ERRORACTION;
            // Reset respective TransactionContext context fields.

            try
            {
                if (Boolean.Parse(ConfigurationManager.AppSettings["StandAloneMode"]))
                {
                    Thread.Sleep(1000);
                    this.TransactionContext.AppliedFee = 2.35;
                    outputTrigger = "submit";
                }
                else
                {
                    CardFeeRequest request = new CardFeeRequest
                    {
                         KioskTxnrefnum = this.TransactionContext.Id,
                         ServiceKey = this.TransactionContext.DispatcherAction
                    };

                    CardFeeResponse response = new CardFeeResponse{
                         Success = false
                    };

                    using (CardServiceClient client = new CardServiceClient())
                    {
                        response = client.GetCardFee(request);
                    }

                    if (response.Success)
                    {
                        outputTrigger = SUBMITACTION;
                        this.TransactionContext.AppliedFee = response.FeePercentage;
                    }
                    else
                        outputTrigger = ERRORACTION;
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("DU,caught exception GetAppliedFee try  block for TransactionContextid:" + this.TransactionContext.Id + "." + ex.Message);
            }
            finally
            {
                ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DGetFeeProcessed(ChangeState), outputTrigger);
            }

        }

        #region private 
        
        protected void ChangeState(string trigger)
        {
            OnKioskStateChanged(new KioskStateChangedEventArgs(trigger));
        }

        #endregion
    }

}
