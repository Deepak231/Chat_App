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


namespace Chat_Application
{
    public partial class Form1 : Form
    {
        MySqlConnection conn;
        public static string uname;
        public Form1()
        {
            InitializeComponent();
        }
        public void connect()
        {
            string MyConnection = "server=localhost;user id=root;password = 12345 ;database=database3;persistsecurityinfo=True";
            conn = new MySqlConnection(MyConnection);
            conn.Open();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Checking whether the user have entered all the details 
            if (textBox1.Text.Equals("") || textBox2.Text.Equals(""))
            {
                MessageBox.Show("Please Enter All The Details");
            }
            else
            {
                try
                {
                    //Signing in by checking there is a user with entered username and password 
                    connect();
                    MySqlCommand cm = new MySqlCommand();
                    cm.Connection = conn;
                    cm.CommandText = "SELECT count(*) from user WHERE username = @username and password = @password";
                    cm.Parameters.AddWithValue("@username", textBox1.Text);
                    cm.Parameters.AddWithValue("@password", textBox2.Text);
                    cm.CommandType = CommandType.Text;
                    cm.ExecuteNonQuery();
                    var count = cm.ExecuteScalar();
                    if (count.ToString().Equals("1"))
                    {
                        uname = textBox1.Text;
                        textBox1.Text = "";
                        textBox2.Text = "";
                        Form2 newForm = new Form2();
                        newForm.Show();

                    }
                    else
                    {
                        MessageBox.Show("Username or Password is Incorrect");
                        textBox1.Text = "";
                        textBox2.Text = "";
                    }
                    conn.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unknown Error");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //cancel
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //Sign up
            Form3 newform = new Form3();
            newform.Show();
        }
    }
}
