using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MBMEDevices.Printers.Utils 
{
    public abstract class  USBDevice : Win32Usb, IDisposable
    {
        #region Privates variables

        /// <summary>Filestream we can use to read/write from</summary>
        private FileStream m_oFile;
        /// <summary>Handle to the device</summary>
        private IntPtr m_hHandle;
        /// <summary>Length of input report : device gives us this</summary>
        private int m_nInputBuffLength;
        
        
        #endregion

        #region Privates/protected
        protected byte[] m_arrInputBuff;
        
        /// <summary>
        /// Initialises the device
        /// </summary>
        /// <param name="strPath">Path to the device</param>
        private void Initialise(string strPath)
        {
            // Create the file from the device path
            m_hHandle = CreateFile(strPath, GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, IntPtr.Zero);
            if (m_hHandle != InvalidHandleValue)	// if the open worked...
            {
                //IntPtr lpData;
                //if (HidD_GetPreparsedData(m_hHandle, out lpData))	// get windows to read the device data into an internal buffer
                //{
                    try
                    {
                        //HidCaps oCaps;
                        //HidP_GetCaps(lpData, out oCaps);	// extract the device capabilities from the internal buffer
                        //m_nInputReportLength = oCaps.InputReportByteLength;	// get the input...
                        //m_nOutputReportLength = oCaps.OutputReportByteLength;	// ... and output report lengths
                        m_oFile = new FileStream(m_hHandle, FileAccess.Read | FileAccess.Write, true, 256, true);	// wrap the file handle in a .Net file stream


                    }
                    catch(Exception ex)
                    {
                        System.Console.WriteLine(ex.Message);
                    }
                //    finally
                //    {
                //        HidD_FreePreparsedData(ref lpData);	// before we quit the funtion, we must free the internal buffer reserved in GetPreparsedData
                //    }
                ////}
                //else	// GetPreparsedData failed? Chuck an exception
                //{
                //    System.Console.WriteLine("GetPreparsedData failed");
                //}
            }
            else	// File open failed? Chuck an exception
            {
                System.Console.WriteLine(Marshal.GetLastWin32Error().ToString());
                m_hHandle = IntPtr.Zero;
                System.Console.WriteLine("Failed to create device file");
            }
        }

        /// <summary>
        /// Kicks off an asynchronous read which completes when data is read or when the device
        /// is disconnected. Uses a callback.
        /// </summary>
        protected short BeginAsyncRead(int respBuffLen)
        {
            short opCode = 0; //Failed
            m_nInputBuffLength = respBuffLen;
            this.m_arrInputBuff = new byte[m_nInputBuffLength];
            // put the buff we used to receive the stuff as the async state then we can get at it when the read completes
            try
            {
                m_oFile.BeginRead(this.m_arrInputBuff, 0, m_nInputBuffLength, new AsyncCallback(ReadCompleted), this.m_arrInputBuff);
                opCode = 1; // Operation successfull.
            }
            catch (Exception ex)	// if we got an IO exception, the device was removed
            {
                System.Console.WriteLine("Read operation initiation resulted into exception" + ex.Message);
                Dispose();
                return (opCode);
            }
            return(opCode);
        }
        /// <summary>
        /// Callback for above. Care with this as it will be called on the background thread from the async read
        /// </summary>
        /// <param name="iResult">Async result parameter</param>
        protected void ReadCompleted(IAsyncResult iResult)
        {
            byte[] arrBuff = (byte[])iResult.AsyncState;	// retrieve the read buffer
            try
            {
                int i = m_oFile.EndRead(iResult);	// call end read : this throws any exceptions that happened during the read
                Console.WriteLine("Length :" + i);
                try
                {
                    HandleDataReceived(arrBuff,i);	// pass the new input report on to the higher level handler
                }
                finally
                {
                    //arrBuff should be deallocated as the buffer is allocated fresh for each read operation.
                }
            }
            catch (IOException ex)	// if we got an IO exception, the device was removed
            {
                //HandleDeviceRemoved();
                System.Console.WriteLine("Read operation completed with an exception" + ex.Message);
                Dispose();
            }
        }

        
        ///// <summary>
        ///// Callback for above. Care with this as it will be called on the background thread from the async write
        ///// </summary>
        ///// <param name="iResult">Async result parameter</param>
        //protected void WriteCompleted(IAsyncResult iResult)
        //{
        //    FileStream fStream = (FileStream)iResult.AsyncState;
        //    try
        //    {
        //        fStream.EndWrite(iResult);	// call end Write : this throws any exceptions that happened during the write
        //        Console.WriteLine("Write Completed Successfully");
        //    }
        //    catch (IOException ex)	// if we got an IO exception, the device was removed
        //    {
        //        //HandleDeviceRemoved();
        //        System.Console.WriteLine("Write operation completed with an exception" + ex.Message);
        //        Dispose();
        //    }
        //}

        ///// <summary>
        ///// Write an output report to the device.
        ///// </summary>
        ///// <param name="oOutRep">Output report to write</param>
        //protected void BeginAsyncWrite(short len)
        //{
        //    try
        //    {
        //        m_oFile.BeginWrite(this.m_arrOutputBuff, 0, len, new AsyncCallback(WriteCompleted), m_oFile);
        //        //m_oFile.Write(oOutRep, 0, len);
        //    }
        //    catch (IOException ex)
        //    {
        //        // The device was removed!
        //        //throw new HIDDeviceException("Device was removed");
        //        System.Console.WriteLine("Write operation initiation resulted into exception" + ex.Message);
        //    }
        //}

        /// <summary>
        /// Write an output buff to the device.
        /// </summary>
        /// <param name="wBuff">Output buffer to write.</param>
        /// <param name="len">length of buffer.</param>
        /// <param name="wBuff">readFlag true if response expected.</param>
        protected short Write(byte[] wBuff,int len,bool readFlag,short expectedRespLen)
        {
            short opCode = 0;
            try
            {
                m_oFile.Write(wBuff, 0, len);
                if (readFlag)
                  opCode = BeginAsyncRead(expectedRespLen);
            }
            catch (IOException ex)
            {
                // The device was removed!
                //throw new HIDDeviceException("Device was removed");
                System.Console.WriteLine("Write opearation resulted into an exception" + ex.Message);
                Dispose();
            }

            return opCode;
        }
        /// <summary>
        /// virtual handler for any action to be taken when data is received. Override to use.
        /// </summary>
        /// <param name="oInRep">The input report that was received</param>
        protected virtual void HandleDataReceived(byte[] arrReadBuff, int bytesRead)
        {
        }
        /// <summary>
        /// Virtual handler for any action to be taken when a device is removed. Override to use.
        /// </summary>
        protected virtual void HandleDeviceRemoved()
        {
        }
        #endregion

        #region Private static
        /// <summary>
	    /// Helper method to return the device path given a DeviceInterfaceData structure and an InfoSet handle.
	    /// Used in 'FindDevice' so check that method out to see how to get an InfoSet handle and a DeviceInterfaceData.
	    /// </summary>
	    /// <param name="hInfoSet">Handle to the InfoSet</param>
	    /// <param name="oInterface">DeviceInterfaceData structure</param>
	    /// <returns>The device path or null if there was some problem</returns>
	    private static string GetDevicePath(IntPtr hInfoSet, ref DeviceInterfaceData oInterface)
	    {
		    uint nRequiredSize = 0;
		    // Get the device interface details
		    if (!SetupDiGetDeviceInterfaceDetail(hInfoSet, ref oInterface, IntPtr.Zero, 0, ref nRequiredSize, IntPtr.Zero))
		    {
			    DeviceInterfaceDetailData oDetail = new DeviceInterfaceDetailData();
			    oDetail.Size = 5;	// hardcoded to 5! Sorry, but this works and trying more future proof versions by setting the size to the struct sizeof failed miserably. If you manage to sort it, mail me! Thx
			    if (SetupDiGetDeviceInterfaceDetail(hInfoSet, ref oInterface, ref oDetail, nRequiredSize, ref nRequiredSize, IntPtr.Zero))
			    {
				    return oDetail.DevicePath;
			    }
		    }
		    return null;
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Disposer called by both dispose and finalise
        /// </summary>
        /// <param name="bDisposing">True if disposing</param>
        protected virtual void Dispose(bool bDisposing)
        {
            if (bDisposing)	// if we are disposing, need to close the managed resources
            {
                if (m_oFile != null)
                {
                    try
                    {
                        m_oFile.Close();
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.Message);
                        m_oFile = null;
                    }
                    m_oFile = null;
                }
            }
            if (m_hHandle != IntPtr.Zero)	// Dispose and finalize, get rid of unmanaged resources
            {
                try
                {
                    CloseHandle(m_hHandle);
                }
                catch (Exception ex)
                {
                    m_hHandle = IntPtr.Zero;
                    Trace.TraceError(ex.Message);
                }
            }
        }
        #endregion


        #region Public static
        /// <summary>
	    /// Finds a device given its PID and VID
	    /// </summary>
	    /// <param name="nVid">Vendor id for device (VID)</param>
	    /// <param name="nPid">Product id for device (PID)</param>
	    /// <param name="oType">Type of device class to create</param>
	    /// <returns>A new device class of the given type or null</returns>
	    public static USBDevice FindDevice(int nVid, int nPid, Type oType)
        {
            string strPath = string.Empty;
		    string strSearch = string.Format("vid_{0:x4}&pid_{1:x4}", nVid, nPid); // first, build the path search string
            Guid gHid = new Guid("{0x28d78fad,0x5a12,0x11D1,{0xae,0x5b,0x00,0x00,0xf8,0x03,0xa8,0xc2}}");
            
            //HidD_GetHidGuid(out gHid);	// next, get the GUID from Windows that it uses to represent the HID USB interface
            IntPtr hInfoSet = SetupDiGetClassDevs(ref gHid, null, IntPtr.Zero, DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);// this gets a list of all HID devices currently connected to the computer (InfoSet)
            try
            {
                DeviceInterfaceData oInterface = new DeviceInterfaceData();	// build up a device interface data block
                oInterface.Size = Marshal.SizeOf(oInterface);
			    // Now iterate through the InfoSet memory block assigned within Windows in the call to SetupDiGetClassDevs
			    // to get device details for each device connected
			    int nIndex = 0;
                System.Console.WriteLine(gHid.ToString());
                System.Console.WriteLine(hInfoSet.ToString());
                Boolean Result = SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref gHid, (uint)nIndex, ref oInterface);
                System.Console.WriteLine(Result.ToString());
                while (SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref gHid, (uint)nIndex, ref oInterface))	// this gets the device interface information for a device at index 'nIndex' in the memory block
                    {
                        string strDevicePath = GetDevicePath(hInfoSet, ref oInterface);	// get the device path (see helper method 'GetDevicePath')
                        if (strDevicePath.IndexOf(strSearch) >= 0)	// do a string search, if we find the VID/PID string then we found our device!
                        {
                            USBDevice oNewDevice = (USBDevice)Activator.CreateInstance(oType);	// create an instance of the class for this device
                            //USBPRINT\VID_088c&PID_2030
                            oNewDevice.Initialise(strDevicePath);	// strDevicePath initialise it with the device path
                            return oNewDevice;	// and return it
                        }
                        nIndex++;	// if we get here, we didn't find our device. So move on to the next one.
                    }
                    System.Console.WriteLine(Marshal.GetLastWin32Error().ToString());
            }
            finally
            {
			    // Before we go, we have to free up the InfoSet memory reserved by SetupDiGetClassDevs
                SetupDiDestroyDeviceInfoList(hInfoSet);
            }
            return null;	// oops, didn't find our device
        }
	    #endregion

    }
}