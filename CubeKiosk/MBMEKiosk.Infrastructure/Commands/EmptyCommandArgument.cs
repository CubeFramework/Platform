using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKiosk.Infrastructure.Commands
{
    public sealed class EmptyCommandArgument
    {
        public static readonly EmptyCommandArgument Empty = new EmptyCommandArgument();

        private EmptyCommandArgument()
        {
        }
    }
}
