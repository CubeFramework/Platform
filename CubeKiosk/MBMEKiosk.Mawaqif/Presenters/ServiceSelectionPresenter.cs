using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.Events;

namespace MBMEKiosk.Mawaqif.Presenters
{
    public class ServiceSelectionPresenter : MawaqifPresenterBase
    {
        protected override void ExecuteSubmitCommand(string param)
        {
            string appendKey = null;

            if (this.Transaction.SelectedLanguageKey == "arabic")
                appendKey = "ar";


            switch (param)
            {
                case "topup":
                    {
                        this.Transaction.ServiceType = MawaqifServiceType.AccountTopUp;
                        this.Transaction.ReceiptFooterKey = this.RcptFooterString("mawaqif_txt_rcpt_footer");
                        this.Transaction.ReceiptServiceNameKey = this.RcptServiceNumberString("mawaqif_txt_rcpt_account_number");
                        this.Transaction.AccountNumberLabelText = this.RcptServiceNumberString("mawaqif_txt_rcpt_account_number");
                        this.Transaction.CurrBalanceLabelText = this.RcptServiceNumberString("mawaqif_txt_rcpt_current_balance");
                        this.Transaction.ServiceAmountLabelText = this.RcptServiceNumberString("mawaqif_txt_rcpt_previous_balance");

                        this.Transaction.CurrBalanceLabelTextAR = this.RcptServiceNumberString("mawaqif_txt_rcpt_current_balance_ar");
                        this.Transaction.AccountNumberLabelTextAR = this.RcptServiceNumberString("mawaqif_txt_rcpt_account_number_ar");
                        this.Transaction.ReceiptServiceNameKeyAR = this.RcptServiceNumberString("mawaqif_txt_rcpt_account_number_ar");
                        this.Transaction.ServiceAmountLabelTextAR = this.RcptServiceNumberString("mawaqif_txt_rcpt_previous_balance_ar");
                        break;
                    }
                case "renewal":
                    {
                        this.Transaction.ServiceType = MawaqifServiceType.PermitRenewal;
                        this.Transaction.ReceiptFooterKey = this.RcptFooterString("mawaqif_txt_rcpt_p_footer");
                        this.Transaction.ReceiptServiceNameKey = this.RcptServiceNumberString("mawaqif_txt_rcpt_permit_number");
                        this.Transaction.CurrBalanceLabelText = this.RcptServiceNumberString("mawaqif_txt_rcpt_current_balance");
                        this.Transaction.AccountNumberLabelText = this.RcptServiceNumberString("mawaqif_txt_rcpt_permit_number");
                        this.Transaction.ServiceAmountLabelText = this.RcptServiceNumberString("mawaqif_txt_rcpt_permit_amount");

                        this.Transaction.ReceiptServiceNameKeyAR = this.RcptServiceNumberString("mawaqif_txt_rcpt_permit_number_ar");
                        this.Transaction.CurrBalanceLabelTextAR = this.RcptServiceNumberString("mawaqif_txt_rcpt_current_balance_ar");
                        this.Transaction.AccountNumberLabelTextAR = this.RcptServiceNumberString("mawaqif_txt_rcpt_permit_number_ar");
                        this.Transaction.ServiceAmountLabelTextAR = this.RcptServiceNumberString("mawaqif_txt_rcpt_permit_amount_ar");
                        break;
                    }

                case "violationpay":
                    {
                        this.Transaction.ServiceType = MawaqifServiceType.ViolationPayment;
                        this.Transaction.ReceiptFooterKey = this.RcptFooterString("mawaqif_txt_rcpt_v_footer");
                        
                        ///English Strings
                        this.Transaction.ReceiptServiceNameKey = this.RcptServiceNumberString("mawaqif_txt_rcpt_ticket_number");
                        this.Transaction.CurrBalanceLabelText = this.RcptServiceNumberString("mawaqif_txt_rcpt_pvt_current_balance");
                        this.Transaction.AccountNumberLabelText = this.RcptServiceNumberString("mawaqif_txt_rcpt_ticket_number");
                        this.Transaction.ServiceAmountLabelText = this.RcptServiceNumberString("mawaqif_txt_rcpt_ticket_amount");

                        ///Arabic strings
                        this.Transaction.ReceiptServiceNameKeyAR = this.RcptServiceNumberString("mawaqif_txt_rcpt_ticket_number_ar");
                        this.Transaction.CurrBalanceLabelTextAR = this.RcptServiceNumberString("mawaqif_txt_rcpt_pvt_current_balance_ar");
                        this.Transaction.AccountNumberLabelTextAR = this.RcptServiceNumberString("mawaqif_txt_rcpt_ticket_number_ar");
                        this.Transaction.ServiceAmountLabelTextAR = this.RcptServiceNumberString("mawaqif_txt_rcpt_ticket_amount_ar");
                        break;
                    }

                default:
                    {
                        this.Transaction.ServiceType = MawaqifServiceType.None;
                        param = ERRORACTION;
                        break;
                    }
            }

            this.Transaction.AccountNumber = string.Empty;
            this.Transaction.BalanceDue = "0.00";
            this.Transaction.AmountDue = "0.00";
            this.Transaction.AmountPaid = "0.00";
            OnKioskStateChanged(new KioskStateChangedEventArgs(param));
        }

        protected override bool CanExecuteSubmitCommand(string param)
        {
            return State.KioskActions.Where(a => string.Compare(a.Name.ToLower(), param.ToLower()) == 0).Count() == 1;
        }

        public bool OrderServiceSelection
        {
            get
            {
                if (this.Transaction.SelectedLanguageKey == "english")
                    return true;
                else
                    return false;
            }
        }
    }
}
