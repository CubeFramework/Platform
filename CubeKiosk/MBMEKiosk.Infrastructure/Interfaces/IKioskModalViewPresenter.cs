using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MBMEKiosk.Infrastructure.Interfaces
{
    public interface IKioskModalViewPresenter
    {
        FrameworkElement LoadXaml(string filePath, FrameworkElement parent);
    }
}
