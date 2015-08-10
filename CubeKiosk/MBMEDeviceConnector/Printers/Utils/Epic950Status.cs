using System;

namespace Ithaca
{
    public class Epic950Status : PrinterStatus
    {
        private Epic950Status()
	    {
	    }
        
        private bool TopOfForm = false;
        private bool TopOfForm1 = false;
        private bool ChasisOpen = false;
        private bool OutOfTicket = false;
        
        enum printerFalutCodes
        {
            PRINTER_READY = 0,
            TOP_OF_FORM,
            RESERVED_1,
            HEAD_IS_UP,
            CHASSIS_IS_OPEN,
            OUT_OF_TICKET,
            RESERVED_2,
            RESERVED_3,
            TICKET_LOW,
            TICKET_IN_PRINTER,
            TOP_OF_FORM_1,
            RESERVED_4,
            BARCODE_COMPLETED,
            VALIDATION_COMPLETED,
            TICKET_IN_PATH,
            PAPER_JAM
        };

        public static PrinterStatus GetInstance()
        {
            if (instance == null)
            {
                instance = new Epic950Status();
            }
            return instance;
        }
        
        public bool IsTopOfForm()
        {
            return TopOfForm;
        }

        public bool IsTopOfForm1()
        {
            return TopOfForm1;
        }
        public bool IsChasisOpen()
        {
            return ChasisOpen;
        }

        public bool IsOutOfTicket()
        {
            return OutOfTicket;
        }

        public void InterpretStatus(byte sbyte1,byte sbyte2)
        {
            TopOfForm = false;
            TopOfForm1 = false;
            ChasisOpen = false;
            OutOfTicket = false;
            Ready = false;
            TicketLow = false;
            TicketInPrinter = false;
            PaperJam = false;
            TicketInPath = false;
            HeadIsUp = false;

            for (byte i = 0; i < 8; i++)
            {
                System.Console.WriteLine(" Num : " + (1 << i));
                if (0 != (byte)(sbyte1 & (1 << i)))
                {
                    switch(i)
                    {
                        case (byte)printerFalutCodes.PRINTER_READY:
                            this.Ready = true;
                        break;

                        case (byte)printerFalutCodes.TOP_OF_FORM:
                            this.TopOfForm = true;
                        break;

                        case (byte)printerFalutCodes.HEAD_IS_UP:
                        this.HeadIsUp = true;
                        break;

                        case (byte)printerFalutCodes.CHASSIS_IS_OPEN:
                        this.ChasisOpen = true;
                        break;

                        case (byte)printerFalutCodes.OUT_OF_TICKET:
                        this.OutOfTicket = true;
                        break;
                    }
                }
            }
            for (byte i = 0; i < 8; i++)
            {
                System.Console.WriteLine(" Num : " + (1 << i));
                if (0 != (byte)(sbyte2 & (1 << i)))
                {
                    switch (i + 8)
                    {
                        case (byte)printerFalutCodes.TICKET_LOW:
                            this.TicketLow = true;
                            break;

                        case (byte)printerFalutCodes.TICKET_IN_PRINTER:
                            this.TicketInPrinter = true;
                            break;

                        case (byte)printerFalutCodes.TOP_OF_FORM_1:
                            this.TopOfForm1 = true;
                            break;

                        case (byte)printerFalutCodes.TICKET_IN_PATH:
                            this.TicketInPath = true;
                            break;

                        case (byte)printerFalutCodes.PAPER_JAM:
                            this.PaperJam = true;
                            break;
                    }
                }
            }
        }
    }
}
