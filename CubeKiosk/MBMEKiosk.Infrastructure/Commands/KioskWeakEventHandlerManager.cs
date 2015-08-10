using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBMEKiosk.Infrastructure.Commands
{
    public sealed class KioskWeakEventHandlerManager
    {
        private KioskWeakEventHandlerManager()
        {
        }

        public static void InvokeHandlers(object sender, List<WeakReference> handlersWeak)
        {
            if (handlersWeak != null)
            {
                EventHandler[] handlersAvailable = new EventHandler[handlersWeak.Count];

                int handlersAvailableCount = 0;
                handlersAvailableCount = CopyAvailableHandlers(handlersWeak, handlersAvailable, handlersAvailableCount);
                for (int counter = 0; counter < handlersAvailableCount; counter++)
                {
                    InvokeHandler(sender, handlersAvailable[counter]);
                }
            }
        }

        private static int CopyAvailableHandlers(List<WeakReference> handlersWeak, EventHandler[] handlersAvailable, int count)
        {
            for (int counter = handlersWeak.Count - 1; counter >= 0; counter--)
            {
                WeakReference reference = handlersWeak[counter];
                EventHandler handler = reference.Target as EventHandler;

                if (handler == null)
                {
                    handlersWeak.RemoveAt(counter);
                }

                else
                {
                    handlersAvailable[count] = handler;
                    count++;
                }
            }

            return count;
        }

        private static void InvokeHandler(object sender, EventHandler handler)
        {
            if (handler != null)
            {
                handler(sender, EventArgs.Empty);
            }
        }

        public static void AddHandler(ref List<WeakReference> handlersWeak, EventHandler newHandler, int defaultListSize)
        {
            if (handlersWeak == null)
            {
                handlersWeak = defaultListSize > 0 ? new List<WeakReference>(defaultListSize) : new List<WeakReference>();
            }

            handlersWeak.Add(new WeakReference(newHandler));
        }

        public static void RemoveHandler(ref List<WeakReference> handlersWeak, EventHandler oldHandler)
        {
            if (handlersWeak != null)
            {
                for (int counter = handlersWeak.Count - 1; counter >= 0; counter--)
                {
                    WeakReference reference = handlersWeak[counter];
                    EventHandler existingHandler = reference.Target as EventHandler;
                    if (existingHandler == null || existingHandler == oldHandler)
                    {
                        handlersWeak.RemoveAt(counter);
                    }
                }
            }
        }
    }
}
