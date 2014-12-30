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
        HObject _imageHole1, _imageHole1Intensity, _imageHole2, _imageHole2Intensity, _imageSideSurface, _imageSideSurfaceIntensity;
        CameraManager camMan;

        public Form1()
        {
            InitializeComponent();
        }

        private void initialize_button_Click(object sender, EventArgs e)
        {
            camMan = new CameraManager("10.10.11.101", "side");

        }

        private void capture_button_Click(object sender, EventArgs e)
        {
            camMan.acquireImage();
            _imageHole1 = camMan.getHImage(camMan.range1);
            _imageHole1Intensity = camMan.getHImage(camMan.intensity1);
            _imageHole2 = camMan.getHImage(camMan.range2);
            _imageHole2Intensity = camMan.getHImage(camMan.intensity2);
            _imageSideSurface = camMan.getHImage(camMan.range3);
            _imageSideSurfaceIntensity = camMan.getHImage(camMan.intensity3);
        }

        private void disconnect_button_Click(object sender, EventArgs e)
        {
            camMan.closeIcon();
        }
    }
}
