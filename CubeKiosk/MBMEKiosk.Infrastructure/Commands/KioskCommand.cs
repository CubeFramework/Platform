using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKiosk.Infrastructure.Commands
{
    public class KioskCommand : KioskParameterisedCommand<EmptyCommandArgument>
    {
        public KioskCommand(Action<EmptyCommandArgument> executeMethod)
            : this(executeMethod, null)
        {
        }

        public KioskCommand(Action<EmptyCommandArgument> executeMethod, Func<EmptyCommandArgument, bool> canExecuteMethod)
            : this(executeMethod, canExecuteMethod, false)
        {
        }

        public KioskCommand(Action<EmptyCommandArgument> executeMethod, Func<EmptyCommandArgument, bool> canExecuteMethod, bool notifyOnExecutionCompletion)
            : base(executeMethod, canExecuteMethod, notifyOnExecutionCompletion)
        {
        }
    }
}
