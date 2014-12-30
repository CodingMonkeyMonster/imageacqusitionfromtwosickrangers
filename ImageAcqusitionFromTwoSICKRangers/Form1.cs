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
        CameraManager camMan;
        CameraManager camMan2;

        public Form1()
        {
            InitializeComponent();
        }

        private void initialize_button_Click(object sender, EventArgs e)
        {
            camMan = new CameraManager("10.10.11.101", "side");
            camMan2 = new CameraManager("10.10.15.105", "side");
        }

        private void capture_button_Click(object sender, EventArgs e)
        {
            trig1();
            trig2();
        }

        private void disconnect_button_Click(object sender, EventArgs e)
        {
            camMan.closeIcon();
        }

        private void trig1()
        {
            camMan.acquireImage();
            _imageHole1a = camMan.getHImage(camMan.range1);
            _imageHole1Intensitya = camMan.getHImage(camMan.intensity1);
            _imageHole2a = camMan.getHImage(camMan.range2);
            _imageHole2Intensitya = camMan.getHImage(camMan.intensity2);
            _imageSideSurfacea = camMan.getHImage(camMan.range3);
            _imageSideSurfaceIntensitya = camMan.getHImage(camMan.intensity3);
        }
        private void trig2()
        {
            camMan2.acquireImage();
            _imageHole1b = camMan.getHImage(camMan.range1);
            _imageHole1Intensityb = camMan.getHImage(camMan.intensity1);
            _imageHole2b = camMan.getHImage(camMan.range2);
            _imageHole2Intensityb = camMan.getHImage(camMan.intensity2);
            _imageSideSurfaceb = camMan.getHImage(camMan.range3);
            _imageSideSurfaceIntensityb = camMan.getHImage(camMan.intensity3);
        }
    }
}
