using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using MBMEKiosk.Infrastructure.ObjectModel;

namespace MBMEKiosk.Infrastructure.Interfaces
{
    public interface IReceiptPresenter
    {
        FrameworkElement LoadReceiptXaml(string filePath);
    }
}
