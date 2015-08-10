using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Zebra;
using MBMEDevices.Printers.Utils;

namespace Zebra
{
    public delegate void ReturnStatus(TTP2030Status pStatus);
    public class TTP2030 : USBDevice
    {
        //private bool readOpPending = false;
        private ushort printerStatus = 0;
        private ReturnStatus pStatus;
        /// <summary>
        /// virtual handler for any action to be taken when data is received. Override to use.
        /// </summary>
        /// <param name="arrReadBuff">The input buffer that was received</param>
        protected override void HandleDataReceived(byte[] arrReadBuff, int bytesRead)
        {
            TTP2030Status objStatus = (TTP2030Status)TTP2030Status.GetInstance();
            if (((arrReadBuff[0] & (byte)128) == (byte)128) && (bytesRead == 2)) // Status Report ENQ 6 - Status Code Available
            {
                GetExtendedStatus(); 
            }
            else// if (bytesRead > 2)
            {
                objStatus.InterpretStatus(arrReadBuff,bytesRead);
            }

            this.pStatus(objStatus);
        }

        /// <summary>
        /// Finds the Epic 950 Printer. 
        /// </summary>
        /// <returns>A new Epic950 printer object or null if not found.</returns>
        public static TTP2030 FindPrinter()
        {
            // VID and PID for EPIC 950 printer are 0x0613 and 0x2213 respectively
            return (TTP2030)FindDevice(0x088c, 0x2030, typeof(TTP2030));
        }

        public short Reset()
        {
            short opCode = -1;
            byte[] reset = { 27, 63 }; // ENQ @
            opCode = this.Write(reset, reset.Length, false, 0);
            return (opCode);
        }

        public short GetExtendedStatus()
        {
            short opCode = -1;
            byte[] extended = { 27, 5, 69 }; // ESC ENQ E
            opCode = this.Write(extended, extended.Length, true, 255);
            return (opCode);
        }

        public short GetStatus(ReturnStatus pStatus)
        {
            short opCode = -1;

            //opCode = Reset();
            //if (opCode != -1)
            //{
                byte[] enq = { 27, 5, 6 }; // Request Combined Printer Status - [GS]y / [29][121] / 1DH 79H
                this.pStatus = pStatus;
                opCode = this.Write(enq, enq.Length, true, 2); // 2 bytes status response is expected from printer.
                //System.Threading.Thread.Sleep(500);
            //}
            return opCode;
        }

        public short PrintBitmap(FileStream fsBMP, int hOffset)
        {
            short opCode = -1;
            //opCode = Reset();
            //SetPrintDirectionInPageMode();
            ////SetAbsoluteHorizontalPosition(hOffset);
            //if (opCode != -1)
            //{
                byte[] bmpContents = new byte[fsBMP.Length + 8];
                fsBMP.Read(bmpContents, 7, (int)fsBMP.Length);
                bmpContents[0] = 27; // ESC
                bmpContents[1] = 98; // b
                bmpContents[2] = 0; // Always 0
                bmpContents[3] = 0; // X Position
                bmpContents[4] = 40; // X Position
                bmpContents[5] = 0; // Y Position
                bmpContents[6] = 0; // Y Position
                bmpContents[fsBMP.Length + 7] = 12; // Cut and Eject
                opCode = Write(bmpContents, bmpContents.Length, false, 0);
                fsBMP.Close();
            //}
            return opCode;
        }
    }
}
