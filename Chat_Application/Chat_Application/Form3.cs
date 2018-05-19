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
    public partial class Form3 : Form
    {
        MySqlConnection conn;
        int count;
        public Form3()
        {
            InitializeComponent();
        }
        public void connect()
        {
            string MyConnection = "server=localhost;user id=root;password = 12345 ;database=database3;persistsecurityinfo=True";
            conn = new MySqlConnection(MyConnection);
            conn.Open();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //User is allowed to sign up only if he accepts terms and condition
            if (checkBox1.Checked.Equals(false))
            {
                button1.Enabled = false;

            }
            else
            {
                button1.Enabled = true;

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Cancel
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Checking Whether the  Password and Confirm Password are same 
            if (!textBox2.Text.Equals(textBox3.Text))
            {
                MessageBox.Show("Password doesn't Match");
                textBox2.Text = "";
                textBox3.Text = "";
            }
            //Checking Whether the user has entered all the details
            else if (textBox1.Text.Equals("") || textBox2.Text.Equals("") || textBox3.Text.Equals(""))
            {
                MessageBox.Show("Please Enter All The Details");
            }
            else
            {
                try
                {
                    //If the entered details are valid then his account is creaeted by storing in the database 'user'
                    connect();
                    MySqlCommand cm = new MySqlCommand();
                    cm.Connection = conn;
                    cm.CommandText = "INSERT INTO user(username,password,image,port_no) values(@username,@password,@image,@port_no)";
                    cm.Parameters.AddWithValue("@username", textBox1.Text);
                    cm.Parameters.AddWithValue("@password", textBox2.Text);
                    cm.Parameters.AddWithValue("@image", "E:\\my prg\\Projects\\Chat_App\\Pictures\\white.jpg");
                    cm.Parameters.AddWithValue("@port_no", (1030 + count).ToString());
                    cm.CommandType = CommandType.Text;
                    cm.ExecuteNonQuery();
                    MessageBox.Show("You've Successfully Signed Up");
                    this.Close();
                    conn.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Username is already Taken");
                }
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            
        }

        private void Form3_Activated(object sender, EventArgs e)
        {
            //using port no from 1030 //port no.(1024 to 49151) is for used for in-house-application
            //Since the port no. must be unique whenever the account is created the port no. increments
            connect();
            MySqlCommand cm = new MySqlCommand();
            cm.Connection = conn;
            cm.CommandText = "SELECT count(*) from user ";
            cm.CommandType = CommandType.Text;
            cm.ExecuteNonQuery();
            var x = cm.ExecuteScalar();
            count = Int32.Parse(x.ToString());
            conn.Close();
        }
    }
}
