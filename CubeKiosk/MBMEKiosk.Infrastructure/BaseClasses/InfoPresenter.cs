using System;
using System.Windows;
using System.Windows.Threading;
using MBMEKiosk.Infrastructure.Events;
using MBMEKiosk.Infrastructure.Interfaces;
using MBMEKiosk.Infrastructure.ObjectModel;
using MBMEKiosk.ObjectModel;

namespace MBMEKiosk.Infrastructure.BaseClasses
{
    public class InfoPresenter : PresenterBase
    {
        private DispatcherTimer timer;

        public override FrameworkElement LoadXaml(IDeviceAgent devices, KioskState state, TransactionContextBase transactionContext)
        {
            timer = new DispatcherTimer();
            FrameworkElement viewGrid = base.LoadXaml(devices, state, transactionContext);
            timer.Tick += OnTimeOut;
            timer.Interval = new TimeSpan(0, 0, state.MessageTimeOut);
            timer.Start();
            return viewGrid;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= OnTimeOut;
                timer = null;
            }
        }

        //protected override void OnTimeOut(object o, EventArgs args)
        //{
        //    //base.OnTimeOut(o, args);
        //    if (timer != null)
        //    {
        //        timer.Stop();
        //        timer.Tick -= OnTimeOut;
        //        timer = null;
        //    }

        //    OnKioskStateChanged(new KioskStateChangedEventArgs("submit"));
        //}
        //public override void OnTimeOut(object o, EventArgs args)
        //{
        //    if (timer != null)
        //    {
        //        timer.Stop();
        //        timer.Tick -= OnTimeOut;
        //        timer = null;
        //    }

        //    OnKioskStateChanged(new KioskStateChangedEventArgs("submit"));
        //}

        protected override int ScreenTimeout
        {
            get
            {
                return this.State.MessageTimeOut;
            }
        }
    }
}
