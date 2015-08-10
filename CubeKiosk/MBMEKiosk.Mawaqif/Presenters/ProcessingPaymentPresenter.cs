using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using MawaqifBackendProxy;
using MBMEKiosk.Infrastructure.Events;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.Infrastructure.Utils;
using MBMEKiosk.ObjectModel;
using MBMEKioskLogger.Logger;
using MBMEKioskLogger.LoggerClass;


[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace MBMEKiosk.Mawaqif.Presenters
{
    internal delegate void DProcessPayment();
    internal delegate void DPaymentProcessed(string action);

    public class ProcessingPaymentPresenter : MawaqifPresenterBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool trnsLoggedLocaly = false;

        public override FrameworkElement LoadXaml(IDeviceAgent devices, KioskState state, Infrastructure.ObjectModel.TransactionContextBase transactionContext)
        {
            FrameworkElement viewGrid = base.LoadXaml(devices, state, transactionContext);
            DProcessPayment doPayment = new DProcessPayment(this.DoPayment);
            Devices.GetCashAcceptor().NoteStackedEvent += OnNoteStackedEvent;
            doPayment.BeginInvoke(null, null);
            return viewGrid;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            this.Transaction.CashCycleInProgress = false;
            if (Devices.GetCashAcceptor().IsEnabled())
            {
                Devices.GetCashAcceptor().DisableAsync();
            }

            Devices.GetCashAcceptor().NoteStackedEvent -= OnNoteStackedEvent;

        }

        private void DoPayment()
        {
            string outputTrigger = "error";
            if (log.IsInfoEnabled) log.Info("mawaqif, Do payment started");
            trnsLoggedLocaly = false;
            // Wait for compensating the cash device stack action delay before getting the final amount inserted/paid.
            Thread.Sleep(Int32.Parse(ConfigurationManager.AppSettings["ProcessPaymentStackDelay"]) * 1000);
            double amountInserted = this.Devices.GetCashAcceptor().GetAmountStackedInLastCycle();

            // KS TODO: Log the amount stacked in the last cash cycle.
            // KS TODO: Log the amount stacked in the last cash cycle.
            if (!this.Transaction.CardPayment)
            {
                this.Transaction.AmountPaid = string.Format("{0:0.00}", amountInserted);

                switch (this.Transaction.ServiceType)
                {
                    case MawaqifServiceType.None:
                        break;
                    case MawaqifServiceType.AccountTopUp:
                        this.Transaction.AmountDue = string.Format("{0:0.00}", double.Parse(this.Transaction.BalanceDue) + amountInserted);
                        break;
                    case MawaqifServiceType.PermitRenewal:
                        this.Transaction.AmountDue = string.Format("{0:0.00}", double.Parse(this.Transaction.BalanceDue) - amountInserted);
                        break;
                    case MawaqifServiceType.ViolationPayment:
                        this.Transaction.AmountDue = string.Format("{0:0.00}", double.Parse(this.Transaction.BalanceDue) - amountInserted);
                        break;
                    default:
                        break;
                }
            }
            

            if (log.IsInfoEnabled) log.Info("mawaqif,before try  block in DoPayment.."); 

            try
            {
                
                if (log.IsInfoEnabled) log.Info("mawaqif,this.Transaction.ServiceType:" + this.Transaction.ServiceType.ToString());
                    switch (this.Transaction.ServiceType)
                    {
                        case MawaqifServiceType.None:
                            break;
                        case MawaqifServiceType.AccountTopUp:
                            if (log.IsInfoEnabled) log.Info("mawaqif,before TopUpRequest call ");
                            
                            if (Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["StandAloneMode"]))
                            {
                                outputTrigger = SUBMITACTION;
                            }
                            else
                            {
                                outputTrigger = TopUpRequest(outputTrigger);
                            }
                            if (log.IsInfoEnabled) log.Info("mawaqif,after TopUpRequest call ");
                            break;
                        case MawaqifServiceType.PermitRenewal:
                            if (log.IsInfoEnabled) log.Info("mawaqif,before rpRequest call ");
                            
                            if (Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["StandAloneMode"]))
                            {
                                outputTrigger = SUBMITACTION;
                            }
                            else
                            {
                                outputTrigger = rpRequest(outputTrigger);
                            }
                            if (log.IsInfoEnabled) log.Info("mawaqif,after rpRequest call ");
                            break;
                        case MawaqifServiceType.ViolationPayment:
                            if (log.IsInfoEnabled) log.Info("mawaqif,before vpayRequest call ");
                            
                            if (Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["StandAloneMode"]))
                            {
                                outputTrigger = SUBMITACTION;
                            }
                            else
                            {
                                outputTrigger = vpayRequest(outputTrigger);
                            }
                            if (log.IsInfoEnabled) log.Info("mawaqif,after vpayRequest call ");
                            break;
                        default:
                            break;
                    }
                 
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled) log.Error("mawaqif,caught general exception in Dopayment try block.. " + e.Message);
                //// KS TODO : Logging.
                this.Transaction.PostingFailed = true;
            }
            finally
            {
                //outputTrigger = SUBMITACTION;
                if (!trnsLoggedLocaly)
                {
                    //bool trans = this.LogTransactionToLocalDb();
                    trnsLoggedLocaly = true;
                }
                ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DPaymentProcessed(ChangeState), outputTrigger);
            }
            if (log.IsInfoEnabled) log.Info("mawaqif, Dopayment ended ");
        }

        private void OnNoteStackedEvent(int denominationStacked, string allowedDenominations)
        {
            CashCycleLog obj = null;
            try
            {
                obj = new CashCycleLog()
                {
                    TxnId = this.Transaction.Id,
                    Accepted = 1,
                    NoteId = 0,
                    NoteVal = denominationStacked,
                    TimeIns = DateTime.Now,
                    Uploaded = 0,
                    KioskId = this.Transaction.MachineId
                };

                DBLogger.AddTxnCashCycle(obj);
                if (log.IsInfoEnabled) log.Info("mawaqif - added entry in tbltxncashcycle using OnNoteStackedEvent." + "denominationStacked:" + denominationStacked.ToString() + ",allowedDenominations:" + allowedDenominations.ToString() + ",TransactionId:" + this.Transaction.Id);
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("mawaqif - Caught exception in  OnNoteStackedEvent method. " + "denominationStacked:" + denominationStacked.ToString() + ",allowedDenominations:" + allowedDenominations.ToString() + ",TransactionId:" + this.Transaction.Id + ",And Exception is:" + ex.Message);
            }
            finally
            {
                //  ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUI(UpdateAmountPaidAndDue), denominationStacked, allowedDenominations, false);
            }
        }

        # region WebService Request

        private string vpayRequest(string outputTrigger)
        {
            mwqBillerServiceProxy.PostPaymentPVTRequest request = new mwqBillerServiceProxy.PostPaymentPVTRequest
             {
                 kioskPayDateTime = this.Transaction.Date,
                 TransId = Convert.ToInt64(this.Transaction.Id),
                 ticketNumber = this.Transaction.AccountNumber,
                 CurrentBalance = Convert.ToDouble(this.Transaction.BalanceDue),
                 //NewBalance = this.Transaction.AmountDue,
                 paidAmount = Convert.ToDouble(this.Transaction.AmountPaid),
                 KioskId = this.Transaction.MachineId,
                 paymentIdentifier = this.Transaction.Id,
                 //ticketNumber = this.Transaction.Id,
                 //StimulateResponse = this.Transaction.StimulateBackend,
                 //Language = (Transaction.SelectedLanguageKey == "english") ? KioskLanguage.English :KioskLanguage.Arabic,
                 //ServiceType = MawaqifBackendProxy.MawaqifServiceType.ViolationPayment,
                 //PaymentMode = (this.Transaction.CardPayment) ? MBMEPaymentMode.CARD : MBMEPaymentMode.CASH,
                 //AppliedFee = this.Transaction.AppliedFeeAmount
             };

            mwqBillerServiceProxy.PostPaymentPVTResponse response = new  mwqBillerServiceProxy.PostPaymentPVTResponse { ResponseCode = -2 };

            //Kiosk Stimulate Response
            if (Boolean.Parse(ConfigurationManager.AppSettings["StandAloneMode"]))
            {
                if (log.IsInfoEnabled) log.Info("mawaqif, Checking standalone configuration" + Boolean.Parse(ConfigurationManager.AppSettings["StandAloneMode"]).ToString());
                outputTrigger = SUBMITACTION;
                Thread.Sleep(1000);
            }
            else
            {
                try
                {
                    //ValidateCertificate.RegisterCallback();
                    using (mwqBillerServiceProxy.BillerServiceClient client = new  mwqBillerServiceProxy.BillerServiceClient())
                    {
                        response = client.PostPVTPayment(request);
                        trnsLoggedLocaly = true;
                    }
                }
                catch (EndpointNotFoundException ex)
                {
                    this.Transaction.PostingFailed = true;
                    Trace.WriteLine(ex.Message);

                }
                catch (CommunicationException ex)
                {
                    this.Transaction.PostingFailed = true;
                    Trace.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    this.Transaction.PostingFailed = true;
                    Trace.WriteLine(ex.Message);
                }
                finally
                {
                    //ValidateCertificate.DeregisterCallback();
                    //this.Transaction.TxnId = response.TxnId;

                    //if (response.TxnId > 0)
                    //{
                    //    this.Transaction.TxnId = response.TxnId;
                    //    this.Transaction.ReceiptNumber = response.TxnId.ToString();
                    //}

                    this.Transaction.TxnId = Convert.ToInt64(response.BillerTxnRefNum);
                    this.Transaction.ReceiptNumber = response.BillerTxnRefNum;

                    if (!trnsLoggedLocaly)
                    {
                        bool trans = this.LogTransactionToLocalDb();
                        trnsLoggedLocaly = true;
                    }
                    outputTrigger = SUBMITACTION;
                }

                //Check for Biller Response
                if (response.ResponseCode == 0)
                {
                    outputTrigger = SUBMITACTION;
                }
                else
                {
                    if (response.ResponseCode == 3)
                    {
                        this.Transaction.Message = response.response_message;
                        outputTrigger = BILLERERRORACTION;
                        this.Transaction.TransactionFailed = true;
                    }
                    else
                    {
                        outputTrigger = SUBMITACTION;
                        this.Transaction.TransactionFailed = false;
                    }

                }

            }



            return outputTrigger;
        }


        ///// <summary>
        ///// Violation Payment Request
        ///// </summary>
        ///// <param name="outputTrigger"></param>
        ///// <returns></returns>
        //private string vpayRequest(string outputTrigger)
        //{
        //    PVTPaymentRequest request = new PVTPaymentRequest
        //    {
        //        TransactionTime = DateTime.Now,
        //        TransactionId = this.Transaction.Id,
        //        AccountNumber = this.Transaction.AccountNumber,
        //        LastBalance = this.Transaction.BalanceDue,
        //        NewBalance = this.Transaction.AmountDue,
        //        Amount = this.Transaction.AmountPaid,
        //        MachineId = long.Parse(this.Transaction.MachineId),
        //        ReceiptNo = this.Transaction.Id,
        //        StimulateResponse = this.Transaction.StimulateBackend,
        //        Language = (Transaction.SelectedLanguageKey == "english") ? KioskLanguage.English :
        //        KioskLanguage.Arabic,
        //        ServiceType = MawaqifBackendProxy.MawaqifServiceType.ViolationPayment,
        //        PaymentMode = (this.Transaction.CardPayment) ? MBMEPaymentMode.CARD : MBMEPaymentMode.CASH,
        //        AppliedFee = this.Transaction.AppliedFeeAmount
        //    };

        //    PVTPaymentResponse response = new PVTPaymentResponse { ResponseCode = "-2" };

        //         //Kiosk Stimulate Response
        //    if (Boolean.Parse(ConfigurationManager.AppSettings["StandAloneMode"]))
        //    {
        //        if (log.IsInfoEnabled) log.Info("mawaqif, Checking standalone configuration" + Boolean.Parse(ConfigurationManager.AppSettings["StandAloneMode"]).ToString());
        //        outputTrigger = SUBMITACTION;
        //        Thread.Sleep(1000);
        //    }
        //    else
        //    {
        //        try
        //        {
        //            ValidateCertificate.RegisterCallback();
        //            using (MBMEMawaqifServiceClient client = new MBMEMawaqifServiceClient())
        //            {
        //                response = client.PVTPostPayment(request);
        //                trnsLoggedLocaly = true;
        //            }
        //        }
        //        catch (EndpointNotFoundException ex)
        //        {
        //            this.Transaction.PostingFailed = true;
        //            Trace.WriteLine(ex.Message);

        //        }
        //        catch (CommunicationException ex)
        //        {
        //            this.Transaction.PostingFailed = true;
        //            Trace.WriteLine(ex.Message);
        //        }
        //        catch (Exception ex)
        //        {
        //            this.Transaction.PostingFailed = true;
        //            Trace.WriteLine(ex.Message);
        //        }
        //        finally
        //        {
        //            ValidateCertificate.DeregisterCallback();
        //            this.Transaction.TxnId = response.TxnId;

        //            if (response.TxnId > 0)
        //            {
        //                this.Transaction.TxnId = response.TxnId;
        //                this.Transaction.ReceiptNumber = response.TxnId.ToString();
        //            }

        //            if (!trnsLoggedLocaly)
        //            {
        //                bool trans = this.LogTransactionToLocalDb();
        //                trnsLoggedLocaly = true;
        //            }
        //            outputTrigger = SUBMITACTION;
        //        }

        //        //Check for Biller Response
        //        if (response.ResponseCode == ((int)MBMEServiceResponse.Success).ToString())
        //        {
        //            outputTrigger = SUBMITACTION;
        //        }
        //        else
        //        {
        //            if (response.ResponseCode == Convert.ToInt16(MBMEServiceResponse.BillerError).ToString())
        //            {
        //                this.Transaction.Message = response.ResponseMessage;
        //                outputTrigger = BILLERERRORACTION;
        //                this.Transaction.TransactionFailed = true;
        //            }
        //            else
        //            {
        //                outputTrigger = SUBMITACTION;
        //                this.Transaction.TransactionFailed = false;
        //            }

        //        }

        //    }
                
            

        //    return outputTrigger;
        //}

        /// <summary>
        /// Renewal Permit Request
        /// </summary>
        /// <param name="outputTrigger"></param>
        /// <returns></returns>
        private string rpRequest(string outputTrigger)
        {
            RPPaymentRequest request = new RPPaymentRequest
            {
                TransactionTime = DateTime.Now,
                TransactionId = this.Transaction.Id,
                AccountNumber = this.Transaction.AccountNumber,
                LastBalance = this.Transaction.BalanceDue,
                NewBalance = this.Transaction.AmountDue,
                Amount = this.Transaction.AmountPaid,
                MachineId = long.Parse(this.Transaction.MachineId),
                StimulateResponse = this.Transaction.StimulateBackend,
                Language = (Transaction.SelectedLanguageKey == "english") ? KioskLanguage.English :
                KioskLanguage.Arabic,
                ServiceType = MawaqifBackendProxy.MawaqifServiceType.PermitRenewal,
                PaymentMode = (this.Transaction.CardPayment) ? MBMEPaymentMode.CARD : MBMEPaymentMode.CASH,
                AppliedFee = this.Transaction.AppliedFeeAmount
            };

            RPPaymentResponse response = new RPPaymentResponse { ResponseCode = "-2" };

            try
            {
                ValidateCertificate.RegisterCallback();
                using (MBMEMawaqifServiceClient client = new MBMEMawaqifServiceClient())
                {
                    response = client.RPPostPayment(request);
                    trnsLoggedLocaly = true;
                    this.Transaction.TxnId = response.TxnId;
                }
            }
            catch (EndpointNotFoundException ex)
            {
                Transaction.PostingFailed = true;
                outputTrigger = ERRORACTION;
                Trace.WriteLine(ex.Message);
            }
            catch (CommunicationException ex)
            {
                Transaction.PostingFailed = true;
                outputTrigger = ERRORACTION;
                Trace.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Transaction.PostingFailed = true;
                Trace.WriteLine(ex.Message);
            }
            finally
            {
                ValidateCertificate.DeregisterCallback();

                if (response.TxnId > 0)
                {
                    this.Transaction.TxnId = response.TxnId;
                    this.Transaction.ReceiptNumber = response.TxnId.ToString();
                }

                if (!trnsLoggedLocaly)
                {
                    bool trans = this.LogTransactionToLocalDb();
                    trnsLoggedLocaly = true;
                }
            }

            if (response.ResponseCode == "0")
            {
                outputTrigger = SUBMITACTION;
            }
            else
            {
                if (response.ResponseCode == Convert.ToInt16(MBMEServiceResponse.BillerError).ToString())
                {
                    this.Transaction.Message = response.ResponseMessage;
                    outputTrigger = BILLERERRORACTION;
                    //outputTrigger = SUBMITACTION;
                    this.Transaction.TransactionFailed = true;
                }
                else
                {
                    outputTrigger = ERRORACTION;
                    this.Transaction.TransactionFailed = false;
                }
            }

            return outputTrigger;
        }

        /// <summary>
        /// Mawaqif Account Top Up Call
        /// </summary>
        /// <param name="outputTrigger"></param>
        /// <returns>eg error or submit</returns>
        private string TopUpRequest(string outputTrigger)
        {
            if (log.IsInfoEnabled) log.Info("mawaqif,TopUpRequest started..");
            TopUpPaymentRequest request = new TopUpPaymentRequest
            {
                TransactionTime = DateTime.Now,
                TransactionId = this.Transaction.Id,
                AccountNumber = this.Transaction.AccountNumber,
                PaymentMode = (this.Transaction.CardPayment) ? MBMEPaymentMode.CARD : MBMEPaymentMode.CASH,
                LastBalance = this.Transaction.BalanceDue,
                NewBalance = this.Transaction.AmountDue,
                Amount = this.Transaction.AmountPaid,               
                MachineId = long.Parse(this.Transaction.MachineId),
                StimulateResponse = this.Transaction.StimulateBackend,
                ClientTxnId = this.Transaction.Id,
                Language = (Transaction.SelectedLanguageKey == "english") ? KioskLanguage.English:
                KioskLanguage.Arabic,
                ServiceType = MawaqifBackendProxy.MawaqifServiceType.AccountTopUp,
                AppliedFee = this.Transaction.AppliedFeeAmount
             
            };

            if (log.IsInfoEnabled) log.Info("mawaqif,TopUpPaymentRequest initialized..");



            TopUpPaymentResponse response = new TopUpPaymentResponse { ResponseCode = "-2" };

            try
            {
                if (this.Transaction.AmountPaid != "0.00")
                {
                    ValidateCertificate.RegisterCallback();
                    if (log.IsInfoEnabled) log.Info("mawaqif,before TopUpPostPayment call..");
                    using (MBMEMawaqifServiceClient client = new MBMEMawaqifServiceClient())
                    {
                        response = client.TopUpPostPayment(request);
                        trnsLoggedLocaly = true;
                        if (log.IsInfoEnabled) log.Info("mawaqif,TopUpPostPayment success.." + response.ResponseMessage);
                    }
                }
                else
                {
                    trnsLoggedLocaly = true;
                }
            }           
            catch (TimeoutException ex)
            {
                Trace.WriteLine("mawaqif,caught TimeoutException exception..");
                if (log.IsErrorEnabled) log.Error("mawaqif,caught TimeoutException exception.." + ex.Message.ToString());
                outputTrigger = "error";
                this.TransactionContext.PostingFailed = true;
                Trace.WriteLine(ex.Message);
            
            }
            catch (EndpointNotFoundException ex)
            {
                if (log.IsErrorEnabled) log.Error("mawaqif,found EndpointNotFoundException in TopUpRequest " + ex.Message);     
                outputTrigger = "error";

                this.Transaction.PostingFailed = true;

                Trace.WriteLine(ex.Message);
            }
            catch (CommunicationException ex)
            {
                if (log.IsErrorEnabled) log.Error("mawaqif,found CommunicationException in TopUpRequest " + ex.Message);
               
                outputTrigger = "error";

                this.Transaction.PostingFailed = true;

       
            }
            catch (Exception ex)
            {

                if (log.IsErrorEnabled) log.Error("mawaqif,found general exception in TopUpRequest " + ex.Message);

                this.Transaction.PostingFailed = true;

                Trace.WriteLine(ex.Message);
            }
            finally
            {
                if (log.IsInfoEnabled) log.Info("mawaqif,certificate deregistered..");
                ValidateCertificate.DeregisterCallback();

                if (response.TxnId > 0)
                {
                    this.Transaction.TxnId = response.TxnId;
                    this.Transaction.ReceiptNumber = response.TxnId.ToString();
                }

                if (!trnsLoggedLocaly)
                {
                    bool trans = this.LogTransactionToLocalDb();
                    trnsLoggedLocaly = true;
                }
            }


            if (response.ResponseCode == Convert.ToInt16(MBMEServiceResponse.Success).ToString())
            {
                outputTrigger = SUBMITACTION;
                 
            }
            else
            {
                if (response.ResponseCode == Convert.ToInt16(MBMEServiceResponse.BillerError).ToString())
                {
                    this.Transaction.Message = response.ResponseMessage;
                    outputTrigger = BILLERERRORACTION;
                    this.Transaction.TransactionFailed = true;
                }
                else
                {
                    outputTrigger = SUBMITACTION;
                    this.Transaction.TransactionFailed = true;
                }
            }

            outputTrigger = SUBMITACTION;
            if (log.IsInfoEnabled) log.Info("mawaqif,TopUpRequest ended ..And outputTrigger=" + outputTrigger);
            return outputTrigger;
           
        }

        #endregion

        # region private methods

        
        private void ChangeState(string trigger)
        {
            OnKioskStateChanged(new KioskStateChangedEventArgs(trigger));
        }

        #endregion
    }
}
