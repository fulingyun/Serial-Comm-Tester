/*
 *   This file is part of Serial-Comm-Tester source code.  All Rights Reserved.
 *
 *  Serial-Comm-Tetster is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License.
 *
 *  This software is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this software; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307,
 *  USA.
 */


/*
 *     Author: Philip Murray
 *     Project Homepage: https://github.com/PhilipMur/Serial-Comm-Tester
 */

using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO.Ports;
using System.Management;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Serial_Comm_Tester
{
    public partial class Form1 : Form
    {
        // public Conversion_Table conTable = null;

        private int send_repeat_counter = 0;
        private int RXcounter = 0;
        private int TXcounter = 0;
        private int rXChartCount = 0;
        private int tXChartCount = 0;

        private Encoding serialPortEncoding;

        private Dictionary<string, string> myAutoReplyDic;
        private char autoReplySplitterChar = ','; //default auto reply text splitter
        private bool isAutoReplyEnabled = false;
        private string autoReplyStringToMatch = "";
        private string autoReplyMessage = "";

        private bool ischkBAutoReadHexChecked = false;
        private bool ischkBAutoReadDecChecked = false;
        private bool ischkBAutoReadChecked = false;

        private bool ischeckBox2DECChecked = false;
        private bool ischeckBox3DECChecked = false;
        private bool ischeckBoxSendHexChecked = false;
        private bool ischeckBoxSendDecChecked = false;

        private int rxDelayTime = 100;



        public Form1()
        {
            InitializeComponent();
            //starts the system to look for available ports
            getAvailablePorts();

            Control.CheckForIllegalCrossThreadCalls = false;


        }

        private void SerialEncoding()
        {
            Invoke((MethodInvoker)delegate ()
            {
                comBoBoxformatText = comboBoxDecodeFormat.Text;


            });

            //  serialPort1.Encoding = Encoding.GetEncoding(1252); //US-ASCII
            //  serialPort1.Encoding = Encoding.GetEncoding(28591);  //EXTENDED ASCII  USED FOR  ISO 8859-1
            //new code with unicode encoding iso-8859-1
            if (serialPort1.IsOpen && btnOpenPort.Enabled == false && comBoBoxformatText == "windows-1252")
            {
                serialPortEncoding = Encoding.GetEncoding("windows-1252");
                // serialPort1.Encoding = Encoding.GetEncoding("windows-1252");
            }
            else if (serialPort1.IsOpen && btnOpenPort.Enabled == false && comBoBoxformatText == "utf-8")
            {
                serialPortEncoding = Encoding.GetEncoding("utf-8");
                // serialPort1.Encoding = Encoding.GetEncoding("utf-8");
            }
            else if (serialPort1.IsOpen && btnOpenPort.Enabled == false && comBoBoxformatText == "utf-16")
            {
                serialPortEncoding = Encoding.GetEncoding("utf-16");
                // serialPort1.Encoding = Encoding.GetEncoding("utf-16");
            }
            else if (serialPort1.IsOpen && btnOpenPort.Enabled == false && comBoBoxformatText == "us-ASCII")
            {
                serialPortEncoding = Encoding.GetEncoding("us-ASCII");
                // serialPort1.Encoding = Encoding.GetEncoding("us-ASCII");
            }
            else if (serialPort1.IsOpen && btnOpenPort.Enabled == false && comBoBoxformatText == "extended-ASCII")
            {
                serialPortEncoding = Encoding.GetEncoding(28591);
                // serialPort1.Encoding = Encoding.GetEncoding(28591);
            }

            else if (serialPort1.IsOpen && btnOpenPort.Enabled == false && comBoBoxformatText == "IBM-437")
            {
                serialPortEncoding = Encoding.GetEncoding(437);
                // serialPort1.Encoding = Encoding.GetEncoding(437);
            }
            else if (serialPort1.IsOpen && btnOpenPort.Enabled == false && comBoBoxformatText == "iso-8859-1")
            {
                serialPortEncoding = Encoding.GetEncoding("iso-8859-1");
                // serialPort1.Encoding = Encoding.GetEncoding("iso-8859-1");
            }

            else if (serialPort1.IsOpen && btnOpenPort.Enabled == false && comBoBoxformatText == "utf-32")
            {
                serialPortEncoding = Encoding.GetEncoding("utf-32");
                // serialPort1.Encoding = Encoding.GetEncoding("utf-32");
            }

            else if (serialPort1.IsOpen && btnOpenPort.Enabled == false && comBoBoxformatText == "utf-16-BigEndian")
            {
                serialPortEncoding = Encoding.BigEndianUnicode;
                // serialPort1.Encoding = Encoding.BigEndianUnicode;
            }
            else if (serialPort1.IsOpen && btnOpenPort.Enabled == false && comBoBoxformatText == "utf-32-BigEndian")
            {
                serialPortEncoding = new UTF32Encoding(true, true);
                // serialPort1.Encoding = new UTF32Encoding(true, true);
            }

            serialPort1.Encoding = serialPortEncoding;


        }
        //this code reads the serial port but catches a timeout exception
        private void btnRead_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {


                   // SerialEncoding();


                    string s = serialPort1.ReadExisting();


                    richTextBoxRecieve.Text += s;


                    if (s != "")
                    {
                        rXChartCount++; //update the samples per interval


                        RXcounter++;
                        lblRxSent.Update();
                        lblRxSent.Text = "RX :" + RXcounter;
                        //flash up rx
                        RX = true;
                    }



                  
                }
                catch (Exception ex)
                {
                    ComPortClosed();
                    MessageBox.Show(ex.Message, "Read Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (!serialPort1.IsOpen)
            {

                ComPortClosed();
            }
            richTextBoxSend.Focus();

            //await Task.Delay(100);
            //RX = false;
        }

        //this code just finds the available ports to use
        void getAvailablePorts()
        {
            //this code gets the name of the port and port number
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {
                var portnames = SerialPort.GetPortNames();
                var ports2 = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());

                var portList = portnames.Select(n => n + " - " + ports2.FirstOrDefault(s => s.Contains(n))).ToList();
                //this code displays the values in a list form in the textbox
                foreach (string s in portList)
                {
                    textBox11.Text += s.ToString() + "\r\n";
                }
            }
            //this code only gets the com port number
            string[] ports = SerialPort.GetPortNames();
            comboBoxActiveComPorts.Items.AddRange(ports);
        }




        private void Form1_Load(object sender, EventArgs e)
        {
            serialPortEncoding = Encoding.GetEncoding("windows-1252");
            comboBoxDecodeFormat.SelectedIndex = 0;
            comboBoxParity.SelectedIndex = 0;
            comboBoxDataBits.SelectedIndex = 3;
            comboBoxStopBits.SelectedIndex = 0;
            comboBoxFlow.SelectedIndex = 0;

            ischkBAutoReadChecked = true;
        }

        //this code on button 3 click it checks to see if combobox1&2 are empty ,else combobox1 = port name
        //it also catches a throw exception if the port is busy already and access denied
        //also it re-enables the buttons and textboxes and buttons if the port is open
        public void btnOpenPort_Click(object sender, EventArgs e)
        {

            try
            {
                if (comboBoxActiveComPorts.Text == "" || comboBoxBaudRate.Text == "" || comboBoxParity.Text == "" || comboBoxDataBits.Text == "" || comboBoxStopBits.Text == "" || comboBoxFlow.Text == "" || comboBoxDecodeFormat.Text == "" && !serialPort1.IsOpen)
                {
                    richTextBoxRecieve.Text = "Select Port Settings First";

                }


                else
                {
                    richTextBoxDec.Text = "";
                    richTextBoxHex.Text = "";
                    richTextBoxSend.Text = "";

                    btnChooseFileAutoReply.Enabled = true;

                    progressBar1.Value = 25;

                    tabControl1.SelectedIndex = 1;
                    tabControl2.SelectedIndex = 0;

                    lblRxSent.Text = "RX :" + RXcounter;
                    lblTxSent.Text = "TX :" + TXcounter;

                    SerialEncoding();

                    txRepeaterDelay.Tick += new EventHandler(SendData);

                    //enable logging
                    ckBAppendLogs.Enabled = false;
                    ckBEnableLogs.Enabled = false;
                    ckBOverwriteLogs.Enabled = false;


                    //this sets the possible comport settings from items in comboboxes collections
                    serialPort1.PortName = comboBoxActiveComPorts.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBoxBaudRate.Text);
                    serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), comboBoxParity.Text);
                    serialPort1.DataBits = Convert.ToInt32(comboBoxDataBits.Text);
                    serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), comboBoxStopBits.Text);

                    if (comboBoxFlow.Text == "RTS/CTS")
                    {
                        serialPort1.Handshake = (Handshake)Enum.Parse(typeof(Handshake), "None");

                        serialPort1.RtsEnable = true;
                        textBoxRTS.BackColor = Color.Lime;
                        textBoxRTS.Text = "no test";
                    }
                    else
                    {
                        serialPort1.Handshake = (Handshake)Enum.Parse(typeof(Handshake), comboBoxFlow.Text);
                    }


                    if (comboBoxFlow.Text != "None" | comboBoxFlow.Text != "XOnXOff")
                    {
                        chkBAutoRead.Enabled = false;
                        checkBoxAutoSend.Enabled = false;


                        chkBAutoReadDec.Checked = false;
                        chkBAutoReadHex.Checked = false;
                        chkBAutoReadDec.Enabled = false;
                        chkBAutoReadHex.Enabled = false;




                        chkBConvertToHexDec.Enabled = true; /////////

                        checkBoxSendHex.Enabled = true;
                        checkBoxSendDec.Enabled = true;
                        checkBoxSendNormal.Enabled = true;
                        checkBox2DEC.Enabled = true;
                        checkBox3DEC.Enabled = true;
                        //checkBoxSendBinary.Enabled = true;
                        //checkBoxSendOct.Enabled = true;

                    }


                    //this code checks that readtimeout is set and if not it defaults to -1
                    if (comboBoxReadTimeout.Text != "")
                    {
                        serialPort1.ReadTimeout = Convert.ToInt32(comboBoxReadTimeout.Text);
                    }
                    if (comboBoxReadTimeout.Text == "")
                    {
                        serialPort1.ReadTimeout = 500;
                    }

                    //this code checks that writetimeout is set and if not it defaults to -1
                    if (comboBoxWriteTimeout.Text != "")
                    {
                        serialPort1.WriteTimeout = Convert.ToInt32(comboBoxWriteTimeout.Text);
                    }
                    if (comboBoxWriteTimeout.Text == "")
                    {
                        serialPort1.WriteTimeout = 500;
                    }

                    progressBar1.Value = 50;

                    //finally it opens the serial port here if its not open
                    serialPort1.Open();

                    //MODIFIED 24/01/17 COMMENTED OUT GOING FOR LABEL INSTEAD
                    //  richTextBox2.Text = "Serial Port Is Open";

                    lblPortStatus.Text = "Open";
                    lblPortStatus.BackColor = Color.Lime;

                    richTextBoxRecieve.Text = "";

                    //this gets the current port settings and displays in the textbox1
                    txtBCurrentPortSet.Text = "Port :" + Convert.ToString(serialPort1.PortName) + "\t" + "Baud Rate :"
                        + Convert.ToInt32(serialPort1.BaudRate) + "\t\t" + "Parity :" + serialPort1.Parity +
                       "\t" + "Data Bits :" + Convert.ToInt32(serialPort1.DataBits) + "\t" + "StopBits :" + serialPort1.StopBits + "\t" +
                       "Handshake :" + serialPort1.Handshake;

                    txtBCurrentPortSet.BackColor = Color.Chartreuse;

                    progressBar1.Value = 75;

                    //this sets the boxes to enabled or disabled
                    richTextBoxDec.Clear();
                    richTextBoxHex.Clear();
                    richTextBoxSend.Enabled = true;
                    richTextBoxRecieve.Enabled = true;
                    richTextBoxDec.Enabled = true;
                    richTextBoxHex.Enabled = true;

                    btnSend.Enabled = true;
                    btnRead.Enabled = true;
                    btnOpenPort.Enabled = false;
                    btnClosePort.Enabled = true;
                    btnRefreshComPorts.Enabled = false;


                    btnDtrOn.Enabled = true;
                    btnRtsOn.Enabled = true;
                 

                    serialPort1.RtsEnable = false;
                    textBoxRTS.BackColor = Color.LightSkyBlue;
                    textBoxRTS.Text = "no test";

                    serialPort1.DtrEnable = true;
                    textBoxDTR.BackColor = Color.LightSkyBlue;
                    textBoxDTR.Text = "no test";

                    //modified 24/01/17 disable COMBOboxes
                    comboBoxActiveComPorts.Enabled = false;
                    comboBoxBaudRate.Enabled = false;
                    comboBoxDataBits.Enabled = false;
                    comboBoxDecodeFormat.Enabled = false;
                    comboBoxFlow.Enabled = false;
                    comboBoxParity.Enabled = false;
                    comboBoxReadTimeout.Enabled = false;
                    comboBoxStopBits.Enabled = false;
                    comboBoxWriteTimeout.Enabled = false;




                    //this gets the current value of read and write timeouts and displays in the textbox
                    //textBoxReadTime.Text = serialPort1.ReadTimeout.ToString();
                    //textBoxWriteTime.Text = serialPort1.WriteTimeout.ToString();

                    progressBar1.Value = 100;
                    richTextBoxSend.Focus();

                    //this code checks if combobox has None enabled and is not using XOnXOff etc.....
                    if (serialPort1.IsOpen && comboBoxFlow.Text == "None" | comboBoxFlow.Text == "XOnXOff")
                    {
                        btnRtsOn.Enabled = true;

                    }
                    chkBAutoRead.Enabled = true;
                    checkBoxAutoSend.Enabled = true;
                    chkBAutoReadDec.Enabled = true;
                    chkBAutoReadHex.Enabled = true;
                    chkBConvertToHexDec.Enabled = true;

                    checkBoxSendNormal.Checked = true;
                    chkBAutoRead.Checked = true;

                    checkBoxSendHex.Enabled = true;
                    checkBoxSendDec.Enabled = true;
                    checkBoxSendNormal.Enabled = true;
                    checkBox2DEC.Enabled = true;
                    checkBox3DEC.Enabled = true;

                    btnSave.Enabled = true;
                    //checkBoxSendBinary.Enabled = true;
                    //checkBoxSendOct.Enabled = true;


                    //==================

                    if (serialPort1.DsrHolding == true)
                    {
                        textBoxDSR.BackColor = Color.Lime;
                        textBoxDSR.Text = "no test";
                    }
                    else
                    {
                        textBoxDSR.BackColor = Color.LightSkyBlue;
                        textBoxDSR.Text = "no test";
                    }

                    if (serialPort1.CDHolding == true)
                    {
                        textBoxCD.BackColor = Color.Lime;
                        textBoxCD.Text = "testing";
                    }
                    else
                    {
                        textBoxCD.BackColor = Color.LightSkyBlue;
                        textBoxCD.Text = "no test";

                    }

                    if (serialPort1.CtsHolding == true)
                    {
                        textBoxCTS.BackColor = Color.Lime;
                        textBoxCTS.Text = "testing";
                    }
                   else
                    {
                        textBoxCTS.BackColor = Color.LightSkyBlue;
                        textBoxCTS.Text = "no test";

                    }

                    //this code gets the the serial ring value
                    //normally the ring only lasts a short time when activated like 2 seconds max
                    textBoxRI.Text = "no test";
                    textBoxRI.BackColor = Color.LightSkyBlue;
                  
                    //this code gets the current state of a break point if a break on the input is detected
                    textBoxBI.Text = "no test";
                    if (serialPort1.BreakState)
                    {
                       
                        textBoxBI.BackColor = Color.Lime;

                    }
                    else
                    {
                        textBoxBI.BackColor = Color.LightSkyBlue;

                    }

                }



            }

            catch (Exception ex)
            {

                progressBar1.Value = 0;
                tabControl1.SelectedIndex = 0;
                MessageBox.Show(ex.Message, "Message ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        //this code uses async to delay closing the port untill auto reads are disabled
        //this code when the close serial port is clicked it disabled the buttons again untill the port is opened again
        private async void btnClosePort_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen == true)
            {

                btnChooseFileAutoReply.Enabled = false;

                ckBAppendLogs.Enabled = true;
                ckBEnableLogs.Enabled = true;
                ckBOverwriteLogs.Enabled = true;

                if (comboBoxFlow.Text == "RTS/CTS")
                {
                    serialPort1.RtsEnable = false;
                    textBoxRTS.BackColor = Color.LightSkyBlue;
                    // serialPort1.RtsEnable = false;  //---------------------------------------17/6/17
                    textBoxRTS.Text = "no test";
                }

                richTextBoxSend.Enabled = false;
                btnRefreshComPorts.Enabled = true;
                if (comboBoxFlow.Text == "None" | comboBoxFlow.Text == "XOnXOff")
                {
                    serialPort1.RtsEnable = false;
                }
                if (serialPort1.DtrEnable)
                {
                    serialPort1.DtrEnable = false;
                }
                textBoxRTS.BackColor = Color.LightSkyBlue;
                // serialPort1.RtsEnable = false;  //---------------------------------------17/6/17
                textBoxRTS.Text = "no test";

                textBoxDTR.BackColor = Color.LightSkyBlue;
                textBoxDTR.Text = "no test";
                textBoxDSR.BackColor = Color.LightSkyBlue;
                textBoxDSR.Text = "no test";
                textBoxCD.BackColor = Color.LightSkyBlue;
                textBoxCD.Text = "no test";
                textBoxCTS.BackColor = Color.LightSkyBlue;
                textBoxCTS.Text = "no test";

                RXcounter = 0;
                TXcounter = 0;
                send_repeat_counter = 0;



                chkBAutoRead.Checked = false;
                checkBoxAutoSend.Checked = false;
                chkBAutoRead.Enabled = false;
                checkBoxAutoSend.Enabled = false;
                chkBAutoReadDec.Checked = false;
                chkBAutoReadHex.Checked = false;
                chkBAutoReadDec.Enabled = false;
                chkBAutoReadHex.Enabled = false;
                chkBConvertToHexDec.Enabled = false;
                chkBConvertToHexDec.Checked = false;
                checkBoxSendHex.Enabled = false;
                checkBoxSendHex.Checked = false;
                checkBoxSendDec.Enabled = false;
                checkBoxSendDec.Checked = false;
                checkBoxSendNormal.Enabled = false;
                checkBoxSendNormal.Checked = false;
                checkBox2DEC.Enabled = false;
                checkBox2DEC.Checked = false;
                checkBox3DEC.Enabled = false;
                checkBox3DEC.Checked = false;
                //checkBoxSendBinary.Enabled = false;
                //checkBoxSendOct.Enabled = false;

                //modified 24/01/17 disable COMBOboxes
                comboBoxActiveComPorts.Enabled = true;
                comboBoxBaudRate.Enabled = true;
                comboBoxDataBits.Enabled = true;
                comboBoxDecodeFormat.Enabled = true;
                comboBoxFlow.Enabled = true;
                comboBoxParity.Enabled = true;
                comboBoxReadTimeout.Enabled = true;
                comboBoxStopBits.Enabled = true;
                comboBoxWriteTimeout.Enabled = true;

                txtBCurrentPortSet.BackColor = Color.DarkGray;

                tabControl1.SelectedIndex = 0;

                txRepeaterDelay.Stop();

                send_repeat_counter = 0;


                txRepeaterDelay.Tick -= new EventHandler(SendData); //this removes the event handler
                txRepeaterDelay.Dispose(); //dispose of the new event handler

                if (timerGraph.Enabled)
                {
                    ckBStartGraph.Checked = false;
                    graph_speed.Enabled = true;
                    timerGraph.Stop();
                }

                // serialPort1.DiscardInBuffer();
                //  serialPort1.DiscardOutBuffer();
                //serialPort1.Close();

                //serialPort1.Dispose();

                await Button1ClickAsync();

            }
        }

        public async Task Button1ClickAsync()
        {
            // Do asynchronous work and wait untill (a set time)
            await Task.Delay(500);


            if (serialPort1.IsOpen)
                try
                {

                    //this code closes the port and enables or disables the neccessary buttons

                    //richTextBox2.Enabled = false;
                    //richTextBox3.Enabled = false;
                    //richTextBox4.Enabled = false;

                    //MODIFIED 24/01/17 wont clear boxes untill re opened
                    //richTextBoxDec.Text = "";
                    //richTextBoxHex.Text = "";
                    //richTextBoxSend.Text = "";

                    //MODIFIED 24/01/17 LABEL INSTEAD
                    //  richTextBox2.Text = "Serial Port Is Closed";
                    lblPortStatus.Text = "Closed";
                    lblPortStatus.BackColor = Color.Red;

                    textBoxRI.Text = "";
                    textBoxRI.BackColor = Color.LightSkyBlue;
                    textBoxBI.BackColor = Color.LightSkyBlue;
                    textBoxBI.Text = "";
                    progressBar1.Value = 0;
                    btnSend.Enabled = false;
                    btnRead.Enabled = false;
                    btnClosePort.Enabled = false;
                    btnOpenPort.Enabled = true;
                    //rts dtr buttons
                    btnRtsOn.Enabled = true;
                    btnDtrOn.Enabled = true;
                    chkBAutoRead.Checked = false;
                    checkBoxAutoSend.Checked = false;
                    chkBAutoRead.Enabled = false;
                    checkBoxAutoSend.Enabled = false;
                    chkBAutoReadDec.Checked = false;
                    chkBAutoReadHex.Checked = false;
                    chkBAutoReadDec.Enabled = false;
                    chkBAutoReadHex.Enabled = false;
                    chkBConvertToHexDec.Enabled = false;
                    chkBConvertToHexDec.Checked = false;
                    checkBoxSendHex.Enabled = false;
                    checkBoxSendHex.Checked = false;
                    checkBoxSendDec.Enabled = false;
                    checkBoxSendDec.Checked = false;
                    checkBoxSendNormal.Enabled = false;
                    checkBoxSendNormal.Checked = false;
                    //checkBoxSendBinary.Enabled = false;
                    //checkBoxSendOct.Enabled = false;


                    serialPort1.DiscardInBuffer();
                    serialPort1.DiscardOutBuffer();

                    serialPort1.Close();
                    serialPort1.Dispose();

                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "message");
                    tabControl1.SelectedIndex = 0;
                }

            if (!serialPort1.IsOpen && btnClosePort.Enabled == true)
                try
                {

                    ComPortClosed();
                }
                catch (Exception ex)
                {
                    ComPortClosed();
                    tabControl1.SelectedIndex = 0;
                    MessageBox.Show(ex.Message, "Message");
                }
        }
        //THIS IS FOR CLOSING THE COM PORT
        private void ComPortClosed()
        {

            richTextBoxSend.Text = "";
            richTextBoxSend.Text = "COM Port Closed Unexpectedly";

            lblPortStatus.Text = "Closed";
            lblPortStatus.BackColor = Color.Red;

            //  richTextBoxSend.Text = "";
            textBoxRI.Text = "";
            textBoxRI.BackColor = Color.LightSkyBlue;
            textBoxBI.Text = "";
            textBoxBI.BackColor = Color.LightSkyBlue;
            progressBar1.Value = 0;
            btnSend.Enabled = false;
            btnRead.Enabled = false;
            btnClosePort.Enabled = false;
            btnOpenPort.Enabled = true;
            richTextBoxSend.Enabled = false;
            btnRefreshComPorts.Enabled = true;
            textBoxRTS.BackColor = Color.Red;
            textBoxRTS.Text = "no test";
            serialPort1.DtrEnable = false;
            textBoxDTR.BackColor = Color.Red;
            textBoxDTR.Text = "no test";
            //DTR AND RTS
            btnRtsOn.Enabled = true;
            btnDtrOn.Enabled = true;

            textBoxDSR.BackColor = Color.LightSkyBlue;
            textBoxDSR.Text = "no test";
            textBoxCD.BackColor = Color.LightSkyBlue;
            textBoxCD.Text = "no test";
            textBoxCTS.BackColor = Color.LightSkyBlue;
            textBoxCTS.Text = "no test";
            chkBAutoRead.Checked = false;
            checkBoxAutoSend.Checked = false;
            chkBAutoRead.Enabled = false;
            checkBoxAutoSend.Enabled = false;
            chkBAutoReadDec.Checked = false;
            chkBAutoReadHex.Checked = false;
            chkBAutoReadDec.Enabled = false;
            chkBAutoReadHex.Enabled = false;
            chkBConvertToHexDec.Enabled = false;
            chkBConvertToHexDec.Checked = false;
            checkBoxSendHex.Enabled = false;
            checkBoxSendHex.Checked = false;
            checkBoxSendDec.Enabled = false;
            checkBoxSendDec.Checked = false;
            checkBoxSendNormal.Enabled = false;
            checkBoxSendNormal.Checked = false;
            checkBox2DEC.Enabled = false;
            checkBox2DEC.Checked = false;
            checkBox3DEC.Enabled = false;
            checkBox3DEC.Checked = false;
            //checkBoxSendBinary.Enabled = false;
            //checkBoxSendOct.Enabled = false;



            //modified 24/01/17 disable COMBOboxes
            comboBoxActiveComPorts.Enabled = true;
            comboBoxBaudRate.Enabled = true;
            comboBoxDataBits.Enabled = true;
            comboBoxDecodeFormat.Enabled = true;
            comboBoxFlow.Enabled = true;
            comboBoxParity.Enabled = true;
            comboBoxReadTimeout.Enabled = true;
            comboBoxStopBits.Enabled = true;
            comboBoxWriteTimeout.Enabled = true;

            txtBCurrentPortSet.BackColor = Color.DarkGray;

            tabControl1.SelectedIndex = 0;
        }


        //this code writes whatever is in richtextbox1 to the serial port and then clears the textbox
        //this bastard was transfering return key as 0a hex
        private string comBoBoxformatText;
        private string tx_data_p;

        private static bool DecSend = false;

        private static bool sendDataNoError = true;

        private void SendData(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen && sendDataNoError == true)
            {
                string tx_data = "";

                if (send_repeat_counter < (int)send_repeat.Value)
                {
                    tx_data = tx_data_p;//richTextBoxSend.Text;

                    tXChartCount++;//tx chart values per interval
                    TXcounter++;

                    lblTxSent.Update();
                    lblTxSent.Text = "TX :" + TXcounter;

                   
                    try
                    {
                        //THIS SEND NORMAL ENCODING EXAMPLE ASCII UTF-8 ETC
                        if (checkBoxSendHex.Checked == false && checkBoxSendDec.Checked == false)
                        {
                            if (boolCarrigeReturnLF == true)
                            {
                                serialPort1.Write(tx_data + "\r\n");
                               
                            }
                            // serialPort1.Write(tx_data.Replace("\\n", Environment.NewLine));
                            if (boolCarrigeReturn == true)
                            {
                                serialPort1.Write(tx_data + "\r");
                              
                            }
                            if (boolCarrigeReturn == false && boolCarrigeReturnLF == false)
                            {
                                serialPort1.Write(tx_data);
                               
                            }

                           
                            TX = true;

                           
                        }

                        //this is to send the values as hexadecimal with or without carrige return and line feed
                        if (checkBoxSendHex.Checked == true)
                        {
                            string asciiToHexSend2 = ConvertHex(tx_data);
                            if (boolCarrigeReturnLF == true)
                            {
                                
                                serialPort1.Write(asciiToHexSend2 + "\r\n");
                               
                            }
                            if (boolCarrigeReturn == true)
                            {

                                serialPort1.Write(asciiToHexSend2 + "\r");
                                
                            }

                            if (boolCarrigeReturn == false && boolCarrigeReturnLF == false)
                            {
                                
                                serialPort1.Write(asciiToHexSend2);


                            }
                            //for lighting up tx
                            TX = true;
                           
                        }


                        if (checkBoxSendDec.Checked == true)
                        {
                            string asciiToHexSend = ConvertDec(tx_data);

                            if (boolCarrigeReturnLF == true)
                            {
                                
                                serialPort1.Write(asciiToHexSend + "\r\n");
                               
                            }
                            if (boolCarrigeReturn == true)
                            {
                                serialPort1.Write(asciiToHexSend + "\r");
                                
                            }

                            if (boolCarrigeReturn == false && boolCarrigeReturnLF == false)
                            {
                                
                                serialPort1.Write(asciiToHexSend);




                            }
                            TX = true;
                          
                           
                        }
                        send_repeat_counter++;
                    }
                    catch
                    {

                        // ComPortClosed();

                        txRepeaterDelay.Stop();
                        send_repeat_counter = 0;
                        MessageBox.Show("Can't write to " + serialPort1.PortName + " port is in use already!!");
                    }
                }

            }
            if (send_repeat_counter == (int)send_repeat.Value)
            {
                txRepeaterDelay.Stop();

                // sendData.Text = "Send";
                //send_repeat_counter = 1;
            }

            if (!serialPort1.IsOpen)
            {

                ComPortClosed();
                txRepeaterDelay.Stop();

                send_repeat_counter = 0;
            }

           
        }

        private void btnSend_Click(object sender, EventArgs e)   ///>>>>>>>>>>>>bool decsend
        {
            send_repeat_counter = 0; //put this here as closing the form and reopening the form would dounble up on the send

            sendDataNoError = true;

           // SerialEncoding();

            richTextBoxSend.Focus();
            //string removeSpaces = richTextBoxSend.Text.Replace(" " , "");   ///THIS WORKS BY SEEING IF THE STRING TO INT IS DIVISABLE BY 2 TO >>>>>>>>>>GENIUS I AM
            //int removeSpacesLenght = removeSpaces.Length;
            //if (removeSpacesLenght % 2 == 0 )
            if (ischeckBox2DECChecked | ischeckBoxSendHexChecked)
            {
                // MessageBox.Show("send is even");
                DecSend = true;
            }
            //  cout << "String length is even" << endl;
            // else
            if (ischeckBox3DECChecked)
            {
                //  MessageBox.Show("send is odd");
                DecSend = false;
            }
            // cout << "String length is odd" 

            //THIS IS USED TO CONVERT THE SEND TEXT TO HEX AND SEND THROUGH THE SERIAL PORT////////
            if (serialPort1.IsOpen && richTextBoxSend.Text != "" && btnSend.Enabled)
                try
                {
                    // SerialEncoding();

                    if (ischeckBoxSendHexChecked == true)
                    {

                        tx_data_p=richTextBoxSend.Text;
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                    //this writes decimal to the serial port
                    if (ischeckBoxSendDecChecked == true)
                    {

                        tx_data_p = richTextBoxSend.Text;
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();


                    }

                    ////this allows access to the value of comboBox1 cross threading
                    if (ischeckBoxSendHexChecked == false && ischeckBoxSendDecChecked == false)
                    {


                        // SerialEncoding();
                        tx_data_p = richTextBoxSend.Text;
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();


                    }
                }
                catch (Exception ex)
                {
                    // ComPortClosed();
                    //  MessageBox.Show("if sending as hex use hex values example 01 not 1 by itself","Message for a newbie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // richTextBox2.Text = "Unauthorized Access Exception Thrown";
                    MessageBox.Show(ex.Message, "Message ", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            if (!serialPort1.IsOpen)
            {

                ComPortClosed();

            }


        }
        //this code makes sure on exit that the port closes too
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
            }


        }
        //this code gets the received data from the serial port and displya it in richtextbox2
        private string richbox2string;
        private string richboxHexString;
        private string richboxDecString;

        public void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
          
            //background worker will wait the RX read Delay time before collecting all the received data as 
            //it can come in smaller chunks and we want the full message
            if (bgwAutoReply.IsBusy == false)
            {
                bgwAutoReply.RunWorkerAsync();
            }

           
        }

        //this code gets the hex value from the com port and drops to a new line if carrige return or line feed is displayed
        private void DisplayAutoHex(object o, EventArgs e)
        {


            //richTextBoxHex.AppendText(SB.ToString());
            string[] hexSeparators = { "0A" };
            string hexValue = richboxHexString;
            string[] hexWords = hexValue.Split(hexSeparators, StringSplitOptions.None);

            string[] hexSeparators2 = { "0D" };
            string hexValue2 = richboxHexString;
            string[] hexWords2 = hexValue2.Split(hexSeparators2, StringSplitOptions.None);


            if (hexValue.Contains("0A"))
            {

                foreach (var word in hexWords)
                {

                    richTextBoxHex.AppendText(word + "0A" + "\n");

                    WriteLogsToFile(word + "0A" + Environment.NewLine);
                }

            }
            else if (hexValue2.Contains("0D"))
            {

                foreach (var word in hexWords2)
                {

                    richTextBoxHex.AppendText(word + "0D" + "\n");

                    WriteLogsToFile(word + "0D" + Environment.NewLine);
                }

            }
            else if (!hexValue.Contains("0A") | !hexValue.Contains("0D"))
            {
                richTextBoxHex.AppendText(richboxHexString);


                WriteLogsToFile(richboxHexString);

            }

          
        }
        //THIS WRITES THE LOG FILES
        private void WriteLogsToFile(string input)
        {
            //this will write to log file
            if (ckBEnableLogs.Checked)
            {
                try
                {
                    // loggingFile.WriteAsync("Hello wtf");
                    using (StreamWriter sw = new StreamWriter(lblDataLogFilePath.Text, ckBAppendLogs.Checked))
                    {
                        // write a line of text to the file
                        sw.Write(input);
                        sw.Flush();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }
        //this code gets the dec value from com port and drops to a new line if carrige return or line feed is displayed
        private void DisplayAutoDec(object o, EventArgs e)
        {
            // richTextBoxDec.AppendText(richbox3string);

            string[] decSeparators = { "010" };

            string decValue = richboxDecString;
            string[] decWords = decValue.Split(decSeparators, StringSplitOptions.None);

            string[] decSeparators2 = { "013" };

            string decValue2 = richboxDecString;
            string[] decWords2 = decValue2.Split(decSeparators2, StringSplitOptions.None);

            if (decValue.Contains("010"))
            {

                foreach (var word in decWords)
                {

                    richTextBoxDec.AppendText(word + "010" + "\n");

                    WriteLogsToFile(word + "010" + Environment.NewLine);
                }

            }

            else if (decValue2.Contains("013"))
            {

                foreach (var word in decWords2)
                {

                    richTextBoxDec.AppendText(word + "013" + "\n");

                    WriteLogsToFile(word + "013" + Environment.NewLine);
                }

            }

            else if (!decValue.Contains("010") && !decValue2.Contains("013"))
            {
                richTextBoxDec.AppendText(richboxDecString);

                WriteLogsToFile(richboxDecString);
            }

        }

        //this code sends the received data to richtextbox2 and doesnt overwrite it
        private void DisplayAutoText(object o, EventArgs e)
        {
            string[] textSeparators = { "\n" };

            string textValue = richbox2string;
            string[] textWords = textValue.Split(textSeparators, StringSplitOptions.None);

            string[] textSeparators2 = { "\r" };

            string textValue2 = richbox2string;
            string[] textWords2 = textValue2.Split(textSeparators2, StringSplitOptions.None);

            richTextBoxRecieve.AppendText(richbox2string);


            if (textValue.Contains("\r")) //if it has a carrige return go to a newline  
            {
                foreach (var word in textWords)
                {

                    WriteLogsToFile(word + Environment.NewLine);

                }
            }
            else if (textValue2.Contains("\n")) //if it has a linefeed go to a newline  
            {
                foreach (var word in textWords2)
                {

                    WriteLogsToFile(word + Environment.NewLine);

                }
            }

            else if (!textValue.Contains("\n") && !textValue2.Contains("\r"))
            {
                WriteLogsToFile(richbox2string);

            }




        }
        /// <summary>
        /// This will look through the Auto reply dictionary to find a match and reply to the message
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void WriteAutoReplyMessage(object o, EventArgs e)
        {

        }

        //when the clear button is pressed it will clear both text boxes and clears in/out serial buffers
        private void btnClearAllText_Click(object sender, EventArgs e)
        {

            richTextBoxSend.Clear();
            richTextBoxRecieve.Clear();
            richTextBoxDec.Clear();
            richTextBoxHex.Clear();

            richTextBoxSend.Focus();

            RXcounter = 0;
            TXcounter = 0;

            lblRxSent.Text = "RX :" + RXcounter;
            lblTxSent.Text = "TX :" + TXcounter;
            //if (serialPort1.IsOpen)
            //{
            //    serialPort1.DiscardInBuffer();
            //    serialPort1.DiscardOutBuffer();
            //}

        }
        //this code sends the data in realtime when checkedbox2 is checked
        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (checkBoxSendHex.Checked || checkBoxSendDec.Checked)
            {

                char ch = e.KeyChar;
                if (richTextBoxSend.Text.Length <= 5 && char.IsControl(ch) && ch == 8)
                {
                    richTextBoxSend.Clear();
                    t = 1;
                    j = 2;
                }
                //THIS REMOVES A CHUNK OF TEXT FROM THE TEXTBOX SEND IF HEX OR DEC IS CHOOSEN
                if (richTextBoxSend.Text.Length > 5 && char.IsControl(ch) && ch == 8 && checkBox3DEC.Checked)
                {
                    //this removes the next 5 chars when the backspace button is hit
                    richTextBoxSend.Text = richTextBoxSend.Text.Substring(0, richTextBoxSend.Text.Length - 4);

                    //this resets the cursor position to the end of the text
                    richTextBoxSend.Select(richTextBoxSend.Text.Length, 0);

                    j = j - 4;


                }
                if (richTextBoxSend.Text.Length > 5 && char.IsControl(ch) && ch == 8 && checkBox2DEC.Checked | checkBoxSendHex.Checked)
                {
                    richTextBoxSend.Text = richTextBoxSend.Text.Substring(0, richTextBoxSend.Text.Length - 3);
                    richTextBoxSend.Select(richTextBoxSend.Text.Length, 0);

                    t = t - 3;


                }
            }
            if (serialPort1.IsOpen)
                try
                {


                    if (serialPort1.IsOpen && checkBoxAutoSend.Checked)

                    {
                        //this code just sends out a character key from keyboard
                        // char[] ch = new char[1];
                        // ch[0] = e.KeyChar;
                        //serialPort1.Write(ch, 0, 1);

                        //MODIFIED 24/1/17 COMMENTED OUT
                        ////this code sends the new serial encoding type but only through reading the textbox so one charcter delay
                        //this allows access to the value of comboBox1 cross threading
                        Invoke((MethodInvoker)delegate ()
                        {
                            comBoBoxformatText = comboBoxDecodeFormat.Text;


                        });


                        // SerialEncoding();

                        //serialPort1.Write(richTextBoxSend.Text);
                        if (boolCarrigeReturnLF == true)
                        {
                            serialPort1.Write(richTextBoxSend.Text + "\r\n");

                        }
                        // serialPort1.Write(tx_data.Replace("\\n", Environment.NewLine));
                        if (boolCarrigeReturn == true)
                        {
                            serialPort1.Write(richTextBoxSend.Text + "\r");

                        }
                        if (boolCarrigeReturn == false && boolCarrigeReturnLF == false)
                        {
                            serialPort1.Write(richTextBoxSend.Text);

                        }
                        richTextBoxSend.Clear();
                        serialPort1.DiscardOutBuffer();

                        TX = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Original Error :" + ex.Message, "Clear To Send Is Not Active ", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }


            if (!serialPort1.IsOpen && checkBoxAutoSend.Checked)
                try
                {


                    ComPortClosed();

                }


                catch (Exception ex)
                {
                    ComPortClosed();

                    MessageBox.Show("Original Error :" + ex.Message, "Clear To Send Is Not Active ", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            //await Task.Delay(100);
            //TX = false;

        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            AboutBox1 openabout = new AboutBox1();
            openabout.ShowDialog();
        }
        //this code checks to see if comport is in use before restart/refreshing the system iterases the previous items 
        //before adding current new item port names
        public void btnRefreshComPorts_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)

                try
                {
                    comboBoxActiveComPorts.Items.Clear();
                    textBox11.Clear();

                    getAvailablePorts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Serial Port is Busy Or Open", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            else
            {
                MessageBox.Show("Close The Current Port Before Refreshing", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }

        }
        private void btnRtsOn_Click(object sender, EventArgs e)
        {
            send_repeat_counter = 0; //put this here as closing the form and reopening the form would dounble up on the send

            sendDataNoError = true;

            // SerialEncoding();

            if (serialPort1.IsOpen)
            {
                myAutoReplyDic = new Dictionary<string, string>();

                try
                {
                    string[] strArr = File.ReadAllLines("mic.txt");

                    lblIsAutoFileLoaded.Text = "mic.txt";
                    lblAutoReplyLineCount.Text = strArr.Length.ToString();

                    foreach (string str in strArr)
                    {
                        string[] strSplitter = str.Split(autoReplySplitterChar);

                        if (strSplitter.Length == 2)
                        {
                            if (rBtnText.Checked)
                            {
                                if (myAutoReplyDic.ContainsKey(strSplitter[0]) == false)
                                {
                                    myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);
                                    /*if (boolCarrigeReturnLF == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r\n", strSplitter[1]);

                                    }
                                    // serialPort1.Write(tx_data.Replace("\\n", Environment.NewLine));
                                    if (boolCarrigeReturn == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r", strSplitter[1]);

                                    }
                                    if (boolCarrigeReturn == false && boolCarrigeReturnLF == false)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);

                                    }*/
                                }
                            }
                            else if (rBtnHex.Checked)
                            {
                                if (strSplitter[0].Length % 2 == 0 && strSplitter[1].Length % 2 == 0)
                                {
                                    string hexKey = "";
                                    string hexValue = "";

                                    //key
                                    for (int i = 0; i < strSplitter[0].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[0].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexKey += s.ToString("X2");
                                        }
                                    }
                                    //value
                                    for (int i = 0; i < strSplitter[1].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[1].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexValue += s.ToString("X2");
                                        }
                                    }

                                    if (myAutoReplyDic.ContainsKey(hexKey) == false)
                                    {
                                        myAutoReplyDic.Add(hexKey, hexValue);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("The file does not contain formatted hex values..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                            }

                        }
                        else
                        {
                            MessageBox.Show("The file does not contain the char splitter between message and reply..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                    }

                    if (myAutoReplyDic.Count > 0)
                    {
                        rBtnEnableAutoReply.Enabled = true;
                        rBtnEnableAutoReply.Checked = true;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error ::" + ex.Message, "Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (rBtnText.Checked)
                {
                    chkBAutoRead.Checked = true;
                    tabControl2.SelectedIndex = 0;
                    checkBoxSendNormal.Checked = true;
                }
                else if (rBtnHex.Checked)
                {
                    chkBAutoReadHex.Checked = true;
                    tabControl2.SelectedIndex = 1;
                    checkBoxSendHex.Checked = true;
                }

                tabControl1.SelectedIndex = 1;
            }

            if (ischeckBox2DECChecked | ischeckBoxSendHexChecked)
            {
                // MessageBox.Show("send is even");
                DecSend = true;
            }
            //  cout << "String length is even" << endl;
            // else
            if (ischeckBox3DECChecked)
            {
                //  MessageBox.Show("send is odd");
                DecSend = false;
            }
            // cout << "String length is odd" 

            //THIS IS USED TO CONVERT THE SEND TEXT TO HEX AND SEND THROUGH THE SERIAL PORT////////
            if (serialPort1.IsOpen && btnRtsOn.Enabled)
                try
                {
                    // SerialEncoding();

                    if (ischeckBoxSendHexChecked == true)
                    {

                        tx_data_p = "mic";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                    //this writes decimal to the serial port
                    if (ischeckBoxSendDecChecked == true)
                    {

                        tx_data_p = "mic";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }

                    ////this allows access to the value of comboBox1 cross threading
                    if (ischeckBoxSendHexChecked == false && ischeckBoxSendDecChecked == false)
                    {

                        // SerialEncoding();
                        tx_data_p = "mic";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                }
                catch (Exception ex)
                {
                    // ComPortClosed();
                    // MessageBox.Show("if sending as hex use hex values example 01 not 1 by itself","Message for a newbie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // richTextBox2.Text = "Unauthorized Access Exception Thrown";
                    MessageBox.Show(ex.Message, "Message ", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }


            if (!serialPort1.IsOpen)
            {

                ComPortClosed();

            }
        }

        private void btnRtsOff_Click(object sender, EventArgs e)
        {
            send_repeat_counter = 0; //put this here as closing the form and reopening the form would dounble up on the send

            sendDataNoError = true;

            // SerialEncoding();
            if (serialPort1.IsOpen)
            {
                myAutoReplyDic = new Dictionary<string, string>();

                try
                {
                    string[] strArr = File.ReadAllLines("info.txt");

                    lblIsAutoFileLoaded.Text = "info.txt";
                    lblAutoReplyLineCount.Text = strArr.Length.ToString();

                    foreach (string str in strArr)
                    {
                        string[] strSplitter = str.Split(autoReplySplitterChar);

                        if (strSplitter.Length == 2)
                        {
                            if (rBtnText.Checked)
                            {
                                if (myAutoReplyDic.ContainsKey(strSplitter[0]) == false)
                                {
                                    myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);
                                    /*if (boolCarrigeReturnLF == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r\n", strSplitter[1]);

                                    }
                                    // serialPort1.Write(tx_data.Replace("\\n", Environment.NewLine));
                                    if (boolCarrigeReturn == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r", strSplitter[1]);

                                    }
                                    if (boolCarrigeReturn == false && boolCarrigeReturnLF == false)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);

                                    }*/
                                }
                            }
                            else if (rBtnHex.Checked)
                            {
                                if (strSplitter[0].Length % 2 == 0 && strSplitter[1].Length % 2 == 0)
                                {
                                    string hexKey = "";
                                    string hexValue = "";

                                    //key
                                    for (int i = 0; i < strSplitter[0].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[0].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexKey += s.ToString("X2");
                                        }
                                    }
                                    //value
                                    for (int i = 0; i < strSplitter[1].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[1].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexValue += s.ToString("X2");
                                        }
                                    }

                                    if (myAutoReplyDic.ContainsKey(hexKey) == false)
                                    {
                                        myAutoReplyDic.Add(hexKey, hexValue);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("The file does not contain formatted hex values..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                            }

                        }
                        else
                        {
                            MessageBox.Show("The file does not contain the char splitter between message and reply..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                    }

                    if (myAutoReplyDic.Count > 0)
                    {
                        rBtnEnableAutoReply.Enabled = true;
                        rBtnEnableAutoReply.Checked = true;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error ::" + ex.Message, "Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (rBtnText.Checked)
                {
                    chkBAutoRead.Checked = true;
                    tabControl2.SelectedIndex = 0;
                    checkBoxSendNormal.Checked = true;
                }
                else if (rBtnHex.Checked)
                {
                    chkBAutoReadHex.Checked = true;
                    tabControl2.SelectedIndex = 1;
                    checkBoxSendHex.Checked = true;
                }

                tabControl1.SelectedIndex = 1;
            }

            if (ischeckBox2DECChecked | ischeckBoxSendHexChecked)
            {
                // MessageBox.Show("send is even");
                DecSend = true;
            }
            //  cout << "String length is even" << endl;
            // else
            if (ischeckBox3DECChecked)
            {
                //  MessageBox.Show("send is odd");
                DecSend = false;
            }
            // cout << "String length is odd" 

            //THIS IS USED TO CONVERT THE SEND TEXT TO HEX AND SEND THROUGH THE SERIAL PORT////////
            if (serialPort1.IsOpen && btnRtsOff.Enabled)
                try
                {
                    // SerialEncoding();

                    if (ischeckBoxSendHexChecked == true)
                    {

                        tx_data_p = "info";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                    //this writes decimal to the serial port
                    if (ischeckBoxSendDecChecked == true)
                    {

                        tx_data_p = "info";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }

                    ////this allows access to the value of comboBox1 cross threading
                    if (ischeckBoxSendHexChecked == false && ischeckBoxSendDecChecked == false)
                    {

                        // SerialEncoding();
                        tx_data_p = "info";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                }
                catch (Exception ex)
                {
                    // ComPortClosed();
                    // MessageBox.Show("if sending as hex use hex values example 01 not 1 by itself","Message for a newbie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // richTextBox2.Text = "Unauthorized Access Exception Thrown";
                    MessageBox.Show(ex.Message, "Message ", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }


            if (!serialPort1.IsOpen)
            {

                ComPortClosed();

            }
        }

        private void btnDtrOn_Click(object sender, EventArgs e)
        {
            send_repeat_counter = 0; //put this here as closing the form and reopening the form would dounble up on the send

            sendDataNoError = true;

            // SerialEncoding();
            if (serialPort1.IsOpen)
            {
                myAutoReplyDic = new Dictionary<string, string>();

                try
                {
                    string[] strArr = File.ReadAllLines("at.txt");

                    lblIsAutoFileLoaded.Text = "at.txt";
                    lblAutoReplyLineCount.Text = strArr.Length.ToString();

                    foreach (string str in strArr)
                    {
                        string[] strSplitter = str.Split(autoReplySplitterChar);

                        if (strSplitter.Length == 2)
                        {
                            if (rBtnText.Checked)
                            {
                                if (myAutoReplyDic.ContainsKey(strSplitter[0]) == false)
                                {
                                    myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);
                                    /*if (boolCarrigeReturnLF == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r\n", strSplitter[1]);

                                    }
                                    // serialPort1.Write(tx_data.Replace("\\n", Environment.NewLine));
                                    if (boolCarrigeReturn == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r", strSplitter[1]);

                                    }
                                    if (boolCarrigeReturn == false && boolCarrigeReturnLF == false)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);

                                    }*/
                                }
                            }
                            else if (rBtnHex.Checked)
                            {
                                if (strSplitter[0].Length % 2 == 0 && strSplitter[1].Length % 2 == 0)
                                {
                                    string hexKey = "";
                                    string hexValue = "";

                                    //key
                                    for (int i = 0; i < strSplitter[0].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[0].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexKey += s.ToString("X2");
                                        }
                                    }
                                    //value
                                    for (int i = 0; i < strSplitter[1].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[1].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexValue += s.ToString("X2");
                                        }
                                    }

                                    if (myAutoReplyDic.ContainsKey(hexKey) == false)
                                    {
                                        myAutoReplyDic.Add(hexKey, hexValue);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("The file does not contain formatted hex values..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                            }

                        }
                        else
                        {
                            MessageBox.Show("The file does not contain the char splitter between message and reply..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                    }

                    if (myAutoReplyDic.Count > 0)
                    {
                        rBtnEnableAutoReply.Enabled = true;
                        rBtnEnableAutoReply.Checked = true;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error ::" + ex.Message, "Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (rBtnText.Checked)
                {
                    chkBAutoRead.Checked = true;
                    tabControl2.SelectedIndex = 0;
                    checkBoxSendNormal.Checked = true;
                }
                else if (rBtnHex.Checked)
                {
                    chkBAutoReadHex.Checked = true;
                    tabControl2.SelectedIndex = 1;
                    checkBoxSendHex.Checked = true;
                }

                tabControl1.SelectedIndex = 1;
            }

            if (ischeckBox2DECChecked | ischeckBoxSendHexChecked)
            {
                // MessageBox.Show("send is even");
                DecSend = true;
            }
            //  cout << "String length is even" << endl;
            // else
            if (ischeckBox3DECChecked)
            {
                //  MessageBox.Show("send is odd");
                DecSend = false;
            }
            // cout << "String length is odd" 

            //THIS IS USED TO CONVERT THE SEND TEXT TO HEX AND SEND THROUGH THE SERIAL PORT////////
            if (serialPort1.IsOpen && btnDtrOn.Enabled)
                try
                {
                    // SerialEncoding();

                    if (ischeckBoxSendHexChecked == true)
                    {

                        tx_data_p = "at";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                    //this writes decimal to the serial port
                    if (ischeckBoxSendDecChecked == true)
                    {

                        tx_data_p = "at";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }

                    ////this allows access to the value of comboBox1 cross threading
                    if (ischeckBoxSendHexChecked == false && ischeckBoxSendDecChecked == false)
                    {

                        // SerialEncoding();
                        tx_data_p = "at";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                }
                catch (Exception ex)
                {
                    // ComPortClosed();
                    // MessageBox.Show("if sending as hex use hex values example 01 not 1 by itself","Message for a newbie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // richTextBox2.Text = "Unauthorized Access Exception Thrown";
                    MessageBox.Show(ex.Message, "Message ", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }


            if (!serialPort1.IsOpen)
            {

                ComPortClosed();

            }
        }

        private void btnDtrOff_Click(object sender, EventArgs e)
        {
            send_repeat_counter = 0; //put this here as closing the form and reopening the form would dounble up on the send

            sendDataNoError = true;

            // SerialEncoding();

            if (serialPort1.IsOpen)
            {
                myAutoReplyDic = new Dictionary<string, string>();

                try
                {
                    string[] strArr = File.ReadAllLines("off.txt");

                    lblIsAutoFileLoaded.Text = "off.txt";
                    lblAutoReplyLineCount.Text = strArr.Length.ToString();

                    foreach (string str in strArr)
                    {
                        string[] strSplitter = str.Split(autoReplySplitterChar);

                        if (strSplitter.Length == 2)
                        {
                            if (rBtnText.Checked)
                            {
                                if (myAutoReplyDic.ContainsKey(strSplitter[0]) == false)
                                {
                                    myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);
                                    /*if (boolCarrigeReturnLF == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r\n", strSplitter[1]);

                                    }
                                    // serialPort1.Write(tx_data.Replace("\\n", Environment.NewLine));
                                    if (boolCarrigeReturn == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r", strSplitter[1]);

                                    }
                                    if (boolCarrigeReturn == false && boolCarrigeReturnLF == false)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);

                                    }*/
                                }
                            }
                            else if (rBtnHex.Checked)
                            {
                                if (strSplitter[0].Length % 2 == 0 && strSplitter[1].Length % 2 == 0)
                                {
                                    string hexKey = "";
                                    string hexValue = "";

                                    //key
                                    for (int i = 0; i < strSplitter[0].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[0].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexKey += s.ToString("X2");
                                        }
                                    }
                                    //value
                                    for (int i = 0; i < strSplitter[1].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[1].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexValue += s.ToString("X2");
                                        }
                                    }

                                    if (myAutoReplyDic.ContainsKey(hexKey) == false)
                                    {
                                        myAutoReplyDic.Add(hexKey, hexValue);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("The file does not contain formatted hex values..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                            }

                        }
                        else
                        {
                            MessageBox.Show("The file does not contain the char splitter between message and reply..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                    }

                    if (myAutoReplyDic.Count > 0)
                    {
                        rBtnEnableAutoReply.Enabled = true;
                        rBtnEnableAutoReply.Checked = true;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error ::" + ex.Message, "Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (rBtnText.Checked)
                {
                    chkBAutoRead.Checked = true;
                    tabControl2.SelectedIndex = 0;
                    checkBoxSendNormal.Checked = true;
                }
                else if (rBtnHex.Checked)
                {
                    chkBAutoReadHex.Checked = true;
                    tabControl2.SelectedIndex = 1;
                    checkBoxSendHex.Checked = true;
                }

                tabControl1.SelectedIndex = 1;
            }

            if (ischeckBox2DECChecked | ischeckBoxSendHexChecked)
            {
                // MessageBox.Show("send is even");
                DecSend = true;
            }
            //  cout << "String length is even" << endl;
            // else
            if (ischeckBox3DECChecked)
            {
                //  MessageBox.Show("send is odd");
                DecSend = false;
            }
            // cout << "String length is odd" 

            //THIS IS USED TO CONVERT THE SEND TEXT TO HEX AND SEND THROUGH THE SERIAL PORT////////
            if (serialPort1.IsOpen && btnDtrOff.Enabled)
                try
                {
                    // SerialEncoding();

                    if (ischeckBoxSendHexChecked == true)
                    {

                        tx_data_p = "off";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                    //this writes decimal to the serial port
                    if (ischeckBoxSendDecChecked == true)
                    {

                        tx_data_p = "off";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }

                    ////this allows access to the value of comboBox1 cross threading
                    if (ischeckBoxSendHexChecked == false && ischeckBoxSendDecChecked == false)
                    {

                        // SerialEncoding();
                        tx_data_p = "off";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                }
                catch (Exception ex)
                {
                    // ComPortClosed();
                    // MessageBox.Show("if sending as hex use hex values example 01 not 1 by itself","Message for a newbie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // richTextBox2.Text = "Unauthorized Access Exception Thrown";
                    MessageBox.Show(ex.Message, "Message ", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }

            if (!serialPort1.IsOpen)
            {

                ComPortClosed();

            }
 
        }


        int t = 1;
        int j = 2;
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            sendDataNoError = true; //this is a switch to restart the send function after it being stopped by an error trying to convert and send an invalid hex dec char from the user

            //THIS IS THE METHOD FOR ADDING A SPACE IN THE RICHTEXTBOX SEND IF HEX OR 2DEC IS CHECKED
            if (checkBoxSendHex.Checked)
            {
                int textLen = 0;

                textLen = richTextBoxSend.Text.Length;



                if (textLen > t)
                {
                    t = textLen;
                    richTextBoxSend.AppendText("  ");
                    // textLen = 0;
                    t = t + 3;
                    // t++;
                }


                if (textLen == 0)
                {
                    t = textLen;
                    t++;
                }
            }
            if (checkBoxSendDec.Checked && checkBox2DEC.Checked)
            {
                int textLen = 0;

                textLen = richTextBoxSend.Text.Length;



                if (textLen > t)
                {
                    t = textLen;
                    richTextBoxSend.AppendText("  ");
                    // textLen = 0;
                    t = t + 3;
                    // t++;
                }


                if (textLen == 0)
                {
                    t = textLen;
                    t++;
                }
            }



            if (checkBox3DEC.Checked && checkBoxSendDec.Checked)
            {
                int textLen = richTextBoxSend.Text.Length;



                if (textLen > j)
                {

                    j = textLen;
                    richTextBoxSend.AppendText("  ");
                    // textLen = 0;
                    j = j + 4;
                    // t++;
                    //if (textLen > 0 && Keys.KeyCode == Keys.Back)
                    //{
                    //  richTextBoxSend.Text = richTextBoxSend.Text.Remove(textLen - 1);
                    //    MessageBox.Show("removed 1");
                    //}
                }
                if (textLen == 0)
                {
                    j = textLen;
                    j++;
                    j++;
                }
            }
        }

        private bool TX = false;
        private bool RX = false;


        private void btnEXIT_Click(object sender, EventArgs e)
        {
            serialPort1.Close();

            Form1.ActiveForm.Close();
            Process.GetCurrentProcess().Kill();
        }


        private void timerCTSETC_Tick(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
                try
                {
                 

                    if (TX == true)
                    {
                        textBoxTX.BackColor = Color.Lime;
                        textBoxTX.Text = "TX On";

                        TX = false;
                      

                    }
                    else
                    {
                        textBoxTX.BackColor = Color.LightSkyBlue;
                        textBoxTX.Text = "TX Off";
                    }

                    if (RX == true)
                    {
                        textBoxRX.BackColor = Color.Lime;
                        textBoxRX.Text = "RX On";

                        lblRxSent.Update();
                        lblRxSent.Text = "RX :" + RXcounter;

                        RX = false;

                    }
                    else
                    {
                        textBoxRX.BackColor = Color.LightSkyBlue;
                        textBoxRX.Text = "RX Off";
                    }



                }

                catch (Exception ex)
                {
                    ComPortClosed();
                    MessageBox.Show("This COM Port Closed Unexpectedly . Original error :" + ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }



        }




        //this code gets the value of richtextbox2 and displays it in hex in RTB4
        public string toHex(string inp)
        {
            string outp = string.Empty;
            char[] value = inp.ToCharArray();

            foreach (char L in value)
            {
                int v = Convert.ToInt32(L);

                outp += string.Format("{0:X2}" + "  ", v);

            }

            return outp;

        }
        //this code gets the value of richtextbox2 and displays it in Decimal in RTB3
        public string toDec(string inp2)
        {


            string outp2 = string.Empty;
            char[] value = inp2.ToCharArray();

            foreach (char L in value)
            {

                int v2 = Convert.ToInt32(L);
                //long v2 = Convert.ToInt64(L);

                outp2 += string.Format("{0:000}" + "  ", v2);

            }
            return outp2;
        }
        //hex to ascii
        public static string ConvertHex(string hexString) //>>>>>>>>>>>>>>>>>>new the textbox can only accept 2 characters
        {
            try
            {
                string ascii = string.Empty;

                // hexString = hexString.Replace("  ", ""); //this make sure no white space is trying to get converted

                StringBuilder sb = new StringBuilder(hexString);
                sb.Replace(" ", "");
                sb.Replace("  ", "");
                hexString = sb.ToString();

                for (int i = 0; i < hexString.Length; i += 2)
                {
                    string hs = string.Empty;

                    hs = hexString.Substring(i, 2);
                    //uint decval = Convert.ToUInt32(hs, 16);
                    // char character = Convert.ToChar(decval); 

                    char character = Convert.ToChar(Convert.ToUInt32(hs, 16)); 
                    // string character = decval.ToString("x2");
                    ascii += character;
                    // ascii += decval;

                }

                return ascii;

            }
            catch (Exception ex)
            {

                sendDataNoError = false;
              

                MessageBox.Show("Enter a Valid Hexadecimal Value. Original error :  " + ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);


            }

            return string.Empty;
        }

        //THIS IS FOR SENDING AS DECIMAL
        public static string ConvertDec(string decString) //>>>>>>>>>>>>>>>>>>new the textbox can only accept 2 characters
        {
            int decSpace = 2;

            if (DecSend == true)  //see if the string is divisable by 2
            {
                // MessageBox.Show("2");
                decSpace = 2;
            }
            if (DecSend == false)  //see if the string is divisable by 2
            {
                //  MessageBox.Show("3");
                decSpace = 3;
            }
            //  decString.Replace(" ", "");
            try
            {
                
                string dec = string.Empty;
                
                StringBuilder sb = new StringBuilder(decString);  
                sb.Replace(" ", "");
                sb.Replace("  ", "");
                decString = sb.ToString();


                // for (int i = 0; i < decString.Length; i += 2)
                for (int i = 0; i < decString.Length; i += decSpace)
                //  for (int i = 0; i < decString.Length; i++)
                {
                    string hs = string.Empty;
                    // hs = decString;
                    //  hs = decString.Substring(i, 2);
                    hs = decString.Substring(i, decSpace);
                    uint decval = Convert.ToUInt32(hs);
                    char character = Convert.ToChar(decval);
                    dec += character;
                    // dec += decval;

                }
               

                return dec;
            }


            catch (Exception ex)
            {
                sendDataNoError = false;
              
                MessageBox.Show("Enter a Valid Decimal Value. Original error : " + ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);


            }


            return string.Empty;
        }


        //this code appends the text from richtextbox2 and converts it to hex and decimal
        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

            if (serialPort1.IsOpen && chkBConvertToHexDec.Checked && richTextBoxRecieve.Text != "")
            {
                richTextBoxHex.Clear();
                richTextBoxDec.Clear();

                //string[] separators = { "0A", "0D" };
                string[] hexSeparators = { "0A" };
                string hexValue = toHex(richTextBoxRecieve.Text);
                string[] hexWords = hexValue.Split(hexSeparators, StringSplitOptions.None);
                foreach (var word in hexWords)
                {
                    // Console.WriteLine(word);
                    richTextBoxHex.AppendText(word + "0A" + "\n");
                }


                string[] decSeparators = { "010" };
                string decValue = toDec(richTextBoxRecieve.Text);
                string[] decWords = decValue.Split(decSeparators, StringSplitOptions.None);
                foreach (var word in decWords)
                {
                    // Console.WriteLine(word);
                    richTextBoxDec.AppendText(word + "010" + "\n");
                }

              
            }


        }

        private void button12_Click_1(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
                try
                {
                    serialPort1.DiscardInBuffer();
                    serialPort1.DiscardInBuffer();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Message ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }
        //RIGHT CLICK TO COPY AND PASTE TO CLIPBOARD
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (richTextBoxSend.SelectedText == "" && richTextBoxSend.Text != "")
            {
                Clipboard.SetText(richTextBoxSend.Text);
            }
            if (richTextBoxSend.SelectedText != "")
            {
                Clipboard.SetText(richTextBoxSend.SelectedText);
            }

        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxSend.Text += Clipboard.GetText();
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (richTextBoxRecieve.SelectedText == "" && richTextBoxRecieve.Text != "")
            {
                Clipboard.SetText(richTextBoxRecieve.Text);
            }
            if (richTextBoxRecieve.SelectedText != "")
            {
                Clipboard.SetText(richTextBoxRecieve.SelectedText);
            }

        }



        private void copyToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (richTextBoxHex.SelectedText == "" && richTextBoxHex.Text != "")
            {
                Clipboard.SetText(richTextBoxHex.Text);
            }
            if (richTextBoxHex.SelectedText != "")
            {
                Clipboard.SetText(richTextBoxHex.SelectedText);
            }

        }


        private void contextMenuStripHex_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void copyToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (richTextBoxDec.SelectedText == "" && richTextBoxDec.Text != "")
            {
                Clipboard.SetText(richTextBoxDec.Text);
            }
            if (richTextBoxDec.SelectedText != "")
            {
                Clipboard.SetText(richTextBoxDec.SelectedText);
            }

        }
        //THIS IS USED TO OPEN ONLY ONE INSTANCE OF THE CONVERSION TABLE AND IF ITS CREATED BRING TO FRONT
        //  Conversion_Table conTable = null;
        private Conversion_Table conTable;

        private void btnConversion_Click(object sender, EventArgs e)
        {
            // Conversion_Table conTable = new Conversion_Table();
            if (conTable == null || (conTable != null && conTable.IsDisposed))
            {
                conTable = new Conversion_Table();
                conTable.Show();
                //   MessageBox.Show("disposed");
            }
            if (conTable.WindowState == FormWindowState.Minimized)
            {
                conTable.WindowState = FormWindowState.Normal;
            }
            else
            {
                conTable.BringToFront();
                //  MessageBox.Show("bring to front");
            }

        }
        //SETS THE CHECK STATE AND GIVES FOCUS TO THE SEND BOX AND DOESNT ALLOW KEYBOARD SHORTCUTS WHEN HEX OR DEC OR SUB DEC ARE CHOSEN
        private void checkBoxSendHex_CheckedChanged(object sender, EventArgs e)
        {

            if (checkBoxSendHex.Checked)
            {
                richTextBoxSend.Focus();
                richTextBoxSend.ShortcutsEnabled = false;
                richTextBoxSend.Text = "";
                checkBox3DEC.Checked = false;

                checkBoxAutoSend.Checked = false;
                checkBoxSendDec.Checked = false;
                checkBoxSendNormal.Checked = false;
            }

            if (checkBoxSendHex.Checked)
            {
                ischeckBoxSendHexChecked = true;
            }
            else
            {
                ischeckBoxSendHexChecked = false;
            }
        }

        private void checkBoxAutoSend_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAutoSend.Checked)
            {
                richTextBoxSend.Focus();
                richTextBoxSend.ShortcutsEnabled = false;
                richTextBoxSend.Text = "";

                checkBoxSendHex.Checked = false;
                checkBoxSendDec.Checked = false;
                checkBoxSendNormal.Checked = false;
            }
        }

        private void checkBoxSendDec_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSendDec.Checked)
            {
                richTextBoxSend.Focus();
                richTextBoxSend.ShortcutsEnabled = false;
                richTextBoxSend.Text = "";

                checkBoxSendHex.Checked = false;
                checkBoxAutoSend.Checked = false;
                checkBoxSendNormal.Checked = false;
                checkBox2DEC.Checked = true;
                // checkBoxSendDec.Checked = true;

                ischeckBoxSendDecChecked = true;
            }
            else
            {
                ischeckBoxSendDecChecked = false;
            }
            if (checkBoxSendDec.Checked == false)
            {
                richTextBoxSend.Focus();
                checkBox2DEC.Checked = false;
                checkBox3DEC.Checked = false;
            }
        }
        private void checkBox2DEC_CheckedChanged(object sender, EventArgs e)
        {
            //if (checkBoxSendDec.Checked)
            //{
            //    checkBox2DEC.Checked = true;
            //}
            if (checkBox2DEC.Checked)
            {
                richTextBoxSend.Focus();
                richTextBoxSend.Text = "";

                checkBoxSendDec.Checked = true;
                checkBox3DEC.Checked = false;
                checkBox2DEC.Checked = true;
            }

            if (checkBox2DEC.Checked)
            {
                ischeckBox2DECChecked = true;
            }
            else
            {
                ischeckBox2DECChecked = false;
            }
        }

        private void checkBox3DEC_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3DEC.Checked)
            {
                richTextBoxSend.Focus();
                richTextBoxSend.Text = "";

                checkBoxSendDec.Checked = true;
                checkBox2DEC.Checked = false;
                checkBox3DEC.Checked = true;
            }

            if (checkBox3DEC.Checked)
            {
                ischeckBox3DECChecked = true;
            }
            else
            {
                ischeckBox3DECChecked = false;
            }
        }

        private void checkBoxSendNormal_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSendNormal.Checked)
            {
                richTextBoxSend.Focus();
                richTextBoxSend.ShortcutsEnabled = true;
                richTextBoxSend.Text = "";

                checkBoxSendHex.Checked = false;
                checkBoxSendDec.Checked = false;
                checkBoxAutoSend.Checked = false;
                checkBoxSendNormal.Checked = true;
            }
        }

        private void chkBAutoReadHex_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBAutoReadHex.Checked)

            {
                richTextBoxSend.Focus();
                chkBAutoReadDec.Checked = false;
                chkBAutoRead.Checked = false;
                chkBConvertToHexDec.Checked = false;
            }

            if (chkBAutoReadHex.Checked)
            {
                ischkBAutoReadHexChecked = true;
            }
            else
            {
                ischkBAutoReadHexChecked = false;
            }
        }

        private void chkBAutoReadDec_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBAutoReadDec.Checked)
            {
                richTextBoxSend.Focus();
                chkBAutoRead.Checked = false;
                chkBConvertToHexDec.Checked = false;
                chkBAutoReadHex.Checked = false;
            }


            if (chkBAutoReadDec.Checked)
            {
                ischkBAutoReadDecChecked = true;
            }
            else
            {
                ischkBAutoReadDecChecked = false;
            }
        }

        private void chkBAutoRead_CheckedChanged(object sender, EventArgs e)
        {
            
           

            if (chkBAutoRead.Checked)
            {
                ischkBAutoReadChecked = true;

                chkBAutoReadHex.Checked = false;
                chkBAutoReadDec.Checked = false;
            }
            else
            {
                ischkBAutoReadChecked = false;
            }

        }

        private void chkBConvertToHexDec_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBConvertToHexDec.Checked)
            {
                chkBAutoRead.Checked = true;
                chkBAutoRead.Enabled = false;
            }
            else
            {
                chkBAutoRead.Enabled = true;
            }

            if (chkBAutoRead.Checked)
            {
                chkBAutoReadHex.Checked = false;
                chkBAutoReadDec.Checked = false;
            }



        }

       
        //this is to limit the size of the the text captured in the richtextboxes
        bool stopMessage = false;


        //TICK EVENT FOR MAX CHARACTERS RECIEVED

        private async void timerTextLenght_Tick(object sender, EventArgs e)
        {
            if (ckBEnableLogs.Checked && richTextBoxRecieve.Lines.Length > 20)
            {
                // richTextBoxRecieve.Lines.Skip(10);
                richTextBoxRecieve.Text = richTextBoxRecieve.Text.Remove(0, richTextBoxRecieve.Lines.Length);
                // richTextBoxRecieve.Clear();

            }
            if (ckBEnableLogs.Checked && richTextBoxHex.Lines.Length > 20)
            {

                richTextBoxHex.Text = richTextBoxHex.Text.Remove(0, richTextBoxHex.Lines.Length);
                // richTextBoxHex.Clear();

            }
            if (ckBEnableLogs.Checked && richTextBoxDec.Lines.Length > 20)
            {
                richTextBoxDec.Text = richTextBoxDec.Text.Remove(0, richTextBoxDec.Lines.Length);
                //  richTextBoxDec.Clear();
            }
            // if (richTextBoxRecieve.Text.Length > 10000 | richTextBoxHex.Text.Length > 10000 | richTextBoxDec.Text.Length > 10000 && stopMessage == false)
            if (richTextBoxRecieve.Lines.Length > 5000 | richTextBoxHex.Lines.Length > 5000 | richTextBoxDec.Lines.Length > 5000 && stopMessage == false)
            {
                //stop capturing the inbound data
                chkBAutoRead.Checked = false;
                chkBAutoReadDec.Checked = false;
                chkBAutoReadHex.Checked = false;

                stopMessage = true;
                DialogResult messageBoxResult = MessageBox.Show("Capturing Data for too long , Choose YES to Save or NO to Clear", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (messageBoxResult == DialogResult.Yes)
                {
                    SaveFileDialog sfd = new SaveFileDialog();

                    sfd.Title = "Save To a File";
                    sfd.Filter = "text files (*.txt)|*.txt|Richtext files (*.rtf)|*.rtf";


                    string recieve = string.Empty;
                    recieve = richTextBoxRecieve.Text;

                    string hexRecieve = string.Empty;
                    hexRecieve = richTextBoxHex.Text;

                    string decRecieve = string.Empty;
                    decRecieve = richTextBoxDec.Text;



                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {

                            using (StreamWriter sw = new StreamWriter(sfd.FileName, true))
                            {

                                await sw.WriteLineAsync($"{"Normal Encoding : "}{"\n"}{recieve} {"\n\n"}{"Hex : "}{"\n"} {hexRecieve}{"\n\n"}{"Dec : "} {"\n"}{decRecieve}{"\n\n"} { "---------------------"} { DateTime.Now.ToString() }");

                                sw.Close();

                            }

                        }

                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: Could not write file. Original error: " + ex.Message);
                        }
                    }

                    richTextBoxRecieve.Clear();
                    richTextBoxDec.Clear();
                    richTextBoxHex.Clear();
                    RXcounter = 0;
                    TXcounter = 0;

                    lblRxSent.Text = "RX :" + RXcounter;
                    lblTxSent.Text = "TX :" + TXcounter;
                    stopMessage = false; //switch

                }
                if (messageBoxResult == DialogResult.No)
                {
                    //if(richTextBoxRecieve.Text.Length > 1000 | richTextBoxHex.Text.Length > 1000 | richTextBoxDec.Text.Length > 1000)
                    //{
                    richTextBoxRecieve.Clear();
                    richTextBoxDec.Clear();
                    richTextBoxHex.Clear();

                    RXcounter = 0;
                    TXcounter = 0;

                    lblRxSent.Text = "RX :" + RXcounter;
                    lblTxSent.Text = "TX :" + TXcounter;
                    // }

                    stopMessage = false;
                }

            }
        }
        //these are make sure that the only keys allowed are numbers/digits or backspace or delete key
        private void comboBoxBaudRate_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;

            if (!char.IsDigit(ch) && ch != 8 && ch != 46 && ch != 110)
            {
                e.Handled = true;
                MessageBox.Show("Please enter numbers only");


            }
        }

        private void comboBoxWriteTimeout_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;

            if (!char.IsDigit(ch) && ch != 8 && ch != 46 && ch != 110)
            {
                e.Handled = true;
                MessageBox.Show("Please enter numbers only");


            }
        }

        private void comboBoxReadTimeout_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;

            if (!char.IsDigit(ch) && ch != 8 && ch != 46 && ch != 110)
            {
                e.Handled = true;
                MessageBox.Show("Please enter numbers only");


            }
        }
        //THIS IS THE HANDLER FOR SURPRESSING THE DELETE KEY AND BACKSPACE KEY ON THE SEND BOX
        private void richTextBoxSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (ischeckBoxSendHexChecked || ischeckBoxSendDecChecked)
            {
                if (richTextBoxSend.Text != "")
                {
                    //SURPRESS THE USE OF THE DELETE KEY
                    if (e.KeyCode == Keys.Delete)
                    {
                        // t--;

                        // MessageBox.Show("minus t");
                        e.SuppressKeyPress = true;

                    }
                    string checkStringHEX = string.Empty;
                    checkStringHEX = richTextBoxSend.Text;


                    //if (e.KeyCode == Keys.Back && richTextBoxRecieve.Text.Substring(richTextBoxRecieve.Text.Length).Contains(" ") == true )
                    // if (e.KeyCode == Keys.Back && checkString.TrimEnd() == " ") //&& checkString.Trim().EndsWith(check) == true) 

                    //THIS WILL CHECK TO SEE IF THE END OF THE STRING DOESNT CONTAIN A SPACE AND THEN APPEND IT
                    if (e.KeyCode == Keys.Back && checkStringHEX.EndsWith(" ") == false)
                    {

                        t--;
                        t--;

                    }
                    if (e.KeyCode == Keys.Back)
                    {

                        // e.SuppressKeyPress = true;
                        t--;

                    }
                }
            }

            if (ischeckBox3DECChecked && ischeckBoxSendDecChecked && richTextBoxSend.Text != "")
            {
                string checkStringDEC = string.Empty;
                checkStringDEC = richTextBoxSend.Text;

                if (e.KeyCode == Keys.Delete)
                {
                    // j--;
                    e.SuppressKeyPress = true;

                }
                if (e.KeyCode == Keys.Back && checkStringDEC.EndsWith("  ") == false)
                {
                    // j = j - 3;
                    j--;
                    j--;
                }
                if (e.KeyCode == Keys.Back)
                {
                    // e.SuppressKeyPress = true;
                    j--;


                }
            }

        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            richTextBoxSend.Clear();
            richTextBoxSend.Focus();
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save Current Readings in a File";
            sfd.Filter = "text files (*.txt)|*.txt|Richtext files (*.rtf)|*.rtf";

            string recieve = string.Empty;
            recieve = richTextBoxRecieve.Text;

            string hexRecieve = string.Empty;
            hexRecieve = richTextBoxHex.Text;

            string decRecieve = string.Empty;
            decRecieve = richTextBoxDec.Text;



            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {

                    using (StreamWriter sw = new StreamWriter(sfd.FileName, true))
                    {

                        await sw.WriteLineAsync($"{"Normal Encoding :"}{"\n"}{recieve} {"\n\n"}{"Hex :"}{"\n"} {hexRecieve}{"\n\n"}{"Dec :"} {"\n"}{decRecieve}{"\n\n"} { "---------------------"} { DateTime.Now.ToString() }");

                        sw.Close();

                    }

                }

                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file. Original error: " + ex.Message);
                }
            }

            richTextBoxRecieve.Clear();
            richTextBoxDec.Clear();
            richTextBoxHex.Clear();
            stopMessage = false; //switch

        }

        private void richTextBoxSend_SelectionChanged(object sender, EventArgs e)
        {

        }
        //THIS IS USED TO REMOVE THE SELECTED TEXT HIGHLIGHTED BY THE USER IF HEX OR DEC IS ENABLED
        private void richTextBoxSend_MouseUp(object sender, MouseEventArgs e)
        {

            //this will unselect the mouse selected text and remove the mouse patse function or enable it if hex or dec is not selected
            if (ischeckBoxSendHexChecked || ischeckBoxSendDecChecked)
            {
                //this sets the cursor position to the end of the text
                richTextBoxSend.Select(richTextBoxSend.Text.Length, 0);

                RemoveSelection(sender);

                pasteToolStripMenuItem.Enabled = false;
            }
            if (!ischeckBoxSendHexChecked && !ischeckBoxSendDecChecked)
            {
                pasteToolStripMenuItem.Enabled = true;
            }
        }
        private void RemoveSelection(Object obj)
        {
            RichTextBox textbox = obj as RichTextBox;
            if (textbox != null)
            {
                textbox.SelectionLength = 0;
            }
        }

        private void contextMenuStripSend_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void checkBoxSendDec_Click(object sender, EventArgs e)
        {

            checkBoxSendDec.Checked = true;
            t = 1;
            richTextBoxSend.Focus();
        }

        private void checkBox2DEC_Click(object sender, EventArgs e)
        {
            checkBox2DEC.Checked = true;
            richTextBoxSend.Focus();
        }

        private void checkBox3DEC_Click(object sender, EventArgs e)
        {
            checkBox3DEC.Checked = true;
            richTextBoxSend.Focus();
        }

        private void checkBoxSendHex_Click(object sender, EventArgs e)
        {
            checkBoxSendHex.Checked = true;
            richTextBoxSend.Focus();
        }

        private void checkBoxSendNormal_Click(object sender, EventArgs e)
        {
            checkBoxSendNormal.Checked = true;
            richTextBoxSend.Focus();
        }

        private void checkBoxAutoSend_Click(object sender, EventArgs e)
        {
            checkBoxAutoSend.Checked = true;
            richTextBoxSend.Focus();
        }

        private void btnClearRecieve_Click(object sender, EventArgs e)
        {
            richTextBoxRecieve.Clear();
            richTextBoxSend.Focus();
        }

        private void btnClearRecieveDec_Click(object sender, EventArgs e)
        {
            richTextBoxDec.Clear();
            richTextBoxSend.Focus();
        }

        private void btnClearRecieveHex_Click(object sender, EventArgs e)
        {
            richTextBoxHex.Clear();
            richTextBoxSend.Focus();
        }

        private void btnClearInOutBuff_Click(object sender, EventArgs e)
        {

            if (serialPort1.IsOpen)
            {
                serialPort1.DiscardInBuffer();
                serialPort1.DiscardOutBuffer();
            }
            richTextBoxSend.Focus();
        }

        private void checkBoxBringToFront_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxBringToFront.Checked == true)
            {
                TopMost = true;
            }
            if (checkBoxBringToFront.Checked == false)
            {
                TopMost = false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Close();
                    serialPort1.DiscardInBuffer();
                    serialPort1.DiscardOutBuffer();
                }
                catch {/*ignore*/}
            }
        }
        //this will append the send data with a carrige return or not
        private bool boolCarrigeReturnLF = true;
        private void checkBoxCRLF_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCRLF.Checked)
            {
                checkBoxCR.Checked = false;
                boolCarrigeReturnLF = true;
            }
            if (checkBoxCRLF.Checked == false)
            {
                boolCarrigeReturnLF = false;
            }
        }
        private bool boolCarrigeReturn = false;
        private void checkBoxCR_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCR.Checked)
            {
                checkBoxCRLF.Checked = false;
                boolCarrigeReturn = true;
            }
            if (checkBoxCR.Checked == false)
            {
                boolCarrigeReturn = false;
            }
        }

        private void btnSendBreak_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                int v2 = Convert.ToInt32("3");

                string sendBreak = string.Format("{0:X2}", v2);
                serialPort1.Write(sendBreak);
            }
        }
        //font selection and colours
        private void btnFont_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowColor = true;

            fontDialog1.Font = richTextBoxRecieve.Font;
            fontDialog1.Color = richTextBoxRecieve.ForeColor;

            if (fontDialog1.ShowDialog() != DialogResult.Cancel)
            {
                richTextBoxRecieve.Font = fontDialog1.Font;
                richTextBoxRecieve.ForeColor = fontDialog1.Color;

                richTextBoxSend.Font = fontDialog1.Font;
                richTextBoxSend.ForeColor = fontDialog1.Color;

                richTextBoxHex.Font = fontDialog1.Font;
                richTextBoxHex.ForeColor = fontDialog1.Color;

                richTextBoxDec.Font = fontDialog1.Font;
                richTextBoxDec.ForeColor = fontDialog1.Color;
            }
        }

        private string inputString = "";
        private async void btnSendFile_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "text files (*.txt)|*.txt|Richtext files (*.rtf)|*.rtf";


                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamReader sw = new StreamReader(ofd.OpenFile(), true))
                        {

                            inputString = await (sw.ReadToEndAsync());

                            richTextBoxSend.Text += inputString;

                        }

                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show(ex.Message);
                    }


                }

            }
        }

        private void btnStopSendFile_Click(object sender, EventArgs e)
        {
            sendDataNoError = false;
        }

        private void ckBEnableLogs_CheckedChanged(object sender, EventArgs e)
        {
            //if (ckBEnableLogs.Checked)
            //{
            //    OpenFileDialog ofd = new OpenFileDialog();

            //    ofd.Title = "Enable Logging To a File";
            //    ofd.Filter = "text files (*.txt)|*.txt|Richtext files (*.rtf)|*.rtf";

            //    if (ofd.ShowDialog() == DialogResult.OK)
            //    {
            //        lblDataLogFilePath.Text = ofd.FileName;

            //    }
            //    else
            //    {
            //        ckBEnableLogs.Checked = false;
            //    }
            //}  if (ckBEnableLogs.Checked)
            //if (ckBEnableLogs.Checked)
            //{
            //    SaveFileDialog sfd = new SaveFileDialog();

            //    sfd.Title = "Enable Logging To a File";
            //    sfd.Filter = "text files (*.txt)|*.txt|Richtext files (*.rtf)|*.rtf";

            //    if (sfd.ShowDialog() == DialogResult.OK)
            //    {
            //        lblDataLogFilePath.Text = sfd.FileName;

            //    }
            //    if (sfd.ShowDialog() == DialogResult.Cancel)
            //    {
            //        ckBEnableLogs.Checked = false;
            //    }
            //}

        }

        private void ckBAppendLogs_CheckedChanged(object sender, EventArgs e)
        {
            if (ckBAppendLogs.Checked)
            {
                ckBOverwriteLogs.Checked = false;
            }
        }

        private void ckBOverwriteLogs_CheckedChanged(object sender, EventArgs e)
        {
            if (ckBOverwriteLogs.Checked)
            {
                ckBAppendLogs.Checked = false;
            }
        }

        //This adds points to the chart graph
        private void GetChartPoint(int input)
        {
            lblCountRX.Update();
            lblCountRX.Text = rXChartCount.ToString();

            lblCountTX.Update();
            lblCountTX.Text = tXChartCount.ToString();

            for (int i = 0; i < rXChartCount; i++)
            {
                chart1.Series["SeriesRX"].Points.AddY(rXChartCount);
                chart1.Series["SeriesRX"].Points.RemoveAt(0);
                chart1.Series["SeriesRX"].Points.Add(rXChartCount);



            }

            for (int i = 0; i < tXChartCount; i++)
            {

                chart1.Series["SeriesTX"].Points.AddY(tXChartCount);
                chart1.Series["SeriesTX"].Points.RemoveAt(0);
                chart1.Series["SeriesTX"].Points.Add(tXChartCount);
            }
            //just keep the chart moving with added value of zero if no counts available
            if (rXChartCount == 0)
            {
                chart1.Series["SeriesRX"].Points.AddY(0);
                chart1.Series["SeriesRX"].Points.RemoveAt(0);
                chart1.Series["SeriesRX"].Points.Add(0);


            }
            if (tXChartCount == 0)
            {
                chart1.Series["SeriesTX"].Points.AddY(0);
                chart1.Series["SeriesTX"].Points.RemoveAt(0);
                chart1.Series["SeriesTX"].Points.Add(0);
            }

            chart1.ResetAutoValues();

        }

        private void ckBStartGraph_CheckedChanged(object sender, EventArgs e)
        {
            if (ckBStartGraph.Checked)
            {
                if (serialPort1.IsOpen)
                {
                    timerGraph.Interval = (int)graph_speed.Value * 1000;
                    timerGraph.Start();

                    graph_speed.Enabled = false;

                }


            }
            if (ckBStartGraph.Checked == false)
            {
                timerGraph.Stop();
                graph_speed.Enabled = true;
            }
        }

        private void timerGraph_Tick(object sender, EventArgs e)
        {
            GetChartPoint(rXChartCount);

            rXChartCount = 0;
            tXChartCount = 0;
        }
        //THIS IS USED TO OPEN ONLY ONE INSTANCE OF THE CONVERSION TABLE AND IF ITS CREATED BRING TO FRONT
        //  Conversion_Table conTable = null;
        private ChecksumCalc crcTable;
        private void button1_Click(object sender, EventArgs e)
        {
            // Conversion_Table conTable = new Conversion_Table();
            if (crcTable == null || (crcTable != null && crcTable.IsDisposed))
            {
                crcTable = new ChecksumCalc();
                crcTable.Show();
                //   MessageBox.Show("disposed");
            }
            if (crcTable.WindowState == FormWindowState.Minimized)
            {
                crcTable.WindowState = FormWindowState.Normal;
            }
            else
            {
                crcTable.BringToFront();
                //  MessageBox.Show("bring to front");
            }
        }

        private void ckBEnableLogs_CheckStateChanged(object sender, EventArgs e)
        {
            if (ckBEnableLogs.Checked)
            {
                SaveFileDialog sfd = new SaveFileDialog();

                sfd.Title = "Enable Logging To a File";
                sfd.Filter = "text files (*.txt)|*.txt|Richtext files (*.rtf)|*.rtf";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    lblDataLogFilePath.Text = sfd.FileName;

                }
                else
                {
                    ckBEnableLogs.Checked = false;
                }
            }
        }

        private void txRepeaterDelay_Tick(object sender, EventArgs e)
        {

        }

        private void btnChooseFileAutoReply_Click(object sender, EventArgs e)
        {

           

            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "(txt files)|*.txt|(rtf files)|*.rtf";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                myAutoReplyDic = new Dictionary<string, string>();



                try
                {
                    string[] strArr = File.ReadAllLines(ofd.FileName);

                    lblIsAutoFileLoaded.Text = ofd.SafeFileName;
                    lblAutoReplyLineCount.Text = strArr.Length.ToString();

                    foreach (string str in strArr)
                    {
                        string[] strSplitter = str.Split(autoReplySplitterChar);

                        if (strSplitter.Length == 2)
                        {
                            if(rBtnText.Checked)
                            {
                                if (myAutoReplyDic.ContainsKey(strSplitter[0]) == false)
                                {
                                    myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);
                                }
                            }
                            else if(rBtnHex.Checked)
                            {
                               if(strSplitter[0].Length % 2 == 0 && strSplitter[1].Length %2 == 0)
                                {
                                    string hexKey = "";
                                    string hexValue = "";

                                    //key
                                    for (int i = 0; i < strSplitter[0].Length; i++)
                                    {
                                      
                                        int s;
                                        bool isHex = int.TryParse(strSplitter[0].Substring(i, 2) , System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexKey += s.ToString("X2");
                                        }
                                    }
                                    //value
                                    for (int i = 0; i < strSplitter[1].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[1].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexValue += s.ToString("X2");
                                        }
                                    }

                                    if (myAutoReplyDic.ContainsKey(hexKey) == false)
                                    {
                                        myAutoReplyDic.Add(hexKey, hexValue);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("The file does not contain formatted hex values..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                            }
                            
                        }
                        else
                        {
                            MessageBox.Show("The file does not contain the char splitter between message and reply..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                    }

                    if(myAutoReplyDic.Count > 0)
                    {
                        rBtnEnableAutoReply.Enabled = true;
                        rBtnEnableAutoReply.Checked = true;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error ::" + ex.Message, "Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (rBtnText.Checked)
                {
                    chkBAutoRead.Checked = true;
                    tabControl2.SelectedIndex = 0;
                    checkBoxSendNormal.Checked = true;
                }
                else if (rBtnHex.Checked)
                {
                    chkBAutoReadHex.Checked = true;
                    tabControl2.SelectedIndex = 1;
                    checkBoxSendHex.Checked = true;
                }

                tabControl1.SelectedIndex = 1;
            }
        }

        private void txtBFileReplySplitter_TextChanged(object sender, EventArgs e)
        {
            if (txtBFileReplySplitter.Text != "")
            {
                autoReplySplitterChar = Convert.ToChar(txtBFileReplySplitter.Text);
            }
            //else
            //{
            //    txtBFileReplySplitter.Text = ",";
            //    autoReplySplitterChar = ',';
            //}
        }

        private void lblAutoReplyActive_TextChanged(object sender, EventArgs e)
        {
            if (lblAutoReplyActive.Text == "Active")
            {
                lblAutoReplyActive.BackColor = Color.Lime;
            }
            else
            {
                lblAutoReplyActive.BackColor = Color.White;
            }
        }

        private void rBtnDisableAutoReply_CheckedChanged(object sender, EventArgs e)
        {
            if (rBtnDisableAutoReply.Checked)
            {
                lblAutoReplyActive.Text = "inactive";
                isAutoReplyEnabled = false;
            }
        }

        private void rBtnEnableAutoReply_CheckedChanged(object sender, EventArgs e)
        {
            if (rBtnEnableAutoReply.Checked)
            {
                lblAutoReplyActive.Text = "Active";

                isAutoReplyEnabled = true;

               
            }
        }

        private void comboBoxDecodeFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            comBoBoxformatText = comboBoxDecodeFormat.Text;
        }

        private void bgwAutoReply_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
           

            System.Threading.Thread.Sleep(rxDelayTime);

           
        }

        private void lblIsFileLoaded(string aReplyMessage)
        {
              
            if (aReplyMessage == "wifi")
            {
                lblIsAutoFileLoaded.Text = "wifi.txt";
            }
            else if (aReplyMessage == "bt")
            {
                lblIsAutoFileLoaded.Text = "bt.txt";
            }
            else if (aReplyMessage == "key")
            {
                lblIsAutoFileLoaded.Text = "key.txt";
            }
            else if (aReplyMessage == "at")
            {
                lblIsAutoFileLoaded.Text = "at.txt";
            }
            else if (aReplyMessage == "mic")
            {
                lblIsAutoFileLoaded.Text = "mic.txt";
            }
            else if (aReplyMessage == "info")
            {
                lblIsAutoFileLoaded.Text = "info.txt";
            }
            else if (aReplyMessage == "off")
            {
                lblIsAutoFileLoaded.Text = "off.txt";
            }
            else if (aReplyMessage == "start")
            {
                lblIsAutoFileLoaded.Text = "start.txt";
            }
        }

        private void bgwAutoReply_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
         
            string serialReadString = "";
            string getAll = "";

            try
            {

                int countBytes = serialPort1.BytesToRead;
                byte[] myBytesToRead = new byte[countBytes];
                serialPort1.Read(myBytesToRead, 0, countBytes);

                getAll = serialPortEncoding.GetString(myBytesToRead);
                serialReadString = getAll;

                rXChartCount++; //update the samples per interval
                RX = true;
                RXcounter++;

              

                // serialReadString = serialPort1.ReadExisting();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }

            if (isAutoReplyEnabled == true)
            {
                //start a new task i think to read through possible replys if there is lots of possible replys
                //only update the search string if the background worker is complted the search and reply
               
                autoReplyStringToMatch = getAll;

              //  Console.WriteLine("autostring= " + autoReplyStringToMatch);
               
            }


            //this code displays the hex values to richtextbox4 you cannot directly call data from the port as its a seperate thread
            if (serialPort1.IsOpen && ischkBAutoReadHexChecked)
                try
                {
                    //this is the original ////////////////////////////////
                    // richbox4string = serialPort1.ReadByte().ToString("X2") + "  ";


                    richboxHexString = toHex(serialReadString); //>>>>>>>>>>>>>>>>>i changed this 2017
                                                                // richbox4string = ConvertHex(serialPort1.ReadExisting());
                    Invoke(new EventHandler(DisplayAutoHex));

                     

                    autoReplyStringToMatch = richboxHexString.Replace(" ", "");

                   autoReplyMessage = "";

                    if (myAutoReplyDic != null && isAutoReplyEnabled && autoReplyStringToMatch != "")
                    {
                        foreach (KeyValuePair<string, string> dic in myAutoReplyDic)
                        {
                            if (autoReplyStringToMatch == dic.Key)
                            {
                                autoReplyMessage = ConvertHex(dic.Value);
                                break;
                            }

                        }
                    }

                    //  RX = true;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            //this code gets the decimal value and displays it to richtextbox3
            else if (serialPort1.IsOpen && ischkBAutoReadDecChecked)
                try
                {
                    //this is the original
                    //  richbox3string = serialPort1.ReadByte().ToString("") + "  ";

                    richboxDecString = toDec(serialReadString);

                    this.Invoke(new EventHandler(DisplayAutoDec));



                    //   RX = true;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            //this checks first to see if the port is open and the checked box is checked to read existing serial data
            else if (serialPort1.IsOpen && ischkBAutoReadChecked && ischkBAutoReadDecChecked == false && ischkBAutoReadHexChecked == false)
            {
                try
                {

                    // SerialEncoding();



                    richbox2string = serialReadString;



                    this.Invoke(new EventHandler(DisplayAutoText));


                    autoReplyMessage = "";

                    if (myAutoReplyDic != null && isAutoReplyEnabled && autoReplyStringToMatch != "")
                    {
                        foreach (KeyValuePair<string, string> dic in myAutoReplyDic)
                        {
                            //if (autoReplyStringToMatch == dic.Key)
                            MatchCollection Matches = Regex.Matches(autoReplyStringToMatch, dic.Key, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                            if(Matches.Count>0)
                            {
                                autoReplyMessage = dic.Value;
                                myAutoReplyDic.Remove(dic.Key);

                                if (myAutoReplyDic.Count == 0 || autoReplyMessage.Substring(0,1) == "/")
                                {
                                    if (lblIsAutoFileLoaded.Text == "wifi.txt") { 
                                        textBoxCD.Text = "test ok";
                                        textBoxCD.BackColor = Color.Green;
                                    }
                                    else if (lblIsAutoFileLoaded.Text == "bt.txt")
                                    {
                                        textBoxDSR.Text = "test ok";
                                        textBoxDSR.BackColor = Color.Green;
                                    }
                                    else if (lblIsAutoFileLoaded.Text == "key.txt")
                                    {
                                        textBoxCTS.Text = "test ok";
                                        textBoxCTS.BackColor = Color.Green;
                                    }
                                    else if (lblIsAutoFileLoaded.Text == "at.txt")
                                    {
                                        textBoxRI.Text = "test ok";
                                        textBoxRI.BackColor = Color.Green;
                                    }
                                    else if (lblIsAutoFileLoaded.Text == "mic.txt")
                                    {
                                        textBoxBI.Text = "test ok";
                                        textBoxBI.BackColor = Color.Green;
                                    }
                                    else if (lblIsAutoFileLoaded.Text == "info.txt")
                                    {
                                        textBoxRTS.Text = "test ok";
                                        textBoxRTS.BackColor = Color.Green;
                                    }
                                    else if (lblIsAutoFileLoaded.Text == "off.txt")
                                    {
                                        textBoxDTR.Text = "test ok";
                                        textBoxDTR.BackColor = Color.Green;
                                    }
                                    else if (lblIsAutoFileLoaded.Text == "start.txt")
                                    {
                                        button8.Enabled = true;
                                        button9.Enabled = true;
                                        button7.BackColor = Color.Green;
                                    }
                                }
                                else {
                                    if (lblIsAutoFileLoaded.Text == "wifi.txt")
                                    {
                                        textBoxCD.Text = "testing";
                                        textBoxCD.BackColor = Color.LightSkyBlue;
                                    }
                                    else if (lblIsAutoFileLoaded.Text == "bt.txt")
                                    {
                                        textBoxDSR.Text = "testing";
                                        textBoxDSR.BackColor = Color.LightSkyBlue;
                                    }
                                    else if (lblIsAutoFileLoaded.Text == "key.txt")
                                    {
                                        textBoxCTS.Text = "testing";
                                        textBoxCTS.BackColor = Color.LightSkyBlue;
                                    }
                                    else if (lblIsAutoFileLoaded.Text == "at.txt")
                                    {
                                        textBoxRI.Text = "testing";
                                        textBoxRI.BackColor = Color.LightSkyBlue;
                                    }
                                    else if (lblIsAutoFileLoaded.Text == "mic.txt")
                                    {
                                        textBoxBI.Text = "testing";
                                        textBoxBI.BackColor = Color.LightSkyBlue;
                                    }
                                    else if (lblIsAutoFileLoaded.Text == "info.txt")
                                    {
                                        textBoxRTS.Text = "testing";
                                        textBoxRTS.BackColor = Color.LightSkyBlue;
                                    }
                                    else if (lblIsAutoFileLoaded.Text == "off.txt")
                                    {
                                        textBoxDTR.Text = "testing";
                                        textBoxDTR.BackColor = Color.LightSkyBlue;
                                    }
                                    else if (lblIsAutoFileLoaded.Text == "start.txt")
                                    {
                                        button8.Enabled = false;
                                        button9.Enabled = false;
                                        button7.BackColor = Color.LightSkyBlue;
                                    }
                                }
                                break;
                            }

                        }
                    }


                    //   RX = true;

                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message, "Message ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (!serialPort1.IsOpen && ischkBAutoReadChecked)
                {

                    ComPortClosed();

                }

            }



            // await Task.Delay(100);
            //RX = false;

           



            //==================send the auto reply message


            if (autoReplyMessage != "")
            {
                if (serialPort1.IsOpen)
                {
                    try
                    {
                        //serialPort1.Write(autoReplyMessage);
                        if (boolCarrigeReturnLF == true)
                        {
                            if (autoReplyMessage.Substring(0, 1) == "/")
                            {
                                serialPort1.Write("/\r\n");
                                string tmp = autoReplyMessage.Remove(0, 1); //autoReplyMessage.Substring(autoReplyMessage.Length-1);
                                serialPort1.Write(tmp + "\r\n");
                                lblIsFileLoaded(tmp);
                            }
                            else 
                            { 
                                serialPort1.Write(autoReplyMessage + "\r\n");
                            }

                        }
                        // serialPort1.Write(tx_data.Replace("\\n", Environment.NewLine));
                        if (boolCarrigeReturn == true)
                        {
                            if (autoReplyMessage.Substring(0, 1) == "/")
                            {
                                serialPort1.Write("/\r");
                                string tmp = autoReplyMessage.Remove(0, 1); //autoReplyMessage.Substring(autoReplyMessage.Length-1);
                                serialPort1.Write(tmp + "\r");
                                lblIsFileLoaded(tmp);
                            }
                            else
                            {
                                serialPort1.Write(autoReplyMessage + "\r");
                            }

                        }
                        if (boolCarrigeReturn == false && boolCarrigeReturnLF == false)
                        {
                            if (autoReplyMessage.Substring(0, 1) == "/")
                            {
                                serialPort1.Write("/");
                                string tmp = autoReplyMessage.Remove(0, 1); //autoReplyMessage.Substring(autoReplyMessage.Length-1);
                                serialPort1.Write(tmp);
                                lblIsFileLoaded(tmp);
                            }
                            else
                            {
                                serialPort1.Write(autoReplyMessage);
                            }

                        }

                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show(ex.Message, "Message ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

           
        }

        private void serialPort1_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
           
            if (e.EventType == SerialPinChange.Ring)
            {
                if(textBoxRI.InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        try
                        {
                            if (textBoxRI.BackColor == Color.Lime)
                            {
                                textBoxRI.BackColor = Color.LightSkyBlue;
                            }
                            else
                            {
                                textBoxRI.BackColor = Color.Lime;
                            }
                        }
                        catch (Exception)
                        {

                           
                        }
                    });
                }
                else
                {
                    try
                    {
                        if (textBoxRI.BackColor == Color.Lime)
                        {
                            textBoxRI.BackColor = Color.LightSkyBlue;
                        }
                        else
                        {
                            textBoxRI.BackColor = Color.Lime;
                        }
                    }
                    catch (Exception)
                    {

                       
                    }
                   
                }
               
            }
            else if (e.EventType == SerialPinChange.DsrChanged)
            {
                if (textBoxDSR.InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        try
                        {
                            if (textBoxDSR.BackColor == Color.Lime)
                            {
                                textBoxDSR.BackColor = Color.LightSkyBlue;
                                textBoxDSR.Text = "no test";
                            }
                            else
                            {
                                textBoxDSR.BackColor = Color.Lime;
                                textBoxDSR.Text = "Dsr On";
                            }
                        }
                        catch (Exception)
                        {


                        }
                    });
                }
                else
                {
                    try
                    {
                        if (textBoxDSR.BackColor == Color.Lime)
                        {
                            textBoxDSR.BackColor = Color.LightSkyBlue;
                            textBoxDSR.Text = "no test";
                        }
                        else
                        {
                            textBoxDSR.BackColor = Color.Lime;
                            textBoxDSR.Text = "Dsr On";
                        }
                    }
                    catch (Exception)
                    {


                    }

                }
               
            }
            else if (e.EventType == SerialPinChange.CDChanged)
            {
                if (textBoxCD.InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        try
                        {
                            if (textBoxCD.BackColor == Color.Lime)
                            {
                                textBoxCD.BackColor = Color.LightSkyBlue;
                                textBoxCD.Text = "no test";
                            }
                            else
                            {
                                textBoxCD.BackColor = Color.Lime;
                                textBoxCD.Text = "test ok";
                            }
                        }
                        catch (Exception)
                        {


                        }
                    });
                }
                else
                {
                    try
                    {
                        if (textBoxCD.BackColor == Color.Lime)
                        {
                            textBoxCD.BackColor = Color.LightSkyBlue;
                            textBoxCD.Text = "no test";
                        }
                        else
                        {
                            textBoxCD.BackColor = Color.Lime;
                            textBoxCD.Text = "test ok";
                        }
                    }
                    catch (Exception)
                    {


                    }

                }
               
            }
            else if (e.EventType == SerialPinChange.CtsChanged)
            {
                if (textBoxCTS.InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        try
                        {
                            if (textBoxCTS.BackColor == Color.Lime)
                            {
                                textBoxCTS.BackColor = Color.LightSkyBlue;
                                textBoxCTS.Text = "no test";
                            }
                            else
                            {
                                textBoxCTS.BackColor = Color.Lime;
                                textBoxCTS.Text = "TEST OK";
                            }
                        }
                        catch (Exception)
                        {


                        }
                    });
                }
                else
                {
                    try
                    {
                        if (textBoxCTS.BackColor == Color.Lime)
                        {
                            textBoxCTS.BackColor = Color.LightSkyBlue;
                            textBoxCTS.Text = "no test";
                        }
                        else
                        {
                            textBoxCTS.BackColor = Color.Lime;
                            textBoxCTS.Text = "Cts On";
                        }
                    }
                    catch (Exception)
                    {


                    }

                }
               

            }
            else if (e.EventType == SerialPinChange.Break)
            {
                if (textBoxBI.InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        try
                        {
                            if (textBoxBI.BackColor == Color.Lime)
                            {
                                textBoxBI.BackColor = Color.LightSkyBlue;
                            }
                            else
                            {
                                textBoxBI.BackColor = Color.Lime;
                            }
                        }
                        catch (Exception)
                        {


                        }
                    });
                }
                else
                {
                    try
                    {
                        if (textBoxBI.BackColor == Color.Lime)
                        {
                            textBoxBI.BackColor = Color.LightSkyBlue;
                        }
                        else
                        {
                            textBoxBI.BackColor = Color.Lime;
                        }
                    }
                    catch (Exception)
                    {


                    }

                }

              
            }
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void numUpDwnRxDelay_ValueChanged(object sender, EventArgs e)
        {
            rxDelayTime = (int)numUpDwnRxDelay.Value;
        }

        private void serialPort1_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            if(e.EventType == SerialError.Frame)
            {
                MessageBox.Show("SerialError ,The hardware detected a frame error", "Error.....", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if(e.EventType == SerialError.Overrun)
            {
                MessageBox.Show("SerialError ,A character-buffer overrun has occured, The next character is lost", "Error.....", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (e.EventType == SerialError.RXOver)
            {
                MessageBox.Show("SerialError ,Input buffer overflow has occured, no room on the input buffer", "Error.....", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (e.EventType == SerialError.RXParity)
            {
                MessageBox.Show("SerialError ,The hardware detected a Parity error", "Error.....", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (e.EventType == SerialError.TXFull)
            {
                MessageBox.Show("SerialError ,The application tried to transmit a character but the ouput buffer was full", "Error.....", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // ComPortClosed();
        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void label18_Click(object sender, EventArgs e)
        {

        }

        private void label35_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen && serialPort1.DtrEnable == false)
            {
                serialPort1.DtrEnable = true;
                button5.BackColor = Color.Lime;
                button5.Text = "Dtr on";
            }
            else if (serialPort1.IsOpen && serialPort1.DtrEnable == true)
            {
                serialPort1.DtrEnable = false;
                button5.BackColor = Color.Red;
                button5.Text = "Dtr off";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            send_repeat_counter = 0; //put this here as closing the form and reopening the form would dounble up on the send

            sendDataNoError = true;

            // SerialEncoding();

            if (serialPort1.IsOpen)
            {
                myAutoReplyDic = new Dictionary<string, string>();

                try
                {
                    string[] strArr = File.ReadAllLines("wifi.txt");

                    lblIsAutoFileLoaded.Text = "wifi.txt";
                    lblAutoReplyLineCount.Text = strArr.Length.ToString();

                    foreach (string str in strArr)
                    {
                        string[] strSplitter = str.Split(autoReplySplitterChar);

                        if (strSplitter.Length == 2)
                        {
                            if (rBtnText.Checked)
                            {
                                if (myAutoReplyDic.ContainsKey(strSplitter[0]) == false)
                                {
                                    myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);
                                    /*if (boolCarrigeReturnLF == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r\n", strSplitter[1]);

                                    }
                                    // serialPort1.Write(tx_data.Replace("\\n", Environment.NewLine));
                                    if (boolCarrigeReturn == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r", strSplitter[1]);

                                    }
                                    if (boolCarrigeReturn == false && boolCarrigeReturnLF == false)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);

                                    }*/
                                }
                            }
                            else if (rBtnHex.Checked)
                            {
                                if (strSplitter[0].Length % 2 == 0 && strSplitter[1].Length % 2 == 0)
                                {
                                    string hexKey = "";
                                    string hexValue = "";

                                    //key
                                    for (int i = 0; i < strSplitter[0].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[0].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexKey += s.ToString("X2");
                                        }
                                    }
                                    //value
                                    for (int i = 0; i < strSplitter[1].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[1].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexValue += s.ToString("X2");
                                        }
                                    }

                                    if (myAutoReplyDic.ContainsKey(hexKey) == false)
                                    {
                                        myAutoReplyDic.Add(hexKey, hexValue);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("The file does not contain formatted hex values..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                            }

                        }
                        else
                        {
                            MessageBox.Show("The file does not contain the char splitter between message and reply..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                    }

                    if (myAutoReplyDic.Count > 0)
                    {
                        rBtnEnableAutoReply.Enabled = true;
                        rBtnEnableAutoReply.Checked = true;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error ::" + ex.Message, "Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (rBtnText.Checked)
                {
                    chkBAutoRead.Checked = true;
                    tabControl2.SelectedIndex = 0;
                    checkBoxSendNormal.Checked = true;
                }
                else if (rBtnHex.Checked)
                {
                    chkBAutoReadHex.Checked = true;
                    tabControl2.SelectedIndex = 1;
                    checkBoxSendHex.Checked = true;
                }

                tabControl1.SelectedIndex = 1;
            }

            if (ischeckBox2DECChecked | ischeckBoxSendHexChecked)
            {
                // MessageBox.Show("send is even");
                DecSend = true;
            }
            //  cout << "String length is even" << endl;
            // else
            if (ischeckBox3DECChecked)
            {
                //  MessageBox.Show("send is odd");
                DecSend = false;
            }
            // cout << "String length is odd" 

            //THIS IS USED TO CONVERT THE SEND TEXT TO HEX AND SEND THROUGH THE SERIAL PORT////////
            if (serialPort1.IsOpen && button4.Enabled)
                try
                {
                    // SerialEncoding();

                    if (ischeckBoxSendHexChecked == true)
                    {

                        tx_data_p = "wifi";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                    //this writes decimal to the serial port
                    if (ischeckBoxSendDecChecked == true)
                    {

                        tx_data_p = "wifi";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }

                    ////this allows access to the value of comboBox1 cross threading
                    if (ischeckBoxSendHexChecked == false && ischeckBoxSendDecChecked == false)
                    {

                        // SerialEncoding();
                        tx_data_p = "wifi";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                }
                catch (Exception ex)
                {
                    // ComPortClosed();
                    // MessageBox.Show("if sending as hex use hex values example 01 not 1 by itself","Message for a newbie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // richTextBox2.Text = "Unauthorized Access Exception Thrown";
                    MessageBox.Show(ex.Message, "Message ", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }


            if (!serialPort1.IsOpen)
            {

                ComPortClosed();

            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            send_repeat_counter = 0; //put this here as closing the form and reopening the form would dounble up on the send

            sendDataNoError = true;

            // SerialEncoding();
            if (serialPort1.IsOpen)
            {
                myAutoReplyDic = new Dictionary<string, string>();

                try
                {
                    string[] strArr = File.ReadAllLines("bt.txt");

                    lblIsAutoFileLoaded.Text = "bt.txt";
                    lblAutoReplyLineCount.Text = strArr.Length.ToString();

                    foreach (string str in strArr)
                    {
                        string[] strSplitter = str.Split(autoReplySplitterChar);

                        if (strSplitter.Length == 2)
                        {
                            if (rBtnText.Checked)
                            {
                                if (myAutoReplyDic.ContainsKey(strSplitter[0]) == false)
                                {
                                    myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);
                                    /*if (boolCarrigeReturnLF == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r\n", strSplitter[1]);

                                    }
                                    // serialPort1.Write(tx_data.Replace("\\n", Environment.NewLine));
                                    if (boolCarrigeReturn == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r", strSplitter[1]);

                                    }
                                    if (boolCarrigeReturn == false && boolCarrigeReturnLF == false)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);

                                    }*/
                                }
                            }
                            else if (rBtnHex.Checked)
                            {
                                if (strSplitter[0].Length % 2 == 0 && strSplitter[1].Length % 2 == 0)
                                {
                                    string hexKey = "";
                                    string hexValue = "";

                                    //key
                                    for (int i = 0; i < strSplitter[0].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[0].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexKey += s.ToString("X2");
                                        }
                                    }
                                    //value
                                    for (int i = 0; i < strSplitter[1].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[1].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexValue += s.ToString("X2");
                                        }
                                    }

                                    if (myAutoReplyDic.ContainsKey(hexKey) == false)
                                    {
                                        myAutoReplyDic.Add(hexKey, hexValue);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("The file does not contain formatted hex values..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                            }

                        }
                        else
                        {
                            MessageBox.Show("The file does not contain the char splitter between message and reply..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                    }

                    if (myAutoReplyDic.Count > 0)
                    {
                        rBtnEnableAutoReply.Enabled = true;
                        rBtnEnableAutoReply.Checked = true;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error ::" + ex.Message, "Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (rBtnText.Checked)
                {
                    chkBAutoRead.Checked = true;
                    tabControl2.SelectedIndex = 0;
                    checkBoxSendNormal.Checked = true;
                }
                else if (rBtnHex.Checked)
                {
                    chkBAutoReadHex.Checked = true;
                    tabControl2.SelectedIndex = 1;
                    checkBoxSendHex.Checked = true;
                }

                tabControl1.SelectedIndex = 1;
            }

            if (ischeckBox2DECChecked | ischeckBoxSendHexChecked)
            {
                // MessageBox.Show("send is even");
                DecSend = true;
            }
            //  cout << "String length is even" << endl;
            // else
            if (ischeckBox3DECChecked)
            {
                //  MessageBox.Show("send is odd");
                DecSend = false;
            }
            // cout << "String length is odd" 

            //THIS IS USED TO CONVERT THE SEND TEXT TO HEX AND SEND THROUGH THE SERIAL PORT////////
            if (serialPort1.IsOpen && button3.Enabled)
                try
                {
                    // SerialEncoding();

                    if (ischeckBoxSendHexChecked == true)
                    {

                        tx_data_p = "bt";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                    //this writes decimal to the serial port
                    if (ischeckBoxSendDecChecked == true)
                    {

                        tx_data_p = "bt";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }

                    ////this allows access to the value of comboBox1 cross threading
                    if (ischeckBoxSendHexChecked == false && ischeckBoxSendDecChecked == false)
                    {

                        // SerialEncoding();
                        tx_data_p = "bt";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                }
                catch (Exception ex)
                {
                    // ComPortClosed();
                    // MessageBox.Show("if sending as hex use hex values example 01 not 1 by itself","Message for a newbie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // richTextBox2.Text = "Unauthorized Access Exception Thrown";
                    MessageBox.Show(ex.Message, "Message ", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }


            if (!serialPort1.IsOpen)
            {

                ComPortClosed();

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            send_repeat_counter = 0; //put this here as closing the form and reopening the form would dounble up on the send

            sendDataNoError = true;

            // SerialEncoding();
            if (serialPort1.IsOpen)
            {
                myAutoReplyDic = new Dictionary<string, string>();

                try
                {
                    string[] strArr = File.ReadAllLines("key.txt");

                    lblIsAutoFileLoaded.Text = "key.txt";
                    lblAutoReplyLineCount.Text = strArr.Length.ToString();

                    foreach (string str in strArr)
                    {
                        string[] strSplitter = str.Split(autoReplySplitterChar);

                        if (strSplitter.Length == 2)
                        {
                            if (rBtnText.Checked)
                            {
                                if (myAutoReplyDic.ContainsKey(strSplitter[0]) == false)
                                {
                                    myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);
                                    /*if (boolCarrigeReturnLF == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r\n", strSplitter[1]);

                                    }
                                    // serialPort1.Write(tx_data.Replace("\\n", Environment.NewLine));
                                    if (boolCarrigeReturn == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r", strSplitter[1]);

                                    }
                                    if (boolCarrigeReturn == false && boolCarrigeReturnLF == false)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);

                                    }*/
                                }
                            }
                            else if (rBtnHex.Checked)
                            {
                                if (strSplitter[0].Length % 2 == 0 && strSplitter[1].Length % 2 == 0)
                                {
                                    string hexKey = "";
                                    string hexValue = "";

                                    //key
                                    for (int i = 0; i < strSplitter[0].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[0].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexKey += s.ToString("X2");
                                        }
                                    }
                                    //value
                                    for (int i = 0; i < strSplitter[1].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[1].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexValue += s.ToString("X2");
                                        }
                                    }

                                    if (myAutoReplyDic.ContainsKey(hexKey) == false)
                                    {
                                        myAutoReplyDic.Add(hexKey, hexValue);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("The file does not contain formatted hex values..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                            }

                        }
                        else
                        {
                            MessageBox.Show("The file does not contain the char splitter between message and reply..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                    }

                    if (myAutoReplyDic.Count > 0)
                    {
                        rBtnEnableAutoReply.Enabled = true;
                        rBtnEnableAutoReply.Checked = true;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error ::" + ex.Message, "Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (rBtnText.Checked)
                {
                    chkBAutoRead.Checked = true;
                    tabControl2.SelectedIndex = 0;
                    checkBoxSendNormal.Checked = true;
                }
                else if (rBtnHex.Checked)
                {
                    chkBAutoReadHex.Checked = true;
                    tabControl2.SelectedIndex = 1;
                    checkBoxSendHex.Checked = true;
                }

                tabControl1.SelectedIndex = 1;
            }

            if (ischeckBox2DECChecked | ischeckBoxSendHexChecked)
            {
                // MessageBox.Show("send is even");
                DecSend = true;
            }
            //  cout << "String length is even" << endl;
            // else
            if (ischeckBox3DECChecked)
            {
                //  MessageBox.Show("send is odd");
                DecSend = false;
            }
            // cout << "String length is odd" 

            //THIS IS USED TO CONVERT THE SEND TEXT TO HEX AND SEND THROUGH THE SERIAL PORT////////
            if (serialPort1.IsOpen && button2.Enabled)
                try
                {
                    // SerialEncoding();

                    if (ischeckBoxSendHexChecked == true)
                    {

                        tx_data_p = "sli";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                    //this writes decimal to the serial port
                    if (ischeckBoxSendDecChecked == true)
                    {

                        tx_data_p = "sli";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }

                    ////this allows access to the value of comboBox1 cross threading
                    if (ischeckBoxSendHexChecked == false && ischeckBoxSendDecChecked == false)
                    {

                        // SerialEncoding();
                        tx_data_p = "sli";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                }
                catch (Exception ex)
                {
                    // ComPortClosed();
                    // MessageBox.Show("if sending as hex use hex values example 01 not 1 by itself","Message for a newbie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // richTextBox2.Text = "Unauthorized Access Exception Thrown";
                    MessageBox.Show(ex.Message, "Message ", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }


            if (!serialPort1.IsOpen)
            {

                ComPortClosed();

            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // RTS cannot be accessed if readyToSend readyToSend xonxoff is enable etc..
            if (comboBoxFlow.SelectedIndex != 3 && comboBoxFlow.SelectedIndex != 4)
            {
                if (serialPort1.IsOpen && serialPort1.RtsEnable == false)
                {

                    serialPort1.RtsEnable = true;
                    button6.BackColor = Color.Lime;
                    button6.Text = "Cts on";
                }
                else if (serialPort1.IsOpen && serialPort1.RtsEnable == true)
                {
                    serialPort1.RtsEnable = false;
                    button6.BackColor = Color.Red;
                    button6.Text = "Cts off";
                }

            }
            else
            {
                MessageBox.Show("Cannot enable RTS when using this Handshaking Flow control", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            send_repeat_counter = 0; //put this here as closing the form and reopening the form would dounble up on the send
            sendDataNoError = true;

            // SerialEncoding();
            if (serialPort1.IsOpen)
            {
                myAutoReplyDic = new Dictionary<string, string>();

                try
                {
                    string[] strArr = File.ReadAllLines("start.txt");

                    lblIsAutoFileLoaded.Text = "start.txt";
                    lblAutoReplyLineCount.Text = strArr.Length.ToString();

                    foreach (string str in strArr)
                    {
                        string[] strSplitter = str.Split(autoReplySplitterChar);

                        if (strSplitter.Length == 2)
                        {
                            if (rBtnText.Checked)
                            {
                                if (myAutoReplyDic.ContainsKey(strSplitter[0]) == false)
                                {
                                    myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);
                                    /*if (boolCarrigeReturnLF == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r\n", strSplitter[1]);

                                    }
                                    // serialPort1.Write(tx_data.Replace("\\n", Environment.NewLine));
                                    if (boolCarrigeReturn == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r", strSplitter[1]);

                                    }
                                    if (boolCarrigeReturn == false && boolCarrigeReturnLF == false)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);

                                    }*/
                                }
                            }
                            else if (rBtnHex.Checked)
                            {
                                if (strSplitter[0].Length % 2 == 0 && strSplitter[1].Length % 2 == 0)
                                {
                                    string hexKey = "";
                                    string hexValue = "";

                                    //key
                                    for (int i = 0; i < strSplitter[0].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[0].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexKey += s.ToString("X2");
                                        }
                                    }
                                    //value
                                    for (int i = 0; i < strSplitter[1].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[1].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexValue += s.ToString("X2");
                                        }
                                    }

                                    if (myAutoReplyDic.ContainsKey(hexKey) == false)
                                    {
                                        myAutoReplyDic.Add(hexKey, hexValue);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("The file does not contain formatted hex values..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                            }

                        }
                        else
                        {
                            MessageBox.Show("The file does not contain the char splitter between message and reply..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                    }

                    if (myAutoReplyDic.Count > 0)
                    {
                        rBtnEnableAutoReply.Enabled = true;
                        rBtnEnableAutoReply.Checked = true;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error ::" + ex.Message, "Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (rBtnText.Checked)
                {
                    chkBAutoRead.Checked = true;
                    tabControl2.SelectedIndex = 0;
                    checkBoxSendNormal.Checked = true;
                }
                else if (rBtnHex.Checked)
                {
                    chkBAutoReadHex.Checked = true;
                    tabControl2.SelectedIndex = 1;
                    checkBoxSendHex.Checked = true;
                }

                tabControl1.SelectedIndex = 1;
            }

            if (ischeckBox2DECChecked | ischeckBoxSendHexChecked)
            {
                // MessageBox.Show("send is even");
                DecSend = true;
            }
            //  cout << "String length is even" << endl;
            // else
            if (ischeckBox3DECChecked)
            {
                //  MessageBox.Show("send is odd");
                DecSend = false;
            }
            // cout << "String length is odd" 

            //THIS IS USED TO CONVERT THE SEND TEXT TO HEX AND SEND THROUGH THE SERIAL PORT////////
            if (serialPort1.IsOpen && button7.Enabled)
                try
                {
                    // SerialEncoding();

                    if (ischeckBoxSendHexChecked == true)
                    {

                        tx_data_p = "cli";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                    //this writes decimal to the serial port
                    if (ischeckBoxSendDecChecked == true)
                    {

                        tx_data_p = "cli";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }

                    ////this allows access to the value of comboBox1 cross threading
                    if (ischeckBoxSendHexChecked == false && ischeckBoxSendDecChecked == false)
                    {

                        // SerialEncoding();
                        tx_data_p = "cli";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                }
                catch (Exception ex)
                {
                    // ComPortClosed();
                    // MessageBox.Show("if sending as hex use hex values example 01 not 1 by itself","Message for a newbie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // richTextBox2.Text = "Unauthorized Access Exception Thrown";
                    MessageBox.Show(ex.Message, "Message ", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }


            if (!serialPort1.IsOpen)
            {

                ComPortClosed();

            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            /*button4_Click(sender, e);
            System.Threading.Thread.Sleep(rxDelayTime);
            button3_Click(sender, e);
            System.Threading.Thread.Sleep(rxDelayTime);
            button2_Click(sender, e);
            System.Threading.Thread.Sleep(rxDelayTime);
            btnDtrOn_Click(sender, e);
            System.Threading.Thread.Sleep(rxDelayTime);
            btnRtsOn_Click(sender, e);
            System.Threading.Thread.Sleep(rxDelayTime);
            btnRtsOff_Click(sender, e);
            System.Threading.Thread.Sleep(rxDelayTime);
            btnDtrOff_Click(sender, e);*/

            send_repeat_counter = 0; //put this here as closing the form and reopening the form would dounble up on the send
            sendDataNoError = true;

            if (serialPort1.IsOpen)
            {
                myAutoReplyDic = new Dictionary<string, string>();

                try
                {
                    string[] strArr = File.ReadAllLines("runall.txt");

                    lblIsAutoFileLoaded.Text = "wifi.txt";
                    lblAutoReplyLineCount.Text = strArr.Length.ToString();

                    foreach (string str in strArr)
                    {
                        string[] strSplitter = str.Split(autoReplySplitterChar);

                        if (strSplitter.Length == 2)
                        {
                            if (rBtnText.Checked)
                            {
                                if (myAutoReplyDic.ContainsKey(strSplitter[0]) == false)
                                {
                                    myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);
                                    /*if (boolCarrigeReturnLF == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r\n", strSplitter[1]);

                                    }
                                    // serialPort1.Write(tx_data.Replace("\\n", Environment.NewLine));
                                    if (boolCarrigeReturn == true)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0] + "\r", strSplitter[1]);

                                    }
                                    if (boolCarrigeReturn == false && boolCarrigeReturnLF == false)
                                    {
                                        myAutoReplyDic.Add(strSplitter[0], strSplitter[1]);

                                    }*/
                                }
                            }
                            else if (rBtnHex.Checked)
                            {
                                if (strSplitter[0].Length % 2 == 0 && strSplitter[1].Length % 2 == 0)
                                {
                                    string hexKey = "";
                                    string hexValue = "";

                                    //key
                                    for (int i = 0; i < strSplitter[0].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[0].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexKey += s.ToString("X2");
                                        }
                                    }
                                    //value
                                    for (int i = 0; i < strSplitter[1].Length; i++)
                                    {

                                        int s;
                                        bool isHex = int.TryParse(strSplitter[1].Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out s);
                                        i++;

                                        if (isHex)
                                        {
                                            hexValue += s.ToString("X2");
                                        }
                                    }

                                    if (myAutoReplyDic.ContainsKey(hexKey) == false)
                                    {
                                        myAutoReplyDic.Add(hexKey, hexValue);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("The file does not contain formatted hex values..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                            }

                        }
                        else
                        {
                            MessageBox.Show("The file does not contain the char splitter between message and reply..", "File Format Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                    }

                    if (myAutoReplyDic.Count > 0)
                    {
                        rBtnEnableAutoReply.Enabled = true;
                        rBtnEnableAutoReply.Checked = true;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error ::" + ex.Message, "Error....", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (rBtnText.Checked)
                {
                    chkBAutoRead.Checked = true;
                    tabControl2.SelectedIndex = 0;
                    checkBoxSendNormal.Checked = true;
                }
                else if (rBtnHex.Checked)
                {
                    chkBAutoReadHex.Checked = true;
                    tabControl2.SelectedIndex = 1;
                    checkBoxSendHex.Checked = true;
                }

                tabControl1.SelectedIndex = 1;
            }

            if (ischeckBox2DECChecked | ischeckBoxSendHexChecked)
            {
                // MessageBox.Show("send is even");
                DecSend = true;
            }
            //  cout << "String length is even" << endl;
            // else
            if (ischeckBox3DECChecked)
            {
                //  MessageBox.Show("send is odd");
                DecSend = false;
            }
            // cout << "String length is odd" 

            //THIS IS USED TO CONVERT THE SEND TEXT TO HEX AND SEND THROUGH THE SERIAL PORT////////
            if (serialPort1.IsOpen && button4.Enabled)
                try
                {
                    // SerialEncoding();

                    if (ischeckBoxSendHexChecked == true)
                    {

                        tx_data_p = "wifi";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                    //this writes decimal to the serial port
                    if (ischeckBoxSendDecChecked == true)
                    {

                        tx_data_p = "wifi";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }

                    ////this allows access to the value of comboBox1 cross threading
                    if (ischeckBoxSendHexChecked == false && ischeckBoxSendDecChecked == false)
                    {

                        // SerialEncoding();
                        tx_data_p = "wifi";
                        txRepeaterDelay.Interval = (int)send_delay.Value;
                        txRepeaterDelay.Start();

                    }
                }
                catch (Exception ex)
                {
                    // ComPortClosed();
                    // MessageBox.Show("if sending as hex use hex values example 01 not 1 by itself","Message for a newbie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // richTextBox2.Text = "Unauthorized Access Exception Thrown";
                    MessageBox.Show(ex.Message, "Message ", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }


            if (!serialPort1.IsOpen)
            {

                ComPortClosed();

            }

        }

        private void textBoxDTR_TextChanged(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (bgwAutoReply.IsBusy == false)
            {
                bgwAutoReply.CancelAsync();
            }
        }
    }
}
