using System;
using System.IO;
using System.Runtime.InteropServices;
using MBMEDevices.Printers.Utils;

namespace Ithaca
{
    public delegate void ReturnStatus(PrinterStatus pStatus);
    public class Epic950 : USBDevice
    {
        //private bool readOpPending = false;
        private ushort printerStatus = 0;
        private ReturnStatus pStatus;
        /// <summary>
        /// virtual handler for any action to be taken when data is received. Override to use.
        /// </summary>
        /// <param name="arrReadBuff">The input buffer that was received</param>
        protected override void HandleDataReceived(byte[] arrReadBuff,int bytesRead)
        {
            byte[] status = new byte[16];
            System.Console.WriteLine("Read " + bytesRead + "status bytes.");
            Epic950Status objStatus = (Epic950Status)Epic950Status.GetInstance();
            if ((arrReadBuff[0] == 29) && (arrReadBuff[1] == 121))
            {
              printerStatus |= arrReadBuff[2]; // [GS] S
              printerStatus |= arrReadBuff[3];
              objStatus.InterpretStatus(arrReadBuff[2], arrReadBuff[3]);
              this.pStatus(objStatus); 
            }
        }

        /// <summary>
        /// Finds the Epic 950 Printer. 
        /// </summary>
        /// <returns>A new Epic950 printer object or null if not found.</returns>
        public static Epic950 FindPrinter()
        {
            // VID and PID for EPIC 950 printer are 0x0613 and 0x2213 respectively
            return (Epic950)FindDevice(0x0613, 0x2213, typeof(Epic950));
        }

        public short Reset()
        {
            short opCode=-1;
            byte[] reset = { 27, 64 }; // ENQ @
            opCode = this.Write(reset,reset.Length,false,0);
            return (opCode);
        }

        public void SetAbsoluteHorizontalPosition(int dots)
        {
            int n1 = dots >> 8;
            int n2 = dots;

            byte[] hOffset = { 27, 36, (byte)(dots >> 8), (byte)dots };
            this.Write(hOffset, hOffset.Length, false, 0);
        }

        public void SetPrintDirectionInPageMode()
        {
            byte[] pDirection = { 27, 116, 48 };
            this.Write(pDirection, pDirection.Length, false, 0);
        }

        public short GetStatus(ReturnStatus pStatus)
        {
            short opCode = 0;
            
            //opCode = Reset();
            if (opCode != -1)
            {
                byte[] enq = { 29, 121 }; // Request Combined Printer Status - [GS]y / [29][121] / 1DH 79H
                this.pStatus = pStatus;
                opCode = this.Write(enq, enq.Length, true, 4); // 4 bytes status response is expected from printer.
                //System.Threading.Thread.Sleep(500);
            }
            return opCode;
        }

        public void WriteText()
        {
            byte[] congratulations_msg =
            {
            27, 64,
            27, 88, 1, 60,			// set horizontal starting position
            27, 89, 0,				// set vertical starting position
            27, 80,				    // 16 cpi font
            14,						// double-width
            67,79,78,71,82,65,84,85,76,65,84,73,79,78,83,10,
            };
            Write(congratulations_msg,congratulations_msg.Length,false,0);
        }

        public short PrintBitmap(FileStream fsBMP, int hOffset)
        {
            short opCode = 0;
            //opCode = Reset();
            //SetPrintDirectionInPageMode();
            ////SetAbsoluteHorizontalPosition(hOffset);
            if (opCode != -1)
            {
                byte[] bmpContents = new byte[fsBMP.Length + 3];
                fsBMP.Read(bmpContents, 1, (int)fsBMP.Length);
                bmpContents[0] = 27; // ESC
                bmpContents[fsBMP.Length + 1] = 27; // ESC E – Form Feed
                bmpContents[fsBMP.Length + 2] = 69;
                opCode = Write(bmpContents, bmpContents.Length, false, 0);
                fsBMP.Close();
            }
            return opCode;
        }

        public short PrintBitmap(MemoryStream fsBMP, int hOffset)
        {
            short opCode = 0;
            //opCode = Reset();
            //SetPrintDirectionInPageMode();
            ////SetAbsoluteHorizontalPosition(hOffset);
            if (opCode != -1)
            {
                byte[] bmpContents = new byte[fsBMP.Length + 3];
                fsBMP.Read(bmpContents, 1, (int)fsBMP.Length);
                bmpContents[0] = 27; // ESC
                bmpContents[fsBMP.Length + 1] = 27; // ESC E – Form Feed
                bmpContents[fsBMP.Length + 2] = 69;
                opCode = Write(bmpContents, bmpContents.Length, false, 0);
                fsBMP.Close();
            }
            return opCode;
        }
    }
}
