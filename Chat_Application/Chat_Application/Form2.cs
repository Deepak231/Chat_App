using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Chat_Application
{
    public partial class Form2 : Form
    {

        Image x;
        MySqlConnection conn;
        MySqlDataReader dr;
        Socket sock;
        EndPoint eplocal, epremote;
        byte[] buffer;
        string myIP;
        public Form2()
        {

            InitializeComponent();
        }
        public void connect()
        {
            string MyConnection = "server=localhost;user id=root;password = 12345 ;database=database3;persistsecurityinfo=True";
            conn = new MySqlConnection(MyConnection);
            conn.Open();
        }
        private Image Resize_image(Image x)
        {
            //Uploaded Image must be resized to the picture box of the form
            Size newsize = pictureBox1.Size;
            Image y = new Bitmap(newsize.Width, newsize.Height);
            using (Graphics gfx = Graphics.FromImage((Bitmap)y))
            {
                gfx.DrawImage(x, new Rectangle(Point.Empty, newsize));
            }
            return y;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //For uploading image
            OpenFileDialog dlg = new OpenFileDialog();
            //Initial Directory
            dlg.InitialDirectory = @"E:\my prg\Projects\Chat_App\Pictures";
            dlg.Title = "Upload";
            //Filters to jpg and png
            dlg.Filter = "Picture Files (*)|*.jpg;*.png";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                x = Image.FromFile(dlg.FileName);
                pictureBox1.Image = Resize_image(x);
            }
            connect();
            //Storing the image file path in the database table 'user'
            //Each time user update's the image,the file path is updated 
            MySqlCommand cm = new MySqlCommand();
            cm.Connection = conn;
            cm.CommandText = "UPDATE user set image = @image where username = @username";
            cm.Parameters.AddWithValue("@username", textBox1.Text);
            cm.Parameters.AddWithValue("@image", dlg.FileName);
            cm.CommandType = CommandType.Text;
            cm.ExecuteNonQuery();
            conn.Close();

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Converts the String message of the user  to byte format before sending 
            try
            {
                int num;
                ASCIIEncoding encode = new ASCIIEncoding();
                byte[] send_msg = new byte[1500];
                num = textBox3.Lines.Length;
                for (int i = 0; i < num;i++ )
                {
                    send_msg = encode.GetBytes(textBox3.Lines[i]);
                    sock.Send(send_msg);
                    listBox1.Items.Add("You  :  " + textBox3.Lines[i]);
                }
                textBox3.Text = "";
            }
            catch (Exception ex)
            {
                //If the user has not selected any receiver
                MessageBox.Show("Please Connect");
            }
        }

        private void Form2_Activated(object sender, EventArgs e)
        {
            //Each time a user account is created it will add to the receiver combo box,
            //so the user can communicate
            connect();
            MySqlCommand cm = new MySqlCommand();
            cm.Connection = conn;
            cm.CommandText = "SELECT username from user WHERE username != @username ";
            cm.Parameters.AddWithValue("@username", textBox1.Text);
            dr = cm.ExecuteReader();
            while (dr.Read())
            {
                if (!(comboBox1.Items.Contains(dr["username"].ToString())))
                    comboBox1.Items.Add(dr["username"].ToString());

            }
            conn.Close();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //To Load the image 
            textBox1.Text = Form1.uname;
            connect();
            MySqlCommand cm = new MySqlCommand();
            cm.Connection = conn;
            cm.CommandText = "SELECT image from user WHERE username = @username ";
            cm.Parameters.AddWithValue("@username", textBox1.Text);
            dr = cm.ExecuteReader();
            if (dr.Read())
            {
                pictureBox1.Image = Resize_image(Image.FromFile(dr["image"].ToString()));
            }
            //Initializing the connection
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            string hostName = Dns.GetHostName();
            myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void msgcallback(IAsyncResult res)
        {
            try
            {
                //To convert the received message from byte format to string format  
                byte[] rec_data = new byte[1500];
                rec_data = (byte[])res.AsyncState;
                ASCIIEncoding encode = new ASCIIEncoding();
                string rec_msg = encode.GetString(rec_data);
                listBox1.Invoke(new Action(() => listBox1.Items.Add(comboBox1.Text + "  : " + rec_msg)));
                buffer = new byte[1500];
                //To keep the connection for incoming message 
                sock.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epremote, new AsyncCallback(msgcallback), buffer);
            }
            catch (Exception e) { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //If combobox is empty
            if (comboBox1.Text.Equals(""))
            {
                MessageBox.Show("Select the receiver");
            }
            else
            {
                //To extract senders and receivers port no.
                int loc_port = 0, rem_port = 0;
                connect();
                MySqlCommand cm = new MySqlCommand();
                cm.Connection = conn;
                cm.CommandText = "SELECT port_no from user WHERE username = @username ";
                cm.Parameters.AddWithValue("@username", textBox1.Text);
                dr = cm.ExecuteReader();
                if (dr.Read())
                {
                    loc_port = Int32.Parse(dr["port_no"].ToString());
                }
                conn.Close();
                connect();
                MySqlCommand cm1 = new MySqlCommand();
                cm1.Connection = conn;
                cm1.CommandText = "SELECT port_no from user WHERE username = @username ";
                cm1.Parameters.AddWithValue("@username", comboBox1.Text);
                dr = cm1.ExecuteReader();
                if (dr.Read())
                {
                    rem_port = Int32.Parse(dr["port_no"].ToString());
                }
                conn.Close();
                try
                {
                    //Binding the senders address and port no.
                    eplocal = new IPEndPoint(IPAddress.Parse(myIP), loc_port);
                    sock.Bind(eplocal);
                    //Connecting to receivers address and port no.
                    epremote = new IPEndPoint(IPAddress.Parse(myIP), rem_port);
                    sock.Connect(epremote);
                    buffer = new byte[1500];
                    //To start receiving the message
                    sock.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epremote, new AsyncCallback(msgcallback), buffer);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connection Successful");
                }
            }
        }





    }
}
