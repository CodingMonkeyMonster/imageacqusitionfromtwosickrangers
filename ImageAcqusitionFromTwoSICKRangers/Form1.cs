using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using HalconDotNet;
using GUI;

namespace ImageAcqusitionFromTwoSICKRangers
{
    public partial class Form1 : Form
    {
        HObject _imageHole1a, _imageHole1Intensitya, _imageHole2a, _imageHole2Intensitya, _imageSideSurfacea, _imageSideSurfaceIntensitya;
        HObject _imageHole1b, _imageHole1Intensityb, _imageHole2b, _imageHole2Intensityb, _imageSideSurfaceb, _imageSideSurfaceIntensityb;
        List<CameraManager> camMan;        

        public Form1()
        {
            InitializeComponent();
        }

        private void initialize_button_Click(object sender, EventArgs e)
        {            
            camMan = new List<CameraManager>();
            camMan.Add(new CameraManager("10.10.11.101", "side"));
            camMan.Add(new CameraManager("10.10.15.105", "side"));            
        }

        private void capture_button_Click(object sender, EventArgs e)
        {
            trig1();
            trig2();
        }

        private void disconnect_button_Click(object sender, EventArgs e)
        {
            foreach (CameraManager cam in camMan)
                cam.closeIcon();            
        }

        private void trig1()
        {
            int i = 0;
            camMan[i].acquireImage();
            _imageHole1a = camMan[i].getHImage(camMan[i].range1);
            _imageHole1Intensitya = camMan[i].getHImage(camMan[i].intensity1);
            _imageHole2a = camMan[i].getHImage(camMan[i].range2);
            _imageHole2Intensitya = camMan[i].getHImage(camMan[i].intensity2);
            _imageSideSurfacea = camMan[i].getHImage(camMan[i].range3);
            _imageSideSurfaceIntensitya = camMan[i].getHImage(camMan[i].intensity3);
        }
        private void trig2()
        {
            int i = 1;
            camMan[i].acquireImage();
            _imageHole1b = camMan[i].getHImage(camMan[i].range1);
            _imageHole1Intensityb = camMan[i].getHImage(camMan[i].intensity1);
            _imageHole2b = camMan[i].getHImage(camMan[i].range2);
            _imageHole2Intensityb = camMan[i].getHImage(camMan[i].intensity2);
            _imageSideSurfaceb = camMan[i].getHImage(camMan[i].range3);
            _imageSideSurfaceIntensityb = camMan[i].getHImage(camMan[i].intensity3);
        }
    }
}
