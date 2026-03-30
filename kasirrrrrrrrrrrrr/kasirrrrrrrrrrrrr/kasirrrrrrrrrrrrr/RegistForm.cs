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

namespace kasirrrrrrrrrrrrr
{
    public partial class RegistForm : Form
    {
        DatabaseHelper dbHelper = new DatabaseHelper();
        public RegistForm()
        {
            InitializeComponent();
        }

        private void guna2GroupBox1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            string phone = guna2TextBox1.Text.Trim();
            string email = guna2TextBox2.Text.Trim();
            string pass = guna2TextBox11.Text.Trim();
            string confirm = guna2TextBox12.Text.Trim();

            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Data utama (Phone, Email, Password) wajib diisi!", "Peringatan");
                return;
            }

            if (pass != confirm)
            {
                MessageBox.Show("Konfirmasi password tidak cocok!", "Error");
                return;
            }

            using (MySqlConnection conn = dbHelper.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = @"INSERT INTO users 
                        (phone_number, email, password, cust_active, vendor_active, 
                         cust_name, cust_address, cust_latitude, cust_longitude, 
                         vendor_name, vendor_address, vendor_latitude, vendor_longitude) 
                        VALUES 
                        (@phone, @email, @pass, @c_act, @v_act, 
                         @c_name, @c_addr, @c_lat, @c_long, 
                         @v_name, @v_addr, @v_lat, @v_long)";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@pass", pass);

                    if (guna2RadioButton1.Checked)
                    {
                       
                        cmd.Parameters.AddWithValue("@c_act", 1);
                        cmd.Parameters.AddWithValue("@v_act", 0);
                        cmd.Parameters.AddWithValue("@c_name", guna2TextBox4.Text.Trim());
                        cmd.Parameters.AddWithValue("@c_addr", guna2TextBox3.Text.Trim());

                        
                        decimal cLat = 0, cLong = 0;
                        decimal.TryParse(guna2TextBox5.Text, out cLat);
                        decimal.TryParse(guna2TextBox6.Text, out cLong);
                        cmd.Parameters.AddWithValue("@c_lat", cLat);
                        cmd.Parameters.AddWithValue("@c_long", cLong);

                        cmd.Parameters.AddWithValue("@v_name", "");
                        cmd.Parameters.AddWithValue("@v_addr", "");
                        cmd.Parameters.AddWithValue("@v_lat", 0);
                        cmd.Parameters.AddWithValue("@v_long", 0);
                    }
                    else
                    {
                       
                        cmd.Parameters.AddWithValue("@c_act", 0);
                        cmd.Parameters.AddWithValue("@v_act", 1);
                        cmd.Parameters.AddWithValue("@v_name", guna2TextBox10.Text.Trim());
                        cmd.Parameters.AddWithValue("@v_addr", guna2TextBox9.Text.Trim());

                        
                        decimal vLat = 0, vLong = 0;
                        decimal.TryParse(guna2TextBox8.Text, out vLat);
                        decimal.TryParse(guna2TextBox7.Text, out vLong);
                        cmd.Parameters.AddWithValue("@v_lat", vLat);
                        cmd.Parameters.AddWithValue("@v_long", vLong);

                       
                        cmd.Parameters.AddWithValue("@c_name", "");
                        cmd.Parameters.AddWithValue("@c_addr", "");
                        cmd.Parameters.AddWithValue("@c_lat", 0);
                        cmd.Parameters.AddWithValue("@c_long", 0);
                    }

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Registrasi akun berhasil!", "Sukses");
                        this.Hide();
                        new LoginForm().Show();
                    }
                }   
                catch (Exception ex)
                {
                    MessageBox.Show("Error saat registrasi: " + ex.Message, "Error");
                }
            }
        }

        private void guna2TextBox11_TextChanged(object sender, EventArgs e)
        {

        }

        

        private void guna2RadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2RadioButton1.Checked)
            {
                guna2GroupBox1.Enabled = true;
                guna2GroupBox2.Enabled = false;
            }
        }

        private void guna2RadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void guna2RadioButton2_CheckedChanged_1(object sender, EventArgs e)
        {
            if (guna2RadioButton2.Checked)
            {
                guna2GroupBox2.Enabled = true;
                guna2GroupBox1.Enabled = false;
            }
        }

        private void RegistForm_Load(object sender, EventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2TextBox4_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
