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

namespace FileSendClient
{
    public partial class Form1 : Form
    {
        public Stream fileStream;
        public byte[] fileBuffer;
        public TcpClient clientSocket;
        public TcpListener Listener;
        public NetworkStream networkStream;

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
            try
            {
                Stream fileStream = File.OpenRead(textBox1.Text);
                // Alocate memory space for the file

               // String cmd = "COMMAND_SEND:" + fileName;
                byte[] fileBuffer = new byte[fileStream.Length];
                fileStream.Read(fileBuffer, 0, (int)fileStream.Length);
                // Open a TCP/IP Connection and send the data
                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Write(fileBuffer, 0, fileBuffer.GetLength(0));
                byte[] bytesToRead = new byte[clientSocket.ReceiveBufferSize];
                int bytesRead = networkStream.Read(bytesToRead, 0, clientSocket.ReceiveBufferSize);
                string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
            }
            catch (Exception f)
            {
                MessageBox.Show("The target machine is not working");
            }
            networkStream.Close();
        }
        public void sendString(NetworkStream ns, string str)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(str);
            ns.Write(buffer, 0, buffer.Length);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            this.clientSocket = new TcpClient("127.0.0.1", 8080);
            String str = "COMMAND_AUTH:"+textBox3.Text;
            NetworkStream networkStream = clientSocket.GetStream();
            this.sendString(networkStream, str);
            byte[] bytesToRead = new byte[clientSocket.ReceiveBufferSize];
            int bytesRead = networkStream.Read(bytesToRead, 0, clientSocket.ReceiveBufferSize);
            string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                this.clientSocket = new TcpClient("127.0.0.1", 8080);
                String str = "COMMAND_LIST";
                NetworkStream networkStream = clientSocket.GetStream();  //betgeb el stream 3ashan a'dar ab3t ll server
                this.sendString(networkStream, str);  //send el string 
                byte[] bytesToRead = new byte[clientSocket.ReceiveBufferSize];  //brecieve 
                int bytesRead = networkStream.Read(bytesToRead, 0, clientSocket.ReceiveBufferSize);
                string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                if (!response.StartsWith("UNAUTH"))
                {
                    string[] files = response.Split(',');
                    foreach (var item in files)
                    {
                        fileList.Items.Add(item);
                    }
                }
                else
                {
                    MessageBox.Show("please authorize first");
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
            this.clientSocket = new TcpClient("127.0.0.1", 8080);
            String fileName = fileList.SelectedItem.ToString();
            String cmd = "COMMAND_RECIEVE:"+ fileName;
            NetworkStream networkStream = clientSocket.GetStream();  
            this.sendString(networkStream, cmd);
            int thisRead = 0;
            int blockSize = 1024;
            Byte[] dataByte = new Byte[blockSize];
            lock (this)
            {
                Stream fileStream = File.OpenWrite(@"C:\Users\Khaled\Desktop\Recievefiles\" + fileName);
                while (true)
                {
                    thisRead = networkStream.Read(dataByte, 0, blockSize);
                    fileStream.Write(dataByte, 0, thisRead);
                    if (thisRead == 0) break;
                }
                fileStream.Close();
            }
            return;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.clientSocket = new TcpClient("127.0.0.1", 8080);
            String str = "SHUT_DOWN";
            NetworkStream networkStream = clientSocket.GetStream();
            this.sendString(networkStream, str);
            byte[] bytesToRead = new byte[clientSocket.ReceiveBufferSize];
            int bytesRead = networkStream.Read(bytesToRead, 0, clientSocket.ReceiveBufferSize);
            string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.clientSocket = new TcpClient("127.0.0.1", 8080);
            String str = "LOG_OFF";
            NetworkStream networkStream = clientSocket.GetStream();
            this.sendString(networkStream, str);
            byte[] bytesToRead = new byte[clientSocket.ReceiveBufferSize];
            int bytesRead = networkStream.Read(bytesToRead, 0, clientSocket.ReceiveBufferSize);
            string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.clientSocket = new TcpClient("127.0.0.1", 8080);
            String str = "RESTART";
            NetworkStream networkStream = clientSocket.GetStream();
            this.sendString(networkStream, str);
            byte[] bytesToRead = new byte[clientSocket.ReceiveBufferSize];
            int bytesRead = networkStream.Read(bytesToRead, 0, clientSocket.ReceiveBufferSize);
            string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
        }
    }
}
