using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Zebra
{
    public class TTP2030Status
    {
        protected static TTP2030Status instance = null;
        private TTP2030Status()
	    {
	    }

        private bool PrinterReady = false;
        private bool PresenterJammed = false; //Presenter Jam, Paper can't be Ejected/Retracted.
        private bool CutterJammed = false; // Cutter can't return to home position.
        private bool OutOfPaper = false; // Printer out of paper.
        private bool PrintHeadLifted = false; // Printer head lifted.
        private bool PaperFeedError = false; // Paper feed error (under head).
        private bool TemperatureError = false; //  Print head is above 60 C.
        private bool PresenterNotRunning = false; //Presenter Jam, motor not running
        private bool PaperJam = false; // Paper Jam during retract. 
        private bool BlackMarkNotFound = false; // Black mark not found.
        private bool BlackMarkCalibrationError = false; // Black mark calibration error.
        private bool IndexError = false; //
        private bool ChecksumError = false;
        private bool WrongFirmware = false;
        private bool NoFirmwareOrWrongFirmwareChecksum = false;
        private bool RetractTimedout = false;
        private bool Paused = false;
        private bool Undefined = false;
        private bool PaperNearEnd = false;
        private short printerStatus;
        private short printerState;
        private string printerStatusDesc;

        enum ZebraStatusReport
        {
            RESERVERD_1=0,
            RESERVERD_2,
            RESERVERD_3,
            ERROR_BLACK_MARK,
            RESERVERD_4,
            POWER_OFF,
            PRINT_DATA_EXISTS,
            STATUS_CODE_AVAILABLE,
            OUT_OF_PAPER,
            PAPER_NEAR_END,
            RESERVED_5,
            PAPER_AT_PRESENTER,
            CUTTER_STUCK,
            PRINTHEAD_LIFTED,
            RESERVED_6,
            RETRACT_UNIT_MOUNTED
        };
        enum ZebraStatusCodes
        {
            PRINTER_READY=0,
            PAPER_LEFT_IN_PRESENTER,
            CUTTER_JAMMED,
            OUT_OF_PAPER,
            PRINTHEAD_LIFTED,
            PAPER_FEED_ERROR,
            TEMPERATURE_ERROR,
            PRESENTER_NOT_RUNNING,
            PAPER_JAM_DURING_RETRACT,
            BLACK_MARK_NOT_FOUND,
            BLACK_MARK_CALIBRATION_ERROR,
            INDEX_ERROR,
            CHECKSUM_ERROR,
            WRONG_FIRMWARE,
            FIRMWARE_CANNOT_START,
            RETRACT_FUNCTION_TIMED_OUT,
            PAUSED,
            UNDEFINED_ERROR,
            PAPER_NEAR_END
        }

        public static TTP2030Status GetInstance()
        {
            if (instance == null)
            {
                instance = new TTP2030Status();
            }
            return instance;
        }

        public short getConnectivityState()
        {
            return printerState;
        }

        public short getDeviceStatus()
        {
            return printerStatus;
        }

        public string getDetails()
        {
            return printerStatusDesc;
        }

        public void InterpretStatus(byte[] status,int bytesRead)
        {
            using (FileStream stream = new FileStream(@"Printer.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(@"Printer Status bytes received..");
                writer.Flush();
                writer.Close();
            }
            if (bytesRead == 2) // This is a Status Report received in response to ESC ENQ 6 
            {
                if ((status[1] & (byte)128) == 128) // Printer Ready.
                {
                    PrinterReady = true;
                    printerStatus = 1;
                    printerStatusDesc = "Printer Ready.";
                }
                if ((status[1] & (byte)2) == 2) // Warning Status indicating paper near end situation.
                {
                    PaperNearEnd = true;
                    printerStatus = 2; // warning status.
                    printerStatusDesc = "Printer Ready - Paper Level Low.";
                }
            }
            else if ((bytesRead > 2) && (bytesRead < 255) && (status[0] == 17)) // Status Codes received in case of Extended Status 
            {
                short counter=0;

                if (0 == status[3])
                {
                    printerStatus = 1;
                    printerStatusDesc = "Printer Ready.";
                }
                else
                {
                    printerStatus = 3;
                    printerStatusDesc = "Printer Not Ready -";
                    while (counter < status[3])
                    {
                        if (status[5 + counter] > 128)
                        {
                            switch (status[5 + counter] - (byte)128)
                            {
                                case (byte)ZebraStatusCodes.PAPER_LEFT_IN_PRESENTER:
                                    PresenterJammed = true;
                                    printerStatusDesc += " Presenter Jam, Paper can't be Ejected/Retracted.";
                                    break;
                                case (byte)ZebraStatusCodes.CUTTER_JAMMED:
                                    CutterJammed = true;
                                    printerStatusDesc += " Cutter can't return to home position, Jammed.";
                                    break;
                                case (byte)ZebraStatusCodes.OUT_OF_PAPER:
                                    OutOfPaper = true;
                                    printerStatusDesc += " Printer Out of paper.";
                                    break;
                                case (byte)ZebraStatusCodes.PRINTHEAD_LIFTED:
                                    PrintHeadLifted = true;
                                    printerStatusDesc += " Printer Head Lifted.";
                                    break;
                                case (byte)ZebraStatusCodes.PAPER_FEED_ERROR:
                                    PaperFeedError = true;
                                    printerStatusDesc += " Paper feed error under head.";
                                    break;
                                case (byte)ZebraStatusCodes.TEMPERATURE_ERROR:
                                    TemperatureError = true;
                                    printerStatusDesc += " Print head Temperature is above 60 degree celsius.";
                                    break;
                                case (byte)ZebraStatusCodes.PRESENTER_NOT_RUNNING:
                                    PresenterNotRunning = true;
                                    printerStatusDesc += " Presenter Jam, motor not running.";
                                    break;
                                case (byte)ZebraStatusCodes.PAPER_JAM_DURING_RETRACT:
                                    PaperJam = true;
                                    printerStatusDesc += " Paper Jam during retract operation.";
                                    break;
                                case (byte)ZebraStatusCodes.BLACK_MARK_NOT_FOUND:
                                    BlackMarkNotFound = true;
                                    printerStatusDesc += " Black Mark not found.";
                                    break;
                                case (byte)ZebraStatusCodes.BLACK_MARK_CALIBRATION_ERROR:
                                    BlackMarkCalibrationError = true;
                                    printerStatusDesc += " Black mark calibration error.";
                                    break;
                                case (byte)ZebraStatusCodes.INDEX_ERROR:
                                    IndexError = true;
                                    printerStatusDesc += " Index error.";
                                    break;
                                case (byte)ZebraStatusCodes.CHECKSUM_ERROR:
                                    ChecksumError = true;
                                    printerStatusDesc += " Checksum error.";
                                    break;
                                case (byte)ZebraStatusCodes.WRONG_FIRMWARE:
                                    WrongFirmware = true;
                                    printerStatusDesc += " Wrong firmware type or target for firmware loading.";
                                    break;
                                case (byte)ZebraStatusCodes.FIRMWARE_CANNOT_START:
                                    NoFirmwareOrWrongFirmwareChecksum = true;
                                    printerStatusDesc += " Firmware can't start.";
                                    break;
                                case (byte)ZebraStatusCodes.RETRACT_FUNCTION_TIMED_OUT:
                                    RetractTimedout = true;
                                    printerStatusDesc += " Retract function timed out.";
                                    break;
                                case (byte)ZebraStatusCodes.PAUSED:
                                    Paused = true;
                                    printerStatusDesc += " Paused to avoid overheating of stepper motors.";
                                    break;
                                case (byte)ZebraStatusCodes.UNDEFINED_ERROR:
                                    Undefined = true;
                                    printerStatusDesc += " Undefined Error.";
                                    break;
                            }
                        }
                        else
                        {
                            Undefined = true;
                            printerStatusDesc += " Undefined Error.";
                        }
                        counter++;
                    }
                }
            }
            using (FileStream stream = new FileStream(@"Printer.log", FileMode.Append, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(@"Printer Status : {0} -- State : {1} -- Desc : {2}", printerStatus,printerState,printerStatusDesc);
                writer.Flush();
                writer.Close();
            }
        }



    }
}
