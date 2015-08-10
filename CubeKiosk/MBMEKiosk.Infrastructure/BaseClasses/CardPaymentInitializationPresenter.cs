using MBMEKiosk.Infrastructure.Events;

namespace MBMEKiosk.Infrastructure.BaseClasses
{
    /// <summary>
    /// Presenter class for checking 
    /// </summary>
    public class CardPaymentInitializationPresenter : PresenterBase
    {
        protected override void ExecuteSubmitCommand(string param)
        {
            if(Devices.GetCardReader().IsReady())
                base.ExecuteSubmitCommand(param);
            else
                OnKioskStateChanged(new KioskStateChangedEventArgs("PaymentModeSelection"));
        }
    }

    
}
