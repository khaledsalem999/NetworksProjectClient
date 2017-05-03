using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FileSendClient
{
    public partial class Form1 : Form
    {
        public Stream fileStream;
        public byte[] fileBuffer;
        public TcpClient clientSocket;
        public TcpListener Listener;
        public NetworkStream networkStream;
        [DllImport("kernel32.dll")]
        private extern static uint SetSystemTime(ref SYSTEMTIME lpSystemTime);

        private struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
        }


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBox1.Text = openFileDialog1.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Nothing to send");
            }
            else
            {
                try
                {
                    this.clientSocket = new TcpClient(textBox2.Text, 8080);
                    Stream fileStream = File.OpenRead(textBox1.Text);
                    // Alocate memory space for the file
                    String fileName = Path.GetFileName(textBox1.Text);
                    String cmd = "COMMAND_SEND:" + fileName;
                    NetworkStream networkStream = clientSocket.GetStream();
                    this.sendString(networkStream, cmd);
                    byte[] bytesToRead = new byte[clientSocket.ReceiveBufferSize];
                    int bytesRead = networkStream.Read(bytesToRead, 0, clientSocket.ReceiveBufferSize);
                    string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    if (response == "COMMAND_SEND_ACKNOWLEGED")
                    {
                        // Open a TCP/IP Connection and send the data
                        // NetworkStream networkStream = clientSocket.GetStream();
                        byte[] fileBuffer = new byte[fileStream.Length];
                        fileStream.Read(fileBuffer, 0, (int)fileStream.Length);
                        networkStream.Write(fileBuffer, 0, fileBuffer.GetLength(0));
                        networkStream.Close();
                        MessageBox.Show("file " + fileName + " has been sent");
                    }
                    else
                    {
                        MessageBox.Show("Please Auth first");
                    }
                }
                catch (Exception f)
                {
                    MessageBox.Show("The target machine is not working");
                }
            }
            
        }
        public void sendString(NetworkStream ns, string str)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(str);
            ns.Write(buffer, 0, buffer.Length);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                this.clientSocket = new TcpClient(textBox2.Text, 8080);
                String str = "COMMAND_AUTH:" + textBox3.Text;
                NetworkStream networkStream = clientSocket.GetStream();
                this.sendString(networkStream, str);
                byte[] bytesToRead = new byte[clientSocket.ReceiveBufferSize];
                int bytesRead = networkStream.Read(bytesToRead, 0, clientSocket.ReceiveBufferSize);
                string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                MessageBox.Show(response);
            }
            catch(Exception f)
            {
                MessageBox.Show("Target machine is not running");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                this.clientSocket = new TcpClient(textBox2.Text, 8080);
                String str = "COMMAND_LIST";
                NetworkStream networkStream = clientSocket.GetStream();  //betgeb el stream 3ashan a'dar ab3t ll server
                this.sendString(networkStream, str);  //send el string 
                byte[] bytesToRead = new byte[clientSocket.ReceiveBufferSize];  //brecieve 
                int bytesRead = networkStream.Read(bytesToRead, 0, clientSocket.ReceiveBufferSize);
                string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                if (!response.StartsWith("UNAUTH"))
                {
                    string[] files = response.Split(',');
                    fileList.Items.Clear();
                    foreach (var item in files)
                    {
                        fileList.Items.Add(item);
                    }
                }
                else
                {
                    MessageBox.Show("Please authorize first");
                }
            }
            catch (Exception f)
            {
                MessageBox.Show("The target machine is not working");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(startTcp));
            t.Start();
        }

        private void startTcp()
        {
        }

        private void fileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                const string message =
                                        "Which command you want to excute? Press (Yes to delete) (No to Recieve file) and (Cancel to abort) ";
                const string caption = "Delete/Recieve File";
                var result = MessageBox.Show(message, caption,
                                             MessageBoxButtons.YesNoCancel,
                                             MessageBoxIcon.Question);
                String fileName = fileList.SelectedItem.ToString();

                if (result == DialogResult.No)
                {
                    this.clientSocket = new TcpClient(textBox2.Text, 8080);

                    String cmd = "COMMAND_RECIEVE:" + fileName;
                    NetworkStream networkStream = clientSocket.GetStream();
                    this.sendString(networkStream, cmd);
                    int thisRead = 0;
                    int blockSize = 1024;
                    Byte[] dataByte = new Byte[blockSize];
                    lock (this)
                    {
                        Stream fileStream = File.OpenWrite(@"C:\Users\user\networks2\" + fileName);
                        while (true)
                        {
                            thisRead = networkStream.Read(dataByte, 0, blockSize);
                            fileStream.Write(dataByte, 0, thisRead);
                            if (thisRead == 0) break;
                        }
                        fileStream.Close();
                    }
                }
                else if (result == DialogResult.Yes)
                {
                    try
                    {
                        this.clientSocket = new TcpClient(textBox2.Text, 8080);
                        String cmdD = "COMMAND_DELETE" + fileName;
                        NetworkStream networkStream = clientSocket.GetStream();
                        this.sendString(networkStream, cmdD);
                        byte[] bytesToRead = new byte[clientSocket.ReceiveBufferSize];
                        int bytesRead = networkStream.Read(bytesToRead, 0, clientSocket.ReceiveBufferSize);
                        string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                        MessageBox.Show(response);
                    }
                    catch (Exception t)
                    {
                        MessageBox.Show("smth went wrong" + t);
                    }
                }
                else
                {

                }
            }
            catch (Exception s)
            {
                MessageBox.Show("Target machine is not running");
            }
            return;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                    this.clientSocket = new TcpClient(textBox2.Text, 8080);
                    String str = "SHUT_DOWN";
                    NetworkStream networkStream = clientSocket.GetStream();
                    this.sendString(networkStream, str);
                    byte[] bytesToRead = new byte[clientSocket.ReceiveBufferSize];
                    int bytesRead = networkStream.Read(bytesToRead, 0, clientSocket.ReceiveBufferSize);
                    string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                if (!response.StartsWith("UNAUTH"))
                {
                    MessageBox.Show("Shutting Down...");
                }
                else
                {
                    MessageBox.Show("Please authorize first");
                }
            }
            catch(Exception a)
            {
                MessageBox.Show("Target machine is not running");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {

                this.clientSocket = new TcpClient(textBox2.Text, 8080);
                String str = "LOG_OFF";
                NetworkStream networkStream = clientSocket.GetStream();
                this.sendString(networkStream, str);
                byte[] bytesToRead = new byte[clientSocket.ReceiveBufferSize];
                int bytesRead = networkStream.Read(bytesToRead, 0, clientSocket.ReceiveBufferSize);
                string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                if (!response.StartsWith("UNAUTH"))
                {
                    MessageBox.Show("Logging off....");
                }
                else
                {
                    MessageBox.Show("Please authorize first");
                }
            }
            catch(Exception w)
            {
                MessageBox.Show("Target machine is not running");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                this.clientSocket = new TcpClient(textBox2.Text, 8080);
                String str = "RESTART";
                NetworkStream networkStream = clientSocket.GetStream();
                this.sendString(networkStream, str);
                byte[] bytesToRead = new byte[clientSocket.ReceiveBufferSize];
                int bytesRead = networkStream.Read(bytesToRead, 0, clientSocket.ReceiveBufferSize);
                string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                if (!response.StartsWith("UNAUTH"))
                {
                    MessageBox.Show("Restarting....");
                }
                else
                {
                    MessageBox.Show("Please authorize first");
                }
            }
            catch(Exception q)
            {
                MessageBox.Show("Target machine is not running");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                this.clientSocket = new TcpClient(textBox2.Text, 8080);
                String str = "TIME_CHANGE";
                NetworkStream networkStream = clientSocket.GetStream();
                this.sendString(networkStream, str);
                byte[] bytesToRead = new byte[clientSocket.ReceiveBufferSize];
                int bytesRead = networkStream.Read(bytesToRead, 0, clientSocket.ReceiveBufferSize);
                string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                if (!response.StartsWith("UNAUTH"))
                {// this will work in admin mode only 
                    string strCmdText;
                    strCmdText = response;
                    System.Diagnostics.Process.Start("CMD.exe", strCmdText);
                    MessageBox.Show("Time changed");
                }
                else
                {
                    MessageBox.Show("Please authorize first");
                }
                
            }
            catch(Exception t)
            {
                MessageBox.Show("Target machine is not running");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
