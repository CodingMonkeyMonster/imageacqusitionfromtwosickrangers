using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Sick.Icon;
using HalconDotNet;

namespace GUI
{
    class CameraManager
    {
        string _cameraType = "";

        public byte[,] intensity1, intensity2, intensity3;
        public ushort[,] range1, range2, range3;

        ICameraSystem camSystem = IconApi.Factory.CreateInstance<ICameraSystem>();

        Sick.Icon.IIconBuffer buffer1 = null, buffer2 = null;
        Sick.Icon.IGrabStatus status = null;

        static void MyErrorHandler(int errLevel, string errString)
        {
            Console.WriteLine("{0}: {1}", errLevel, errString);
        }
        static void MyStateHandler(IState state)
        {
            // Could put in a useful statechange handler if desired.
            //Console.Out.WriteLine("Camera 1. State: {0}", state.ToString());
        }

        public CameraManager(string ipAddress, string cameraType)
        {
            _cameraType = cameraType;
            try
            {
                Sick.Icon.IconApi.Utility.OpenLogFile("example.log");
                Sick.Icon.IconApi.Utility.SetErrorEventHandler(MyErrorHandler);

                camSystem.Init(CameraSystemType.ETHERNET_CAMERA, "MyCamera1");

                /* Add event handlers here */
                camSystem.StateChanged += MyStateHandler;

                /* Make sure to connect to the correct camera */
                camSystem.SetParameter("", "camera IP address", ipAddress);

                /* Set some parameters to make sure we don't
                 * timeout and use the appropriate mode
                 * 
                 * These were the ones mentioned in the old support thread
                 * all parameters can be set this way, but it is faster to
                 * import a parameter (.prm) or configuration (.icx) file */
                camSystem.SetParameter("", "frame grabber memory", "250");
                camSystem.SetParameter("", "timeout", "30000");
                camSystem.SetParameter("", "buffer callbacks enabled", "false");
                camSystem.SetParameter("", "image mode enabled", "false");

                /* Connect */
                camSystem.Connect();

                /* After connection we can import the parameter file */
                switch (cameraType)
                {
                    case "bottom":
                        camSystem.ImportCameraParameterFile("C:\\402-13\\Shared\\prm files\\Parameter FIle Bottom Inspection (just 1 3D inspection faster) v5.prm");
                        break;
                    case "side":
                        camSystem.ImportCameraParameterFile("../../../Parameter File Side Inspection (three fast multiscan 3D inspection) v15.prm");
                        break;
                    default:
                        break;
                }

                /* Start acquiring
                * The camera is now running and capturing data
                * later pick out the buffers to read using grab */
                camSystem.Start(); 
            }
            catch (IconException e)
            {
                Console.WriteLine("Print somewhere");
            }
        }

        public void closeIcon()
        {
            // stop in the name of love
            camSystem.Stop();
            camSystem.Disconnect();
            Sick.Icon.IconApi.Utility.CloseLogFile();
            Sick.Icon.IconApi.CloseApi();
        }

        /// <summary>
        /// Delegate defined to enabled asyncronous grabbing.  Return type is for returning errorcode 
        /// </summary>
        /// <returns></returns>
        public delegate Sick.Icon.Result ConnectDelegateType();

        /// <summary>
        /// Although I should have some sort of error tracking, I have eliminated the prompt
        /// so that the error is caught amd handled more easily in higher levels without stopping the program with a 
        /// prompt.  What you need to do is come up with a way of carrying errors up the chain and ultimatley to
        /// some sort of error logger.
        /// </summary>
        public void acquireImage()
        {
            try
            {
                /* Do grab with a high timeout to make sure we are
                 * not stopping before all profiles have been captured 
                 * 
                 * Maybe do some threading to improve performance */
                ///// FIND OUT HOW TO MAKE THIS ASYNCHRONOUS
                //camSystem.Start();
                camSystem.Grab(out buffer1, out status, 100000);
                //camSystem.Grab(out buffer2, out status, 100000);                            
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);
            }

            /* Check that all is ok with the buffer.
             * You may choose how to handle each of these possible
             * error states. For now we just print the information. */
            if (status.AllOK() != true)
            {
                if (status.GetOverflowStatus())
                    Console.Out.WriteLine("\nBuffer overflow occurred.\n");

                if (status.GetOvertrigs() != 0)
                    Console.Out.WriteLine("Camera reported {0} overtrigs in the buffer.", status.GetOvertrigs());

                if (status.GetScansLost() != 0)
                    Console.Out.WriteLine("Lost {0} scans in the buffer.", status.GetScansLost());
            }

            /* We need to do some processing to get the data into
             * an Halcon image.*/

            switch (_cameraType)
            {
                case "bottom":
                    intensity1 = buffer1.Components["Hi3D 1"].SubComponents["Intensity"].GetRows<byte>(0, buffer1.Height);
                    range1 = buffer1.Components["Hi3D 1"].SubComponents["Range"].GetRows<ushort>(0, buffer1.Height);
                    break;
                case "side":
                    intensity1 = buffer1.Components["Hi3D 1"].SubComponents["Intensity"].GetRows<byte>(0, buffer1.Height);
                    range1 = buffer1.Components["Hi3D 1"].SubComponents["Range"].GetRows<ushort>(0, buffer1.Height);
                    intensity2 = buffer1.Components["Hi3D 2"].SubComponents["Intensity"].GetRows<byte>(0, buffer1.Height);
                    range2 = buffer1.Components["Hi3D 2"].SubComponents["Range"].GetRows<ushort>(0, buffer1.Height);
                    intensity3 = buffer1.Components["Hi3D 3"].SubComponents["Intensity"].GetRows<byte>(0, buffer1.Height);
                    range3 = buffer1.Components["Hi3D 3"].SubComponents["Range"].GetRows<ushort>(0, buffer1.Height);
                    break;
                default:
                    break;
            }

            /* Make sure to release the buffer back to iCon. 
            * This could be done as soon as you are done using buffer. */
            camSystem.Release();
        }

        public HImage getHImage(byte[,] byteArray)
        {
            int width1 = 0;
            int height1 = 0;

            width1 = byteArray.GetLength(1);
            height1 = byteArray.GetLength(0);
            int imageSizeInBytes1 = Buffer.ByteLength(byteArray);


            /* From iCon we get a 2D array, but Halcon requires a 1D array.
             * Here we do our first copy operation to a 1D buffer of the
             * generic byte type. */
            HImage _hImage = new HImage();
            byte[] data1D1 = new byte[imageSizeInBytes1];
            Buffer.BlockCopy(byteArray, 0, data1D1, 0, imageSizeInBytes1);

            // Now we make sure it does not move by binding it with a GCHandle
            GCHandle handle1D = GCHandle.Alloc(data1D1, GCHandleType.Pinned);

            IntPtr pData = handle1D.AddrOfPinnedObject();

            /* I believe this is how the GenImage1 should be used:
             * - Non calibrated Range = "uint2"
             * - Calibrated Range = "int4"
             * - Normal Intensity = "byte"
             * - COG Intensity = "uint2"    */
            _hImage.GenImage1("byte", width1, height1, pData);

            _hImage.WriteImage("tiff", 0, "testing.tif");


            // Release the handle so that the garbage collector can do its work
            handle1D.Free();

            return _hImage;
        }

        public HImage getHImage(ushort[,] byteArray)
        {
            int width1 = 0;
            int height1 = 0;

            width1 = byteArray.GetLength(1);
            height1 = byteArray.GetLength(0);
            int imageSizeInBytes1 = Buffer.ByteLength(byteArray);

            /* From iCon we get a 2D array, but Halcon requires a 1D array.
             * Here we do our first copy operation to a 1D buffer of the
             * generic byte type. */
            HImage _hImage = new HImage();
            byte[] data1D1 = new byte[imageSizeInBytes1];
            Buffer.BlockCopy(byteArray, 0, data1D1, 0, imageSizeInBytes1);

            // Now we make sure it does not move by binding it with a GCHandle
            GCHandle handle1D = GCHandle.Alloc(data1D1, GCHandleType.Pinned);

            IntPtr pData = handle1D.AddrOfPinnedObject();

            /* I believe this is how the GenImage1 should be used:
             * - Non calibrated Range = "uint2"
             * - Calibrated Range = "int4"
             * - Normal Intensity = "byte"
             * - COG Intensity = "uint2"    */
            _hImage.GenImage1("uint2", width1, height1, pData);

            _hImage.WriteImage("tiff", 0, "testing2.tif");

            // Release the handle so that the garbage collector can do its work
            handle1D.Free();

            return _hImage;
        }

        private HImage generateImage(int w, int h, int size, string dataType)
        {
            /* From iCon we get a 2D array, but Halcon requires a 1D array.
             * Here we do our first copy operation to a 1D buffer of the
             * generic byte type. */
            HImage _hImage = new HImage();
            byte[] data1D1 = new byte[size];
            Buffer.BlockCopy(intensity1, 0, data1D1, 0, size);

            // Now we make sure it does not move by binding it with a GCHandle
            GCHandle handle1D = GCHandle.Alloc(data1D1, GCHandleType.Pinned);

            IntPtr pData = handle1D.AddrOfPinnedObject();

            /* I believe this is how the GenImage1 should be used:
             * - Non calibrated Range = "uint2"
             * - Calibrated Range = "int4"
             * - Normal Intensity = "byte"
             * - COG Intensity = "uint2"    */
            _hImage.GenImage1(dataType, w, h, pData);

            // Release the handle so that the garbage collector can do its work
            handle1D.Free();

            return _hImage;

        }

    }
}
