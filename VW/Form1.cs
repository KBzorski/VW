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

        int[] big_rect = new int[2];
        double[,] position_history = new double[3,2];
        bool historyY = false;
        bool historyX = false;
        bool historyZ = false;

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

        //ADJUST CURRENT POSITION
        double get_face_positionX(double X)
        {
            if (historyX == false)
            {
                position_history[0, 0] = X;
                position_history[0, 1] = X;
                historyX = true;
            }
            double position = 0;

            if (Math.Abs(X - position_history[0, 0]) > 100)
            {
                if (Math.Abs(X - position_history[0, 1]) > 100)
                {
                    position = position_history[0, 0];
                }
                else
                {
                    position = 2 * X / 3 + position_history[0, 1] / 3;
                }
            }
            else
                position = 0.36 * X + 0.32 * position_history[0, 1] + 0.32 * position_history[0, 0];

            position_history[0, 1] = position_history[0, 0];
            position_history[0, 0] = X;
            return position;
        }
        double get_face_positionY(double Y)
        {
            if (historyY == false)
            {
                position_history[1, 0] = Y;
                position_history[1, 1] = Y;
                historyY = true;
            }
            double position = 0;
            if (Math.Abs(Y - position_history[1, 0]) > 100)
            {
                if (Math.Abs(Y - position_history[1, 1]) > 100)
                {
                    position = position_history[1, 0];
                }
                else
                {
                    position = 2 *Y / 3 + position_history[1, 1] / 3;
                }
            }
            else
                position = 0.36 * Y + 0.32 * position_history[1, 1] + 0.32 * position_history[1, 0];

            position_history[1, 1] = position_history[1, 0];
            position_history[1, 0] = Y;           
            return position;
        }
        double get_face_positionZ(double Z)
        {
            if (historyY == false)
            {
                position_history[2, 0] = Z;
                position_history[2, 1] = Z;
                historyY = true;
            }
            double position = 0;
            if (Math.Abs(Z- position_history[2, 0]) > 30)
            {
                if (Math.Abs(Z - position_history[2, 1]) > 30)
                {
                    position = position_history[2, 0];
                }
                else
                {
                    position = 2 * Z / 3 + position_history[2, 1] / 3;
                }
            }
            else
                position = 0.2 * Z + 0.45 * position_history[2, 1] + 0.35 * position_history[2, 0];

            position_history[2, 1] = position_history[1, 0];
            position_history[2, 0] = Z;
            return position;
        }

        //FACE DETECTION
        static readonly CascadeClassifier cascadeClassfier = new CascadeClassifier("haarcascade_frontalface_default.xml");
        void detec_face() 
        {
            big_rect[0] = 0;
            big_rect[1] = 0;
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

            int rect_num = 0;
            foreach (Rectangle rectangle in rectangles)
            {
                if (big_rect[1] < rectangle.Height * rectangle.Width)
                {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    using (Pen pen = new Pen(Color.Red, 1))
                    {
                        graphics.DrawRectangle(pen, rectangle);
                    }
                }
                
                    big_rect[1] = rectangle.Height * rectangle.Width;
                    big_rect[0] = rect_num;
                }
                ++rect_num;
            }
            imageBox1.Image.Dispose();
            imageBox1.Image= new Image<Bgr, Byte>(bitmap);

            if (rectangles != null && rectangles.Length != 0)
            {
                x_position = rectangles[big_rect[0]].X+ rectangles[big_rect[0]].Width/2-x_capture/2;
                x_position = get_face_positionX(x_position);

                y_position =-(rectangles[big_rect[0]].Y+ rectangles[big_rect[0]].Height/2-y_capture/2);
                y_position = get_face_positionY(y_position);

                distance = (100000-(rectangles[big_rect[0]].Width * rectangles[big_rect[0]].Height))/700;
                distance= get_face_positionZ(distance);
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
            if (timer.Enabled == false)
            {
                timer.Enabled = true;
                timer.Tick += new System.EventHandler(OnTimerEvent);
                timer.Start();
            }
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
