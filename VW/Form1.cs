using Emgu.CV;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Timers;

using AForge.Video;
using AForge.Video.DirectShow;
using Emgu.CV.Structure;

namespace VW
{
    public partial class Form1 : Form
    {
        Timer timer = new Timer
        {
            Interval = 40
        };

        double x_position = 0;
        double y_position = 0;
        double distance = 0;
        string textBox_string = "";
        double x_capture = 0;
        double y_capture = 0;

        VideoCapture capture;
        //capture.FlipVertical.get=true;
        

        public Form1()
        {
            InitializeComponent();
            textBox_string = "x position = "+ x_position.ToString() + "\r\ny position = "+y_position.ToString() + "\r\ndistance = "+ distance.ToString();
            textBox1.Text = textBox_string;            
            Run();
            
        }

        //CAPTURE IMAGE FROM DEVICE
        private void Run()
        {
            try
            {
                capture = new VideoCapture();
                x_capture = capture.Width;
                y_capture = capture.Height;
                capture.FlipHorizontal = true;
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
                return;
            }
            Application.Idle += ProcessFrame;
        }

        //SHOW CAPTURED IMAGE
        private void ProcessFrame(object sender, EventArgs e)
        {
            imageBox1.Image = capture.QueryFrame();
        }

        
        //VideoCaptureDevice device;

        //CREATE LIST OF AVAIABLE DEVICES
        private void Form1_Load(object sender, EventArgs e)
        {
            FilterInfoCollection filter;
            filter = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in filter)
                comboBox1.Items.Add(device.Name);
            try
            {
                comboBox1.SelectedIndex = 0;
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Cannot find any video device");
                comboBox1.SelectedItem = null;
                return;
            }
            
            //device = new VideoCaptureDevice();
        }

        //CHANGE ACTIVE VIDEO DEVICE
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            capture = new VideoCapture(comboBox1.SelectedIndex);
            x_capture = capture.Width;
            y_capture = capture.Height;
            capture.FlipHorizontal = true;
        }


        //FACE DETECTION
        static readonly CascadeClassifier cascadeClassfier = new CascadeClassifier("haarcascade_frontalface_alt_tree.xml");
        void detec_face() 
        {

            Mat frame = new Mat();
            capture.Retrieve(frame, 0);
            //imageBox1.Image = frame;
            Bitmap bitmap = new Bitmap(frame.Bitmap);
            Image<Bgr, byte> grayImage = new Image<Bgr, byte>(bitmap);
            Rectangle[] rectangles = cascadeClassfier.DetectMultiScale(grayImage, 1.2, 1);
            
            //Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height);
            //Graphics background = Graphics.FromImage(bitmap2);
            //bitmap2.MakeTransparent();
            //bitmap = bitmap2;

            foreach (Rectangle rectangle in rectangles)
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    using (Pen pen = new Pen(Color.Red, 1))
                    {
                        graphics.DrawRectangle(pen, rectangle);
                    }
                }
            }
            imageBox1.Image= new Image<Bgr, Byte>(bitmap);

            if (rectangles != null && rectangles.Length != 0)
            {
                x_position = rectangles[0].X+ rectangles[0].Width/2-x_capture/2;
                y_position =-(rectangles[0].Y+ rectangles[0].Height/2-y_capture/2);
                distance = (100000-(rectangles[0].Width * rectangles[0].Height))/700;
            }
            textBox_string = "x position = " + x_position.ToString() + "\r\ny position = " + y_position.ToString() + "\r\ndistance = " + distance.ToString();
            textBox1.Text = textBox_string;

        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
            detec_face();
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    detec_face();
        //}

        private void button1_Click_1(object sender, EventArgs e)
        {
            timer.Enabled = true;
            timer.Tick += new System.EventHandler(OnTimerEvent);
            timer.Start();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }


        //private void Form1_Load(object sender, EventArgs e)
        //{
        //
        //}
    
}
