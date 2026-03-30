using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace kasirrrrrrrrrrrrr
{

    public partial class productcontrol : UserControl
    {
        double price = 0;
        double stock = 0;
        DatabaseHelper dbhelper = new DatabaseHelper();
        public productcontrol()
        {
            InitializeComponent();
            LoadData();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UserSession.UserID))
            {
                MessageBox.Show("Sesi login habis, silahkan login ulang.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(guna2TextBox1.Text))
            {
                MessageBox.Show("Nama produk tidak boleh kosong!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = dbhelper.GetConnection())
            {
                try
                {

                    string query = "INSERT INTO products (products_name, price_per_unit, unit_stock, vendor_id) VALUES (@name, @price, @stock, @vendorId)";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", guna2TextBox1.Text);
                    cmd.Parameters.AddWithValue("@price", guna2NumericUpDown1.Value);
                    cmd.Parameters.AddWithValue("@stock", guna2NumericUpDown2.Value);
                    cmd.Parameters.AddWithValue("@vendorId", UserSession.UserID);
                    
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Produk berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadData();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Simpan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void LoadData()
        {
            using (MySqlConnection conn = dbhelper.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = "SELECT id, products_name, price_per_unit, unit_stock FROM products WHERE deleted_at IS NULL";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    guna2DataGridView1.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Load: " + ex.Message);
                }
            }
        }
        private void ClearForm()
        {
            guna2TextBox1.Clear();
            comboBox1.SelectedIndex = -1;
            guna2NumericUpDown1.Value = 0;
            guna2NumericUpDown2.Value = 0;
            guna2RadioButton1.Checked = false;
            guna2RadioButton2.Checked = false;
            comboBox2.SelectedIndex = -1;
        }
        int selectedId = 0;


        private string selectedID = "";


        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex < 0) return;

            DataGridViewRow row = guna2DataGridView1.Rows[e.RowIndex];

            selectedID = row.Cells["id"].Value.ToString();
            guna2TextBox1.Text = row.Cells["products_name"].Value.ToString();
            guna2NumericUpDown1.Value = Convert.ToDecimal(row.Cells["price_per_unit"].Value);
            guna2NumericUpDown2.Value = Convert.ToDecimal(row.Cells["unit_stock"].Value);

            price = Convert.ToDouble(row.Cells["price_per_unit"].Value);
            stock = Convert.ToDouble(row.Cells["unit_stock"].Value);

            guna2NumericUpDown3.Value = 1;
            HitungTotal();

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

            if (selectedID == "")
            {
                MessageBox.Show("Pilih data di tabel dulu!", "Peringatan");
                return;
            }

            using (MySqlConnection conn = dbhelper.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = @"UPDATE products SET 
                            products_name = @name,
                            price_per_unit = @price,
                            unit_stock = @stock,
                            updated_at = NOW()
                            WHERE id = @id";

                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    
                    cmd.Parameters.AddWithValue("@name", guna2TextBox1.Text);
                    cmd.Parameters.AddWithValue("@price", guna2NumericUpDown1.Value);
                    cmd.Parameters.AddWithValue("@stock", guna2NumericUpDown2.Value);
                    cmd.Parameters.AddWithValue("@id", Convert.ToInt32(selectedID));


                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Data berhasil diupdate!", "Sukses");

                    LoadData();
                    ClearForm();
                    selectedID = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Update: " + ex.Message);
                }
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedID))
            {
                MessageBox.Show("Silahkan pilih data di tabel terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            DialogResult dialogResult = MessageBox.Show($"Apakah Anda yakin ingin menghapus data dengan ID: {selectedID}?",
                "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.Yes)
            {
                using (MySqlConnection conn = dbhelper.GetConnection())
                {
                    try
                    {

                        string query = "DELETE FROM products WHERE id=@id";
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", selectedID);

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Data berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                            selectedID = "";
                        }
                        else
                        {
                            MessageBox.Show("Tidak ada data yang terhapus. ID mungkin tidak ditemukan.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saat menghapus: " + ex.Message);
                    }
                }
            }
        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void productcontrol_Load(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel12_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel12_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedID))
            {
                MessageBox.Show("Silahkan pilih product di tabel terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = dbhelper.GetConnection())
            {
                //try
                //{
                //    string query = "SELECT FROM products WHERE id=@id, ";
                //    MySqlCommand cmd = new MySqlCommand(query, conn);
                //    cmd.Parameters.AddWithValue("@id", selectedID);


                //}
            }
        }

        private void guna2NumericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            HitungTotal();
        }

        private void HitungTotal()
        {
            try
            {
                double qty = Convert.ToDouble(guna2NumericUpDown3.Value);

                // Validasi stok
                if (qty > stock)
                {
                    MessageBox.Show("Stock tidak cukup!");

                    guna2NumericUpDown3.Value = (decimal)stock;
                    qty = stock;
                }

                // Hitung total
                double total = price * qty;

                // Contoh delivery cost (flat)
                double deliveryCost = 15000;

                double grandTotal = total + deliveryCost;

                // Tampilkan ke label
                guna2HtmlLabel12.Text = "Rp " + total.ToString("N0");
                guna2HtmlLabel11.Text = "Rp " + deliveryCost.ToString("N0");

            }
            catch
            {
                guna2HtmlLabel12.Text = "Rp 0";
                guna2HtmlLabel11.Text = "Rp 0";

            }
        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {

        }
    }
}