using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using MBMEKiosk.Infrastructure.BaseClasses;
using MBMEKiosk.Infrastructure.Events;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.Infrastructure.ObjectModel;
using MBMEKiosk.ObjectModel;
using MBMEKioskLogger.LoggerClass;
using MBMEKioskLogger.Logger;
using System.Configuration;
using System.IO;

namespace MBMEKiosk.Mawaqif.Presenters
{
    internal delegate void DUpdateUI(int denominationStacked, string allowedDenominations, bool isCashCycleCompleted);
    internal delegate void DCashCycleProcessed(string action);
    internal delegate void DStartStackDelayNotificationTimer();

    public class CashPaymentPresenter : MawaqifPresenterBase
    {
        
        private string DisplayedDenominations;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool stackCommandIssued;
        private bool submitButtonPressed;
        private int escrowDenomination;

        public override FrameworkElement LoadXaml(IDeviceAgent devices, KioskState state, TransactionContextBase transactionContext)
        {

            FrameworkElement viewGrid = base.LoadXaml(devices, state, transactionContext);

            List<int> expectedDenominations = new List<int>();
            foreach (string item in this.State.Denomination.Split(','))
            {
                int denomination = 0;
                if (int.TryParse(item, out denomination))
                {
                    expectedDenominations.Add(denomination);
                }
            }

            stackCommandIssued = false;
            submitButtonPressed = false;
            StateTimerTimedOut = false;
            escrowDenomination = -1;

            MaxAmountReached = false;
            Devices.GetCashAcceptor().CashCycleInitiatedEvent += OnCashCycleInitiated;
            Devices.GetCashAcceptor().CashInsertedEvent += OnCashInserted;
            Devices.GetCashAcceptor().NoteStackedEvent += OnNoteStackedEvent;
            Devices.GetCashAcceptor().NoteReturnedEvent += OnNoteReturnedEvent;
            ////Devices.GetCashAcceptor().CashCycleCompletedEvent += OnCashCycleCompletedEvent;
            //Devices.GetCashAcceptor().EnableAsync(expectedDenominations, Double.Parse(this.Transaction.BalanceDue), false);
            //this.Transaction.CashCycleInProgress = false;
            this.State.MinAmount = 0;
            this.State.MaxAmount = Convert.ToDouble(this.Transaction.BalanceDue);
            this.Transaction.AmountDue = this.Transaction.BalanceDue;

            //if ((this.State.MinAmount == -1) && (this.State.MinAmount == -1))
            //    Devices.GetCashAcceptor().EnableAsync(expectedDenominations, Double.Parse(this.Transaction.BalanceDue), this.State.MinAmount, this.State.MaxAmount, false);
            //else
            //    
            Devices.GetCashAcceptor().EnableAsync(expectedDenominations, Double.Parse(this.Transaction.BalanceDue), -2, -2, true);
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
            
            Devices.GetCashAcceptor().CashInsertedEvent -= OnCashInserted;
            Devices.GetCashAcceptor().NoteStackedEvent -= OnNoteStackedEvent;
            Devices.GetCashAcceptor().CashCycleInitiatedEvent -= OnCashCycleInitiated;
            Devices.GetCashAcceptor().NoteReturnedEvent -= OnNoteReturnedEvent;
            ////Devices.GetCashAcceptor().CashCycleCompletedEvent -= OnCashCycleCompletedEvent;
            
        }

        protected override void ExecuteSubmitCommand(string param)
        {
            this.Transaction.CashCycleInProgress = false;
            //Devices.GetCashAcceptor().DisableAsync();
            //base.ExecuteSubmitCommand(param);
            submitButtonPressed = true;

            if (Devices.GetCashAcceptor().IsEnabled())
            {
                Devices.GetCashAcceptor().DisableAsync();
            }

            if (log.IsInfoEnabled) log.Info(string.Format(@"Submit Button Pressed. Datetime: {0}. stackCommandIssued: {1} for TransactionID:{2}", DateTime.Now, stackCommandIssued, this.Transaction.Id));

            if (!stackCommandIssued)
            {
                base.ExecuteSubmitCommand(param);
            }
            else
            {
                if (log.IsInfoEnabled) log.Info(string.Format(@"Submit Button Pressed. Datetime: {0}. stackCommandnot executed and TransactionId:{1}", DateTime.Now, this.Transaction.Id));
            }
        }

        protected override bool CanExecuteSubmitCommand(string param)
        {
            bool result = false;        
            
            if (this.Transaction.ServiceType == MawaqifServiceType.PermitRenewal)
                result =    double.Parse(this.Transaction.AmountPaid) == double.Parse(this.Transaction.BalanceDue);
            else if (this.Transaction.ServiceType == MawaqifServiceType.ViolationPayment)
                result =    double.Parse(this.Transaction.AmountPaid) > 0;

            return result;
        }

        private void OnCashCycleInitiated(string allowedDenominations)
        {
            ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUI(UpdateAmountPaidAndDue), 0, allowedDenominations, false);
        }

        private void OnCashInserted(int denominationInEscrow,bool stackFlag)
        {
            if (!this.Transaction.CashCycleInProgress)
            {
                this.Transaction.CashCycleInProgress = true;
            }
            RestartStateTimer();
            //if (log.IsInfoEnabled) log.Info("Fewa - added entry in tbltxncashcycle using OnCashInserted." + "denominationinEscrow:" + denominationInEscrow.ToString() + ",TransactionId:" + this.Transaction.Id);
            //ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUI(UpdateAmountPaidAndDue), 0, allowedDenominations, false);
            // RestartStateTimer();
            if (stackFlag)
            {
                // Start Delay notification timer in order to make sure App doesn't miss any note stacking because of delay in H/W Stacking Cycle.
                // This happens to mostly the last note when the stacked note event arrives after a long delay ranging from 5-35 seconds.
                // MBME wants the App to wait maximum 40 seconds before recording it as missing note exception.
                stackCommandIssued = true;
                escrowDenomination = denominationInEscrow;
                Devices.GetCashAcceptor().IssueStack();
                ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DStartStackDelayNotificationTimer(StartStackDelayNotificationTimer));
                if (log.IsInfoEnabled) log.Info(string.Format(@"On Cash Inserted. Datetime: {0}. StackDelay timer started and stack command issued for TransactionId:{1}", DateTime.Now, this.Transaction.Id));
            }
            if (log.IsInfoEnabled) log.Info("mawaqif - added entry in tbltxncashcycle using OnCashInserted." + "denominationinEscrow:" + denominationInEscrow.ToString() + ",TransactionId:" + this.Transaction.Id); 
            
        }

        /// <summary>
        /// TO Display the Label Maximum amount Displayed
        /// </summary>
        public bool MaxAmountReached { get; set; }

        private void OnNoteStackedEvent(int denominationStacked, string allowedDenominations)
        {
            stackCommandIssued = false;
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

                 //DBLogger.AddTxnCashCycle(obj);
                 if (log.IsInfoEnabled) log.Info("Mawaqif - added entry in tbltxncashcycle using OnNoteStackedEvent." + "denominationStacked:" + denominationStacked.ToString() + ",allowedDenominations:" + allowedDenominations.ToString() + ",TxnId:" + this.Transaction.Id);
             }
             catch (Exception ex)
             {
                 if (log.IsErrorEnabled) log.Error("Mawaqif - Caught exception in  OnNoteStackedEvent method. " + "denominationStacked:" + denominationStacked.ToString() + ",allowedDenominations:" + allowedDenominations.ToString() + ",TxnId:" + obj.TxnId.ToString() + "And Exception is:" + ex.Message);
             }
             finally
             {                 
                 ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUI(UpdateAmountPaidAndDue), denominationStacked, allowedDenominations, false);
                 OnStackDelayNotificationTimeOut(null, null);
             }
        }

        private void OnNoteReturnedEvent(int denominationReturned)
        {
            stackCommandIssued = false;            
            CashCycleLog obj = null;
            try
            {
                if (this.Transaction.CashCycleInProgress)
                {
                    this.Transaction.CashCycleInProgress = false;
                }

                 obj = new CashCycleLog()
                {
                    TxnId = this.Transaction.Id,
                    Accepted = 0,
                    NoteId = 0,
                    NoteVal = denominationReturned,
                    TimeIns = DateTime.Now,
                    Uploaded = 0,
                    KioskId = this.Transaction.MachineId
                };

                //DBLogger.AddTxnCashCycle(obj);
                if (log.IsInfoEnabled) log.Info("Mawaqif - added entry in tbltxncashcycle using OnNoteReturnedEvent." + "denominationReturned:" + denominationReturned.ToString() + ",TxnId:" + this.Transaction.Id);
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error("Mawaqif - Caught exception in  OnNoteReturnedEvent method. " + "denominationReturned:" + denominationReturned.ToString() + ",TxnId:" + obj.TxnId.ToString() + "And Exception is:" + ex.Message);
            }
            finally
            {
                OnStackDelayNotificationTimeOut(null, null);
            } 
        }

        ////private void OnCashCycleCompletedEvent(int denominationStacked)
        ////{
        ////    ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DUpdateUI(UpdateAmountPaidAndDue), denominationStacked, string.Empty, true);
        ////}

        private void UpdateAmountPaidAndDue(int denominationStacked, string allowedDenominations, bool isCashCycleCompleted)
        {
            // Added By JK on 28/08/12 to make sure Main and Back button doesn't appear in the Payment screen. 
            //this.Transaction.CashCycleInProgress = true;
            this.DisplayedDenominations = allowedDenominations + ",";
            if (denominationStacked == 0)
            {
                this.OnRequestLayoutUpdate();
                return;
            }
            if(!this.Transaction.CashCycleInProgress)
              this.Transaction.CashCycleInProgress = true;
            double updatedAmountPaid = double.Parse(this.Transaction.AmountPaid) + denominationStacked;
            this.Transaction.AmountPaid = string.Format("{0:0.00}", updatedAmountPaid);
            this.Transaction.AmountDue = string.Format("{0:0.00}", double.Parse(this.Transaction.BalanceDue) - updatedAmountPaid);
            if (Convert.ToDouble(this.Transaction.AmountPaid) == Convert.ToDouble(this.Transaction.BalanceDue))
            {
                MaxAmountReached = true;
                this.DisplayedDenominations = string.Empty;
            }
            
            this.ReevaulateIfCommandsCanExecute();
            this.OnRequestLayoutUpdate();
        }

        protected override void OnStackDelayNotificationTimeOut(object o, EventArgs args)
        {
            if (log.IsInfoEnabled) log.Info("OnStackDelayNotificationTimeOut called for TransactionId:" + this.Transaction.Id);            
            
            base.OnStackDelayNotificationTimeOut(o, args);
            if (stackCommandIssued)
            {
                if (log.IsInfoEnabled) log.Info("On Stack Delay Notification Timeout. stackCommandIssued for TransactionId:" + this.Transaction.Id);                

                //Logger.LogEvents(new EventLogger
                //{
                //    KioskId = Convert.ToInt32(KioskAppConfig.KioskId),
                //    EventDtTm = DateTime.Now,
                //    EventId = "200",
                //    Desc = this.Transaction.Id + "," + "Mawaqif," + "Denom:" + escrowDenomination.ToString()
                //}, LOGTO.DATABASE);
            }
            stackCommandIssued = false;            
            if ((submitButtonPressed) || (StateTimerTimedOut))
            {
                if (log.IsInfoEnabled) log.Info(string.Format(@"On Stack Delay Notification Timeout. Datetime: {0}. submit kiosk state change for TransactionId:{1}", DateTime.Now, this.Transaction.Id));
                ViewGrid.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DCashCycleProcessed(ChangeState), "submit");
            }
        }

        public bool IsDenom5Allowed
        {
            get
            {
                if (this.DisplayedDenominations == null)
                    return false;
                if (!this.DisplayedDenominations.Contains("5,"))
                    return false;
                else
                    return true;
            }

        }

        public bool IsDenom10Allowed
        {
            get
            {
                if (this.DisplayedDenominations == null)
                    return false;
                if (!this.DisplayedDenominations.Contains("10,"))
                    return false;
                else
                    return true;
            }

        }
        public bool IsDenom20Allowed
        {
            get
            {
                if (this.DisplayedDenominations == null)
                    return false;
                if (!this.DisplayedDenominations.Contains("20,"))
                    return false;
                else
                    return true;
            }

        }

        public bool IsDenom50Allowed
        {
            get
            {
                if (this.DisplayedDenominations == null)
                    return false;
                if (!this.DisplayedDenominations.Contains("50,"))
                    return false;
                else
                    return true;
            }

        }
        
        public bool IsDenom100Allowed
        {
            get
            {
                if (this.DisplayedDenominations == null)
                    return false;
                if (!this.DisplayedDenominations.Contains("100,"))
                    return false;
                else
                    return true;
            }

        }
        
        public bool IsDenom200Allowed
        {
            get
            {
                if (this.DisplayedDenominations == null)
                    return false;
                if (!this.DisplayedDenominations.Contains("200,"))
                    return false;
                else
                    return true;
            }

        }
        
        public bool IsDenom500Allowed
        {
            get
            {
                if (this.DisplayedDenominations == null)
                    return false;
                if (!this.DisplayedDenominations.Contains("500,"))
                    return false;
                else
                    return true;

            }

        }
        
        public bool IsDenom1000Allowed
        {
            get
            {
                if (this.DisplayedDenominations == null)
                    return false;
                if (!this.DisplayedDenominations.Contains("1000,"))
                    return false;
                else
                    return true;
            }

        }

        public string PaymentScreenHeaderText
        {
            get
            {
                string strRes = null;
                switch (this.Transaction.ServiceType)
                {
                    case MawaqifServiceType.PermitRenewal:
                        strRes = this.ViewGrid.TryFindResource("mawaqif_txt_pm_title_money") as string;
                        break;
                    case MawaqifServiceType.ViolationPayment:
                        strRes = this.ViewGrid.TryFindResource("mawaqif_txt_vm_title_money") as string;
                        break;
                    
                }

                return strRes;
                
            }
        }

        public bool StateTimerTimedOut { get; set; }


        protected override void OnTimeOut(object o, EventArgs args)
        {
               try
               {
                   if (!stackCommandIssued)
                   {
                       this.Transaction.CashCycleInProgress = false;
                       Devices.GetCashAcceptor().DisableAsync();

                       if (double.Parse(this.Transaction.AmountPaid) > 0)
                       {
                           if (StateTimer != null)
                           {
                               StateTimer.Stop();
                               StateTimer.Tick -= OnTimeOut;
                               StateTimer = null;
                           }

                           if (State.KioskActions.Where(a => a.Name.ToLower() == "autosubmit").Count() == 1)
                           {
                               OnKioskStateChanged(new KioskStateChangedEventArgs("autosubmit"));
                           }
                           else
                           {
                               OnKioskStateChanged(new KioskStateChangedEventArgs("submit"));
                           }
                       }
                       else
                       {
                           base.OnTimeOut(o, args);
                       }
                   }
                   else
                   {
                       if (StateTimer != null)
                       {
                           StateTimer.Stop();
                           StateTimer.Tick -= OnTimeOut;
                           StateTimer = null;
                       }
                       StateTimerTimedOut = true;
                   }
              }
              catch (Exception ex)
              {
                  if (log.IsErrorEnabled) log.Error("Fewa - caught exception in  OnTimeOut." + ex.Message + "TransactionId:" + this.Transaction.Id);
              }
        }

        private void ChangeState(string trigger)
        {
            OnKioskStateChanged(new KioskStateChangedEventArgs(trigger));
        }

    }

}
