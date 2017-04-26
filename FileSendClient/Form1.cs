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
            fileStream = File.OpenRead(textBox1.Text);
            // Alocate memory space for the file
            //hello world 22
            fileBuffer = new byte[fileStream.Length];
            fileStream.Read(fileBuffer, 0, (int)fileStream.Length);
            // Open a TCP/IP Connection and send the data
            clientSocket = new TcpClient(textBox2.Text, 8080);
            networkStream = clientSocket.GetStream();
            networkStream.Write(fileBuffer, 0, fileBuffer.GetLength(0));
            networkStream.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            clientSocket = new TcpClient("192.168.1.110", 8080);

            String str = textBox3.Text;
            Stream stm = clientSocket.GetStream();

            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(str);

            stm.Write(ba, 0, ba.Length);

            byte[] bb = new byte[100];
            int k = stm.Read(bb, 0, 100);

            for (int i = 0; i < k; i++)
                Console.Write(Convert.ToChar(bb[i]));

        }

    }
}
