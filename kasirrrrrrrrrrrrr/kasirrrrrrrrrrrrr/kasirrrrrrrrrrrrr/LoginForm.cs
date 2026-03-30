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
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace kasirrrrrrrrrrrrr
{
    public partial class LoginForm : Form
    {
        DatabaseHelper dbHelper = new DatabaseHelper();
        public LoginForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel3_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            using (MySqlConnection conn = dbHelper.GetConnection())
            {
                try
                {
                    
                    string query = "SELECT id, phone_number, cust_active, cust_name,vendor_name, password FROM users WHERE phone_number=@phone AND password=@pass";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@phone", guna2TextBox1.Text);
                    cmd.Parameters.AddWithValue("@pass", guna2TextBox2.Text);

                    conn.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        UserSession.UserID = reader["id"].ToString();
                        UserSession.PhoneNumber = reader["phone_number"].ToString();

                        int isCust = Convert.ToInt32(reader["cust_active"]);
                        string selectedRole = guna2ComboBox1.SelectedItem.ToString();

                        
                        if (selectedRole == "Customer" && isCust == 1)
                        {
                            UserSession.UserRole = "Customer";
                            UserSession.Username = reader["cust_name"].ToString();
                        }
                        else if (selectedRole == "Vendor" && isCust == 0)
                        {
                            UserSession.UserRole = "Vendor";
                            UserSession.Username = reader["vendor_name"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("Role tidak sesuai dengan akun!", "Login Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        MessageBox.Show("Login Berhasil!", "Sukses");

                        MainForm main = new MainForm();
                        main.Show();
                        this.Hide();
                    }
                }
                   
                
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            RegistForm regist = new RegistForm();
            regist.Show();
        }
    }
}
