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
using MBMEKiosk.Infrastructure.Proxies.ProxyCardService;


namespace MBMEKiosk.Mawaqif.Presenters
{
    internal delegate void DGetFeeProcessed(string trigger);

    public class MwqProcessingGetFeePresenter : ProcessingGetFeePresenter
    {
        public MawaqifTransaction Transaction
        {
            get
            {
                return this.TransactionContext as MawaqifTransaction;
            }
        }

        protected override void GetFee()
        {
            string outputTrigger = ERRORACTION;
            
            try
            {
                if (Boolean.Parse(ConfigurationManager.AppSettings["StandAloneMode"]))
                {
                    Thread.Sleep(1000);
                    this.TransactionContext.AppliedFee = 2.35;
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
                }
                else
                {
                    CardFeeRequest request = new CardFeeRequest
                    {
                        KioskTxnrefnum = this.TransactionContext.Id,
                        ServiceKey = this.TransactionContext.DispatcherAction
                    };

                    CardFeeResponse response = new CardFeeResponse
                    {
                        Success = false
                    };

                    using (CardServiceClient client = new CardServiceClient())
                    {
                        response = client.GetCardFee(request);
                    }

                    if (response.Success)
                    {
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
                        this.TransactionContext.AppliedFee = response.FeePercentage;
                    }
                    else
                        outputTrigger = ERRORACTION;
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Mawaqif,caught exception GetAppliedFee try  block for TransactionContextid:" + this.TransactionContext.Id + "." + ex.Message);
            }
            finally
            {
                ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DGetFeeProcessed(ChangeState), outputTrigger);
            }

        }
    }
}
