using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseAPI
{
    public partial class MainWindow : Form
    {
        System.Diagnostics.Stopwatch StopperAccurate;

        static SerialPort COM3;
        static SerialPort COM4;
        static int BaudRate = 38400;
        static Parity ParityMode = Parity.None;
        static int dataBits = 8;
        static StopBits stopBits = StopBits.One;
        private static int MessageLength = 19;
        int timeout = 5000;
        bool _continue = true;
        Thread readThreadCOM3;
        Thread readThreadCOM4;
        SerialDataReceivedEventHandler reev;

        public MainWindow()
        {
            InitializeComponent();
            COM3 = new SerialPort("COM3", BaudRate, ParityMode, dataBits, stopBits)
            {
                ReadTimeout = timeout,
                WriteTimeout = timeout
            };
            //COM3.Open();

            COM4 = new SerialPort("COM4", BaudRate, ParityMode, dataBits, stopBits)
            {
                ReadTimeout = timeout,
                WriteTimeout = timeout
            };
            COM4.Open();
            //COM4.NewLine = "\n";
            //readThreadCOM3 = new Thread(ReadCOM3);
            //readThreadCOM3.Start();
            readThreadCOM4 = new Thread(ReadCOM4);
            //readThreadCOM4.Start();
            reev = new SerialDataReceivedEventHandler(DataReceivedHandler);
            COM4.DataReceived += reev;
            StopperAccurate = new System.Diagnostics.Stopwatch();
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string message = COM4.ReadLine();
                byte[] ba = Encoding.Default.GetBytes(message);
                int value = int.Parse(message);
                string hexString = BitConverter.ToString(ba);
                hexString = hexString.Replace("-", " ");
                this.Invoke((MethodInvoker)delegate ()
                {
                    if (chart1.Series[0].Points.Count > 100)
                        chart1.Series[0].Points.RemoveAt(0);
                    this.chart1.Series[0].Points.AddY(value);

                    if (chart2.Series[0].Points.Count > 100)
                        chart2.Series[0].Points.RemoveAt(0);
                    this.chart2.Series[0].Points.AddY(StopperAccurate.ElapsedMilliseconds);

                    this.labelCOM4IN.Text = message;
                    this.labelCOM4HEX.Text = hexString;
                });
            }
            catch (TimeoutException) {
            }
            StopperAccurate.Restart();
        }

        //public void Read()
        //{
        //    while (_continue)
        //    {
        //        ReadCOM3();
        //        //ReadCOM4();
        //    }
        //}

        //public void ReadCOM3()
        //{
        //    while (_continue)
        //    {
        //        try
        //        {
        //            string message = COM3.ReadLine();
        //            byte[] ba = Encoding.Default.GetBytes(message);
        //            string hexString = BitConverter.ToString(ba);
        //            hexString = hexString.Replace("-", " ");
        //            this.Invoke((MethodInvoker)delegate ()
        //            {
        //                this.labelCOM3IN.Text = message;
        //                this.labelCOM3HEX.Text = hexString;
        //            });
        //        }
        //        catch (TimeoutException) { }
        //    }
        //}
        public void ReadCOM4()
        {
            while (_continue)
            {
                try
                {
                    //while (COM4.BytesToRead > 0) ;
                    string message = COM4.ReadLine();
                    byte[] ba = Encoding.Default.GetBytes(message);
                    string hexString = BitConverter.ToString(ba);
                    hexString = hexString.Replace("-", " ");
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        this.labelCOM4IN.Text = message;
                        this.labelCOM4HEX.Text = hexString;
                    });
                }
                catch (TimeoutException) { }
            }
        }

        //private void textBoxCOM3_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (e.KeyChar == (char)Keys.Enter)
        //    {
        //        //byte[] ba = Encoding.Default.GetBytes(this.textBoxCOM3.Text);
        //        //string hexString = BitConverter.ToString(ba);
        //        //hexString = hexString.Replace("-", " ");
        //        //this.labelCOM4IN.Text = this.textBoxCOM3.Text;
        //        //this.labelCOM4HEX.Text = hexString;
        //        COM3.WriteLine(this.textBoxCOM3.Text);
        //        this.textBoxCOM3.Text = "";
        //    }
        //}

        private void textBoxCOM4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                //byte[] ba = Encoding.Default.GetBytes(this.textBoxCOM4.Text);
                //string hexString = BitConverter.ToString(ba);
                //hexString = hexString.Replace("-", " ");
                //this.labelCOM3IN.Text = this.textBoxCOM4.Text;
                //this.labelCOM3HEX.Text = hexString;
                //this.textBoxCOM4.Text = "";
                List<string> messages = Utilities.SplitStringIntoParts(this.textBoxCOM4.Text,MessageLength);
                try
                {
                    foreach (string message in messages)
                    {
                        COM4.WriteLine(message);
                        //Thread.Sleep(10);
                        //while (COM4.BytesToRead > 0)
                        //    Thread.Sleep(1); ;
                    }
                    //COM4.WriteLine(this.textBoxCOM4.Text);
                }
                catch (Exception ex) {
                    int i = 0;
                }
                this.textBoxCOM4.Text = "";
            }
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (COM4.IsOpen)
            {
                e.Cancel = true; //cancel the fom closing
                Thread CloseDown = new Thread(new ThreadStart(CloseSerialOnExit)); //close port in new thread to avoid hang
                CloseDown.Start(); //close port in new thread to avoid hang
            }
        }

        private void CloseSerialOnExit()

        {
            try
            {
                COM4.Close(); //close the serial port
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); //catch any serial port closing error messages
            }
            this.Invoke((MethodInvoker)delegate ()
            {
                this.Close();
            }); //now close back in the main thread

        }
    }

}
