

// ---------------------------------------------------------------
// Date          220509
// Author        BBIM
// Version       Visual Studio 2008 Pro SP1 (C#, WPF).
// Framework     3.5.
// Module name   Mod_XAMLToBitmap
// Purpose       XAML to Bitmap converter.
// Comments      WPF module.
// ---------------------------------------------------------------

// History.
// ---------------------------------------------------------------
// Source: D:\PC3000_NET_WPF_35\WPF_Test\Test_XAMLToBitmap\XAMLToBitmap_05.
// 17-05-09 - Mod_XAMLToBitmap_06 - New class Mod_XAMLToBitmap_06.
// 21-05-09 - Mod_XAMLToBitmap_07 - Basic functions also made public. There are now 5 overloaded fuctions.
// 22-05-09 - Mod_XAMLToBitmap_08 - In XAMLToBitmap() versions with FrameworkElement as parameter,
//                                  the parameter is cloned first to prevent arranging of the 
//                                  element in the UI. CloneXaml() added.
// ---------------------------------------------------------------


// --- Converter -------------------------------
// public bool                XAMLToBitmap(file, file, elementname)         ' Convert a XAML element to bitmap and save as PNG file.
// public bool                XAMLToBitmap(file, file, elementname, dpi)    ' Convert a XAML element to bitmap and save as PNG file.
// public RenderTargetBitmap  XAMLToBitmap(FrameworkElement)	            ' Convert a XAML element to bitmap.
// public RenderTargetBitmap  XAMLToBitmap(FrameworkElement, file)	        ' Convert a XAML element to bitmap and save.
// public RenderTargetBitmap  XAMLToBitmap(FrameworkElement, file, dpi)	    ' Convert a XAML element to bitmap and save, set dpi's.
// public bool                ObjectToFile()                                ' Save an object to file as XAML.
// --- private support -------------------------
// private FrameworkElement   GetRootElement()	                            ' Get XAML root element from file.
// private FrameworkElement   GetItemElement()	                            ' Get a child element with a name.
// private Object             CloneXaml()                                   ' Clone a XAML object (disconnect).
// ---------------------------------------------


using System;
using System.IO;                    // File, Directory.
using System.Windows;
using System.Windows.Markup;        // XamlReader, XamlWriter.
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MBMEDevices.Printers.Utils
{


    class Mod_XAMLToBitmap_08
    {

        // Constructor.
        public Mod_XAMLToBitmap_08()
        {
            // Inits.
            // ...
        }


        // === Converter ========================================================================


        // ---------------------------------------------------------------
        // Date      220509
        // Purpose   Convert a XAML element to bitmap and save as PNG file.
        // Entry     sFileName_xam - The XAML file from which to convert the element (.xaml).
        //           sFileName_png - Filename for the converted XAML element (.png). 
        //           sElementName - The Name of the XAML element to convert.
        // Return    True if successful.
        // Comments  1) Uses an overloaded version of this function.
        //           2) The XAML image to convert is usuallay on a Canvas. Use the Name of the
        //           Canvas element for sElementName.
        //           3) The bitmap is returned with 96dpi.
        //           4) Overloaded.
        // ---------------------------------------------------------------
        public bool XAMLToBitmap(string sFileName_xam, string sFileName_png, string sElementName)
        {
            if (File.Exists(sFileName_xam) == false)
            {
                return false;
            }

            FileInfo fi = new FileInfo(sFileName_png);
            if (Directory.Exists(fi.DirectoryName) == false)
            {
                return false;
            }

            try
            {
                FrameworkElement oRootElement = GetRootElement(sFileName_xam);
                FrameworkElement oItemElement = GetItemElement(oRootElement, sElementName);
                RenderTargetBitmap rtb = XAMLToBitmap(oItemElement, sFileName_png);
                if (rtb != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }


        // ---------------------------------------------------------------
        // Date      220509
        // Purpose   Convert a XAML element to bitmap and save as PNG file.
        // Entry     sFileName_xam - The XAML file from which to convert the element (.xaml).
        //           sFileName_png - Filename for the converted XAML element (.png). 
        //           sElementName - The Name of the XAML element to convert.
        //           dpi - The resolution for the bitmap in dots per inch.
        // Return    True if successful.
        // Comments  1) Uses an overloaded version of this function.
        //           2) The XAML image to convert is usuallay on a Canvas. Use the Name of the
        //           Canvas element for sElementName.
        //           3) The bitmap is returned with 96dpi.
        //           4) Overloaded.
        // ---------------------------------------------------------------
        public bool XAMLToBitmap(string sFileName_xam, string sFileName_png, string sElementName, double dpi)
        {
            if (File.Exists(sFileName_xam) == false)
            {
                return false;
            }

            FileInfo fi = new FileInfo(sFileName_png);
            if (Directory.Exists(fi.DirectoryName) == false)
            {
                return false;
            }

            try
            {
                FrameworkElement oRootElement = GetRootElement(sFileName_xam);
                //FrameworkElement oItemElement = GetItemElement(oRootElement, sElementName);
                RenderTargetBitmap rtb = XAMLToBitmap(oRootElement, sFileName_png, dpi);
                if (rtb != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }


        // ---------------------------------------------------------------
        // Date      220509
        // Purpose   Convert a XAML element to bitmap.
        // Entry     oElement - The XAML element to convert. 
        // Return    The bitmap from the XAML element.
        // Comments  1) Width and Height of the element must be non-negative,
        //           the element must have a With and Height property.
        //           2) The bitmap is returned with 96dpi.
        //           3) Overloaded.
        // ---------------------------------------------------------------
        public RenderTargetBitmap XAMLToBitmap(FrameworkElement oElement)
        {
            try
            {
                // Make a copy of the element (prevent arranging of source).
                FrameworkElement oE = (FrameworkElement)CloneXaml(oElement);

                // Get PixelWidth, PixelHeight.
                oE.Measure(new Size((int)oE.Width, (int)oE.Height));
                oE.Arrange(new Rect(new Size((int)oE.Width, (int)oE.Height)));

                // Convert to bitmap.
                RenderTargetBitmap rtb =
                    new RenderTargetBitmap((int)oE.ActualWidth,
                                           (int)oE.ActualHeight, 96d, 96d,
                                           PixelFormats.Pbgra32);
                rtb.Render(oE);
                return rtb;
            }
            catch
            {
                return null;
            }
        }


        // ---------------------------------------------------------------
        // Date      220509
        // Purpose   Convert a XAML element to bitmap and save to file.
        // Entry     oElement - The XAML element to convert. 
        //           sFileName_png - The name of the image file to save (.png).
        // Return    The bitmap from the XAML element.
        // Comments  1) Width and Height of the element must be non-negative,
        //           the element must have a With and Height property.
        //           2) The bitmap is returned with 96dpi.
        //           3) Overloaded.
        // ---------------------------------------------------------------
        public RenderTargetBitmap XAMLToBitmap(FrameworkElement oElement, string sFileName_png)
        {
            try
            {
                // Make a copy of the element (prevent arranging of source).
                FrameworkElement oE = (FrameworkElement)CloneXaml(oElement);

                // Get PixelWidth, PixelHeight.
                oE.Measure(new Size((int)oE.Width, (int)oE.Height));
                oE.Arrange(new Rect(new Size((int)oE.Width, (int)oE.Height)));

                // Convert to bitmap.
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)oE.ActualWidth,
                                         (int)oE.ActualHeight, 96d, 96d,
                                         PixelFormats.Pbgra32);
                rtb.Render(oE);

                if (File.Exists(sFileName_png) == true)
                {
                    File.SetAttributes(sFileName_png, FileAttributes.Normal);
                }
                // Force extension to .png.
                sFileName_png = System.IO.Path.ChangeExtension(sFileName_png, ".png");
                // Save to file.
                using (FileStream fs = new FileStream(sFileName_png, FileMode.Create))
                {
                    PngBitmapEncoder enc = new PngBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(rtb));
                    enc.Save(fs);
                }
                return rtb;
            }
            catch
            {
                return null;
            }
        }


        // ---------------------------------------------------------------
        // Date      220509
        // Purpose   Convert a XAML element to bitmap and save to file.
        // Entry     oElement - The XAML element to convert. 
        //           sFileName_png - The name of the image file to save (.png).
        //           DPI - The dpi for the bitmap to create.
        // Return    The bitmap from the XAML element.
        // Comments  1) Width and Height of the element must be non-negative,
        //           the element must have a With and Height property.
        //           2) 96dpi for bitmap is considered an image on scale 1:1.
        //           3) Overloaded.
        // ---------------------------------------------------------------
        public RenderTargetBitmap XAMLToBitmap(FrameworkElement oElement,
                                               string sFileName_png,
                                               double dpi)
        {
            if (dpi < 96d) 
            {
                dpi = 96d;
            }
            try
            {
                // Make a copy of the element (prevent arranging of source).
                FrameworkElement oE = (FrameworkElement)CloneXaml(oElement);

                // Get PixelWidth, PixelHeight.
                oE.Measure(new Size((int)oE.Width, (int)oE.Height));
                oE.Arrange(new Rect(new Size((int)oE.Width, (int)oE.Height)));

                RenderTargetBitmap rtb = null;

                // Convert to bitmap.
                int iW = (int)((dpi / 96d) * (oE.ActualWidth));
                int iH = (int)((dpi / 96d) * (oE.ActualHeight));
                rtb = new RenderTargetBitmap(iW, iH, dpi, dpi, PixelFormats.Pbgra32);
                rtb.Render(oE);
                if (File.Exists(sFileName_png) == true)
                {
                    File.SetAttributes(sFileName_png, FileAttributes.Normal);
                }
                // Force extension to .png.
                //sFileName_png = System.IO.Path.ChangeExtension(sFileName_png, ".png");
                sFileName_png = System.IO.Path.ChangeExtension(sFileName_png, ".bmp");
                // Save to file.
                using (FileStream fs = new FileStream(sFileName_png, FileMode.Create))
                {
                    //PngBitmapEncoder enc = new PngBitmapEncoder();
                    BmpBitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(rtb));
                    //enc.Frames.Add(BitmapFrame.Create(bmp));
                    enc.Save(fs);
                }
                return rtb;
            }
            catch(Exception ex)
            {
                System.Console.WriteLine("Exception :" + ex.InnerException);
                return null;
            }
        }


        // ---------------------------------------------------------------
        // Date      190209
        // Purpose   Save an object to file as XAML.
        // Entry     xamlObject - The System.Windows.Controls object to save.
        //           sFileName_xaml - The filename.
        // Return    True if successful.
        // Comments  1) Overwrites an existing file.
        //           2) For testing purposes.
        // ---------------------------------------------------------------
        public bool ObjectToFile(Object xamlObject, string sFileName_xaml)
        {
            try
            {
                using (FileStream fs = new FileStream(sFileName_xaml, FileMode.Create))
                {
                    XamlWriter.Save(xamlObject, fs);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public RenderTargetBitmap XAMLToBitmap(FrameworkElement oElement, double dpi)
        {
            if (dpi < 96d)
            {
                dpi = 96d;
            }
            try
            {
                // Make a copy of the element (prevent arranging of source).
                FrameworkElement oE = (FrameworkElement)CloneXaml(oElement);

                // Get PixelWidth, PixelHeight.
                oE.Measure(new Size((int)oE.Width, (int)oE.Height));
                oE.Arrange(new Rect(new Size((int)oE.Width, (int)oE.Height)));

                RenderTargetBitmap rtb = null;

                // Convert to bitmap.
                int iW = (int)((dpi / 96d) * (oE.ActualWidth));
                int iH = (int)((dpi / 96d) * (oE.ActualHeight));
                rtb = new RenderTargetBitmap(iW, iH, dpi, dpi, PixelFormats.Pbgra32);
                rtb.Render(oE);

                ////using (MemoryStream ms = new MemoryStream())
                ////{
                ////    BmpBitmapEncoder enc = new BmpBitmapEncoder();
                ////    enc.Frames.Add(BitmapFrame.Create(rtb));
                ////    enc.Save(ms);
                ////}
                return rtb;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception :" + ex.InnerException);
                return null;
            }
        }

        // === end Converter =================================================================
        // === private support ===============================================================



        // OK 140509
        // ---------------------------------------------------------------
        // Date      190209
        // Purpose   Get XAML root element from file.
        // Entry     sFileName_xaml - The filename.
        // Return    The element.
        // Comments  Returns the root element from the file.
        // ---------------------------------------------------------------
        private FrameworkElement GetRootElement(string sFileName_xaml)
        {
            try
            {
                using (FileStream fs = new FileStream(sFileName_xaml, FileMode.Open))
                {
                    return (FrameworkElement)XamlReader.Load(fs);
                }
            }
            catch
            {
                return null;
            }
        }


        // OK 140509
        // ---------------------------------------------------------------
        // Date      190209
        // Purpose   Get a child element with a name.
        // Entry     oElement - Parent (root).
        //           sElementName - The name of the child
        // Return    The child element with the name.
        // Comments  Example (Page is root, Canvas is child): 
        //           <Page>
        //              <Canvas x:Name="CNV_00" Width="240" Height="240">
        //              ...
        //              </Canvas>
        //           </Page>
        // ---------------------------------------------------------------
        private FrameworkElement GetItemElement(FrameworkElement oElement, string sElementName)
        {
            return (FrameworkElement)oElement.FindName(sElementName);
        }


        // ---------------------------------------------------------------
        // Date      090409
        // Purpose   Clone a XAML object (disconnect).
        // Entry     xamlObject - The object to clone.
        // Return    A disconnected copy of the object.
        // Comments  1) To prevent:
        //           "Specified element is already the logical child of 
        //           another element. Disconnect it first."
        //           2) Use:
        //           Ellipse oE1 = new Ellipse;
        //           Ellipse oE2 = new Ellipse;
        //           ...
        //           oE2 = (Ellipse)CloneXaml(oE1);
        // ---------------------------------------------------------------
        private Object CloneXaml(Object xamlObject)
        {
            try
            {
                // Write to string.
                string sXaml = XamlWriter.Save(xamlObject);
                // Read from string.
                return XamlReader.Parse(sXaml);
            }
            catch
            {
                return null;
            }
        }

        // === end private support ==============================================================


    }   // end class
}       // end namespace
