using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using MySql.Data.MySqlClient;

namespace kasirrrrrrrrrrrrr
{
    public partial class ProfileControl : UserControl
    {

        DatabaseHelper dbHelper = new DatabaseHelper();
        public ProfileControl()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            using (MySqlConnection conn = dbHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                   
                    string query = "SELECT * FROM users WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", UserSession.UserID);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        
                        guna2TextBox1.Text = reader["phone_number"].ToString();
                        guna2TextBox2.Text = reader["email"].ToString();

                        
                        if (UserSession.UserRole == "Customer")
                        {
                           
                            guna2TextBox4.Text = reader["cust_name"].ToString();
                            guna2TextBox3.Text = reader["cust_address"].ToString();
                            guna2TextBox5.Text = reader["cust_latitude"].ToString();
                            guna2TextBox6.Text = reader["cust_longitude"].ToString();

                            guna2RadioButton1.Checked = true;
                            guna2RadioButton2.Checked = false;

                            
                            guna2GroupBox2.Enabled = false;
                            guna2GroupBox1.Enabled = true;
                        }
                        else
                        {
                           
                            guna2TextBox10.Text = reader["vendor_name"].ToString();
                            guna2TextBox9.Text = reader["vendor_address"].ToString();
                            guna2TextBox8.Text = reader["vendor_latitude"].ToString();
                            guna2TextBox7.Text = reader["vendor_longitude"].ToString();

                            guna2RadioButton2.Checked = true;
                            guna2RadioButton1.Checked = false;

                            
                            guna2GroupBox1.Enabled = false;
                            guna2GroupBox2.Enabled = true;
                        }
                        guna2RadioButton1.Enabled = false;
                        guna2RadioButton2.Enabled = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat profil: " + ex.Message);
                }
            }
        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            using (MySqlConnection conn = dbHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "";

                    // Pilih query berdasarkan role
                    if (UserSession.UserRole == "Customer")
                    {
                        query = @"UPDATE users SET 
                                 phone_number = @phone, 
                                 email = @email, 
                                 cust_name = @name, 
                                 cust_address = @addr, 
                                 cust_latitude = @lat, 
                                 cust_longitude = @lng 
                                 WHERE id = @id";
                    }
                    else
                    {
                        query = @"UPDATE users SET 
                                 phone_number = @phone, 
                                 email = @email, 
                                 vendor_name = @name, 
                                 vendor_address = @addr, 
                                 vendor_latitude = @lat, 
                                 vendor_longitude = @lng 
                                 WHERE id = @id";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@phone", guna2TextBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@email", guna2TextBox2.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", UserSession.UserID);

                    if (UserSession.UserRole == "Customer")
                    {
                        cmd.Parameters.AddWithValue("@name", guna2TextBox4.Text.Trim());
                        cmd.Parameters.AddWithValue("@addr", guna2TextBox3.Text.Trim());
                        cmd.Parameters.AddWithValue("@lat", guna2TextBox5.Text.Trim());
                        cmd.Parameters.AddWithValue("@lng", guna2TextBox6.Text.Trim());
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@name", guna2TextBox10.Text.Trim());
                        cmd.Parameters.AddWithValue("@addr", guna2TextBox9.Text.Trim());
                        cmd.Parameters.AddWithValue("@lat", guna2TextBox8.Text.Trim());
                        cmd.Parameters.AddWithValue("@lng", guna2TextBox7.Text.Trim());
                    }

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Profil berhasil diperbarui!", "Sukses");
                        // Refresh data
                        LoadUserData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menyimpan perubahan: " + ex.Message, "Error");
                }
            }
        }
    }
}
