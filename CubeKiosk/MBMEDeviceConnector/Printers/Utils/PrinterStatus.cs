using System;

namespace Ithaca
{
    public abstract class PrinterStatus
    {
        protected static PrinterStatus instance = null;
        protected bool Ready = false;
        protected bool TicketLow = false;
        protected bool TicketInPrinter = false;
        protected bool PaperJam = false;
        protected bool TicketInPath = false;
        protected bool HeadIsUp = false;

        public bool IsPrinterReady()
        {
            return (Ready);
        }

        public bool IsLevelLow()
        {
            return (TicketLow);
        }

        public bool IsTicketLoaded()
        {
            return (TicketInPrinter);
        }

        public bool IsPaperJam()
        {
            return (PaperJam);
        }

        public bool IsPrinterHeadCorrectlyPlaced()
        {
            return (HeadIsUp);
        }

        public bool IsTicketInPath()
        {
            return (TicketInPath);
        }

  }
}
