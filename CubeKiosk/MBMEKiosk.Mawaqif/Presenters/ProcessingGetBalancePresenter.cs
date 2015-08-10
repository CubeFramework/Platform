using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.ObjectModel;
using MBMEKiosk.Infrastructure.ObjectModel;
using System.Threading;
using MBMEKiosk.Infrastructure.Utils;
using System.Windows.Threading;
using MBMEKiosk.Infrastructure.Events;
using System.Configuration;
using MawaqifBackendProxy;
using System.Diagnostics;
using System.Globalization;
using log4net;
namespace MBMEKiosk.Mawaqif.Presenters
{
    internal delegate void DGetBalance();
    internal delegate void DGetBalanceProcessed(string trigger);
    

    public class ProcessingGetBalancePresenter : MawaqifPresenterBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override FrameworkElement LoadXaml(IDeviceAgent devices, KioskState state, TransactionContextBase transactionContext)
        {
            FrameworkElement viewGrid = base.LoadXaml(devices, state, transactionContext);
            DGetBalance getBalance = new DGetBalance(this.GetBalance);
            getBalance.BeginInvoke(null, null);
            return viewGrid;
        }

        private void GetBalance()
        {
            string outputTrigger = ERRORACTION;
            
            
            // Reset respective transaction context fields.
            this.Transaction.Id = DateTime.Now.ToString("yyMMddHHmmssff") + this.Transaction.MachineId;
            this.Transaction.BalanceDue = "0.00";
            //this.Transaction.BalanceAfterTax = "0.00";
            this.Transaction.AmountPaid = "0.00";
            this.Transaction.AmountDue = "0.00";

            try
            {
                // KS TODO: Remove the below hardcoded value and get the same from app settings using the commented code below.
                this.Transaction.BalanceDue = "0.00";
                this.Transaction.AmountPaid = "0.00";

                if (Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["StandAloneMode"]))
                {
                    this.Transaction.BalanceDue = "25.00";
                    this.Transaction.ServiceCharges = "2.00";
                    this.Transaction.AmountPaid = "0.00";
                    //this.Transaction.AmountDue = this.Transaction.BalanceAfterTax;
                    outputTrigger = SUBMITACTION;
                    Thread.Sleep(3000);
                }
                else
                {
                    switch (this.Transaction.ServiceType)
                    {
                        case MawaqifServiceType.AccountTopUp:
                            if (Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["StandAloneMode"]))
                            {
                                this.Transaction.BalanceDue = "200";
                                outputTrigger = SUBMITACTION;
                            }
                            else
                            {
                                TopupRequest(ref outputTrigger);
                            }
                            break;
                        case MawaqifServiceType.PermitRenewal:
                            if (Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["StandAloneMode"]))
                            {
                                this.Transaction.IssueDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                                this.Transaction.ExpiryDate = DateTime.Now.AddDays(365).ToString("dd/MM/yyyy HH:mm:ss");
                                this.Transaction.BalanceDue = "400";
                                this.Transaction.AmountDue = "400";
                                outputTrigger = SUBMITACTION;
                            }
                            else
                            {
                                rpRequest(ref outputTrigger);
                            }
                            break;
                        case MawaqifServiceType.ViolationPayment:
                            if (Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["StandAloneMode"]))
                            {
                                this.Transaction.IssueDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                                this.Transaction.PlateNumber = "347";
                                this.Transaction.BalanceDue = "300.00";
                                this.Transaction.AmountDue = "300";
                                outputTrigger = SUBMITACTION;
                            }
                            else
                            {
                                vpayRequest(ref outputTrigger);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                outputTrigger = ERRORACTION;
                if (log.IsErrorEnabled) log.Error("salik,caught general exception in post payment main try block for " +e.Message); 
                //Trace.WriteLine(e.Message.ToString());
                // KS TODO : Logging.
            }
            finally
            {
                ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DGetBalanceProcessed(ChangeState), outputTrigger);
            }
        }

        #region WebService Request Method

        private void TopupRequest(ref string outputTrigger)
        {
            outputTrigger = ERRORACTION;
            TopUpBalanceRequest request = new TopUpBalanceRequest
            {
                
                AccountNumber = this.TransactionContext.AccountNumber,
                TransactionTime = Transaction.Date,
                TransactionId = this.Transaction.Id,
                MachineId = long.Parse(this.Transaction.MachineId),
                //StimulateResponse = this.Transaction.StimulateBackend,
                PIN = "000",
                ServiceType = MawaqifBackendProxy.MawaqifServiceType.AccountTopUp,
                Language = (Transaction.SelectedLanguageKey == "english") ? MawaqifBackendProxy.KioskLanguage.English : MawaqifBackendProxy.KioskLanguage.Arabic,
            };

            TopUpBalanceResponse response = new TopUpBalanceResponse
            {
                BalanceAmount = "0.00",
                ResponseCode = Convert.ToInt16(MBMEServiceResponse.Failed).ToString()
            };


            ValidateCertificate.RegisterCallback();
            using (MBMEMawaqifServiceClient client = new MBMEMawaqifServiceClient())
            {
                try
                {
                    response = client.TopUpGetBalance(request);
                }
                catch (TimeoutException ex)
                {
                    outputTrigger = TIMEOUTACTION;
                    if (log.IsInfoEnabled) log.Info("caught timeout exception in mawaqif." + ex.Message); 
                }
                catch (Exception ex)
                {
                    outputTrigger = ERRORACTION;
                    //Trace.WriteLine(ex.Message.ToString());
                    if (log.IsInfoEnabled) log.Info("caught exception in mawaqif."+ex.Message); 
                    // KS TODO: Logging.
                }
                finally
                {
                    this.Transaction.TxnId = response.TxnId;
                    this.Transaction.ReceiptNumber = response.TxnId.ToString();
                    ValidateCertificate.DeregisterCallback();
                }
            }
            
            //KS TODO : Logging.
            if (response.ResponseCode == Convert.ToInt16(MBMEServiceResponse.Success).ToString())
            {
                //this.Transaction.AccountNumber = response.AccountNumber;
                this.Transaction.BalanceDue = string.Format("{0:0.00}", (Math.Ceiling(Convert.ToDecimal(response.BalanceAmount))).ToString());
                this.Transaction.AmountDue = string.Format("{0:0.00}", (Math.Ceiling(Convert.ToDecimal(response.BalanceAmount))).ToString());                 
                //this.Transaction.AmountPaid = response.BalanceAmount;
                outputTrigger = SUBMITACTION;
            }
            
            else
            {
                this.Transaction.AccountNumber = string.Empty;
                if (this.Transaction.InvalidCount < this.State.RetryCount)
                {
                    if (response.ResponseCode == Convert.ToInt16(MBMEServiceResponse.BillerError).ToString())
                    {
                        Transaction.InvalidCount++;
                        Transaction.Message = response.ResponseMessage;
                        outputTrigger = BILLERERRORACTION;
                    }

                }
                else              
                    outputTrigger = ERRORACTION;
             
            }
        }

        private void vpayRequest(ref string outputTrigger)
        {
            outputTrigger = ERRORACTION;
             
            mwqBillerServiceProxy.GetPVTRequest request = new mwqBillerServiceProxy.GetPVTRequest
            {
                //TransactionTime = Transaction.Date,
                ConsumerNumber = this.TransactionContext.AccountNumber,
                TransId = Convert.ToInt64(this.Transaction.Id),
                KioskId = this.Transaction.MachineId,
                
                //StimulateResponse = this.Transaction.StimulateBackend,
                //Locale = this.Transaction.LocaleId,
                //ServiceType = MawaqifBackendProxy.MawaqifServiceType.ViolationPayment,
                //Language = (Transaction.SelectedLanguageKey == "english") ? MawaqifBackendProxy.KioskLanguage.English : MawaqifBackendProxy.KioskLanguage.Arabic,
            };

            mwqBillerServiceProxy.GetPVTResponse response = new mwqBillerServiceProxy.GetPVTResponse
            {
                ResponseCode = -1
            };

            //ValidateCertificate.RegisterCallback();
            using (mwqBillerServiceProxy.BillerServiceClient client = new mwqBillerServiceProxy.BillerServiceClient())
            {
                try
                {
                    response = client.GetPVT(request);
                }
                catch (Exception ex)
                {
                    outputTrigger = ERRORACTION;
                    //Trace.WriteLine(ex.Message);
                    if (log.IsInfoEnabled) log.Info("caught exception in PVTGetBalance." + ex.Message);
                }
                finally
                {
                    //this.Transaction.TxnId = response.TxnId;
                    this.Transaction.ReceiptNumber = this.Transaction.Id; //response.TxnId.ToString();
                    //ValidateCertificate.DeregisterCallback();
                }
            }


            //KS TODO : Logging.
            //if (response.ResponseCode == Convert.ToInt16(MBMEServiceResponse.Success).ToString())
            if (response.ResponseCode == 0)
            {
                //this.Transaction.AccountNumber = response.AccountNumber;
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
                this.Transaction.IssueDate = response.issueDate.ToString((this.Transaction.LocaleId == "en") ?
                    "dd/MM/yyyy HH:mm:ss" : "HH:mm:ss yyyy/MM/dd");
                //this.Transaction.IssueDate = response.IssueDate.ToString("dd/MM/yyyy HH:mm:ss");
                this.Transaction.PlateNumber = response.PlateNumber;
                this.Transaction.BalanceDue = Convert.ToDouble(response.amount).ToString("0.00");
                this.Transaction.AmountDue = Convert.ToDouble(response.amount).ToString("0.00");
                this.Transaction.Country = response.country;
                this.Transaction.Category = response.category;
                this.Transaction.PVTType = response.type;
                this.Transaction.StageMessage = response.response_message;
                outputTrigger = SUBMITACTION;
            }
            else
            {
                this.Transaction.AccountNumber = string.Empty;
                if (this.Transaction.InvalidCount < this.State.RetryCount)
                {
                    if (response.ResponseCode > 0)
                    {
                        Transaction.InvalidCount++;
                        Transaction.Message = response.response_message;
                        outputTrigger = BILLERERRORACTION;
                    }
                    else
                    {
                        outputTrigger = ERRORACTION;
                    }
                }
                else
                    outputTrigger = ERRORACTION;
            }
        }

        //private void vpayRequest(ref string outputTrigger)
        //{
        //    outputTrigger = ERRORACTION;
        //    PVTBalanceRequest request = new PVTBalanceRequest
        //    {
        //        TransactionTime = Transaction.Date,
        //        AccountNumber = this.TransactionContext.AccountNumber,
        //        TransactionId = this.Transaction.Id,
        //        MachineId = long.Parse(this.Transaction.MachineId),
        //        StimulateResponse = this.Transaction.StimulateBackend,
        //        Locale = this.Transaction.LocaleId,
        //        ServiceType= MawaqifBackendProxy.MawaqifServiceType.ViolationPayment,
        //        Language = (Transaction.SelectedLanguageKey == "english") ? MawaqifBackendProxy.KioskLanguage.English : MawaqifBackendProxy.KioskLanguage.Arabic,
        //    };

        //    PVTBalanceResponse response = new PVTBalanceResponse
        //    {
        //        ResponseCode = Convert.ToInt16(MBMEServiceResponse.Failed).ToString(),
        //    };
            
        //    ValidateCertificate.RegisterCallback();

           
        //    using (MBMEMawaqifServiceClient client = new MBMEMawaqifServiceClient())
        //    {
        //        try
        //        {
        //            response = client.PVTGetBalance(request);
        //        }
        //        catch (Exception ex)
        //        {
        //            outputTrigger = ERRORACTION;
        //            //Trace.WriteLine(ex.Message);
        //            if (log.IsInfoEnabled) log.Info("caught exception in PVTGetBalance." + ex.Message); 
        //        }
        //        finally
        //        {
        //            this.Transaction.TxnId = response.TxnId;
        //            this.Transaction.ReceiptNumber = response.TxnId.ToString();
        //            ValidateCertificate.DeregisterCallback();
        //        }
        //    }


        //    //KS TODO : Logging.
        //    if (response.ResponseCode == Convert.ToInt16(MBMEServiceResponse.Success).ToString())
        //    {
        //        this.Transaction.AccountNumber = response.AccountNumber;
        //        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
        //        this.Transaction.IssueDate = response.IssueDate.ToString((this.Transaction.LocaleId == "en") ?
        //            "dd/MM/yyyy HH:mm:ss" : "HH:mm:ss yyyy/MM/dd");
        //        //this.Transaction.IssueDate = response.IssueDate.ToString("dd/MM/yyyy HH:mm:ss");
        //        this.Transaction.PlateNumber = response.PlateNumber;
        //        this.Transaction.BalanceDue = Convert.ToDouble(response.Amount).ToString("0.00");
        //        this.Transaction.AmountDue = Convert.ToDouble(response.Amount).ToString("0.00");
        //        this.Transaction.Country = response.Country;
        //        this.Transaction.Category = response.Category;
        //        this.Transaction.PVTType = response.Type; 
        //        this.Transaction.StageMessage = response.ResponseMessage;
        //        outputTrigger = SUBMITACTION;
        //    }
        //    else
        //    {
        //        this.Transaction.AccountNumber = string.Empty;
        //        if (this.Transaction.InvalidCount < this.State.RetryCount)
        //        {
        //            if (response.ResponseCode == Convert.ToInt16(MBMEServiceResponse.BillerError).ToString())
        //            {
        //                Transaction.InvalidCount++;
        //                Transaction.Message = response.ResponseMessage;
        //                outputTrigger = BILLERERRORACTION;
        //            }
        //            else
        //            {
        //                outputTrigger = ERRORACTION;
        //            }
        //        }
        //        else
        //            outputTrigger = ERRORACTION;
        //    }
        //}

        private void rpRequest(ref string outputTrigger)
        {
            outputTrigger = ERRORACTION;
            RPBalanceRequest request = new RPBalanceRequest
            {
                TransactionTime = Transaction.Date,
                AccountNumber = this.TransactionContext.AccountNumber,
                TransactionId = this.Transaction.Id,
                StimulateResponse = this.Transaction.StimulateBackend,
                MachineId = long.Parse(this.Transaction.MachineId),
                LocaleId = this.Transaction.LocaleId,
                ServiceType = MawaqifBackendProxy.MawaqifServiceType.PermitRenewal,
                Language = (Transaction.SelectedLanguageKey=="english")?MawaqifBackendProxy.KioskLanguage.English: MawaqifBackendProxy.KioskLanguage.Arabic,
            };

            RPBalanceResponse response = new RPBalanceResponse
            {
                ResponseCode = Convert.ToInt16(MBMEServiceResponse.Failed).ToString()
            };


            ValidateCertificate.RegisterCallback();
            using (MBMEMawaqifServiceClient client = new MBMEMawaqifServiceClient())
            {
                try
                {
                    response = client.RPGetBalance(request);
                }
               
                catch (Exception ex)
                {
                    outputTrigger = ERRORACTION;
                    if (log.IsErrorEnabled) log.Error(ex.Message); 
                    //Trace.WriteLine(ex.Message);
                    // KS TODO: Logging.
                }
                finally
                {
                    this.Transaction.TxnId = response.TxnId;
                    this.Transaction.ReceiptNumber = response.TxnId.ToString();
                    ValidateCertificate.DeregisterCallback();
                }
            }


            //KS TODO : Logging.
            if (response.ResponseCode == Convert.ToInt16(MBMEServiceResponse.Success).ToString())
            {
                this.Transaction.AccountNumber = response.AccountNumber;

                 
                this.Transaction.IssueDate =  response.IssuedDate.ToString((this.Transaction.LocaleId=="en")?
                    "dd/MM/yyyy" : "yyyy/MM/dd");
                    this.Transaction.ExpiryDate = response.ExpiryDate.ToString((this.Transaction.LocaleId == "en")?
                    "dd/MM/yyyy" : "yyyy/MM/dd");

                this.Transaction.BalanceDue = Convert.ToDouble(response.PermitCost).ToString("0.00");
                this.Transaction.AmountDue = Convert.ToDouble(response.PermitCost).ToString("0.00");
                
                
                //this.Transaction.AmountPaid = response.BalanceAmount;
                outputTrigger = SUBMITACTION;
            }
            else
            {
                this.Transaction.AccountNumber = string.Empty;
                if (this.Transaction.InvalidCount < this.State.RetryCount)
                {
                    if (response.ResponseCode == Convert.ToInt16(MBMEServiceResponse.BillerError).ToString())
                    {
                        Transaction.InvalidCount++;
                        //split the message into two lines
                        if ((string.IsNullOrEmpty(response.PermitStage) ? "" : response.PermitStage.ToLower()) == "permit issued")
                        {
                            Transaction.Message = response.ResponseMessage.Replace("-n","\n");
                        }
                        else
                        {
                            Transaction.Message = response.ResponseMessage;
                        }
                        outputTrigger = BILLERERRORACTION;
                        
                    }
                    else
                    {
                        outputTrigger = ERRORACTION;
                    }
                }
                else
                    outputTrigger = ERRORACTION;
            }
        }

        #endregion
        private void ChangeState(string trigger)
        {
            OnKioskStateChanged(new KioskStateChangedEventArgs(trigger));
        }
    }
}
