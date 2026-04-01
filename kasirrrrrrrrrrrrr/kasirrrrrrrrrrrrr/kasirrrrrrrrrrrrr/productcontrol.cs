using MySql.Data.MySqlClient;
using Mysqlx.Crud;
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

        private int selectedID = 0;
        private string selectedVendorName = null;
        DatabaseHelper dbhelper = new DatabaseHelper();
        public productcontrol()
        {
            InitializeComponent();

            LoadCategories();
            LoadData(); ;

            ApplyRoleVisibility();
        }

        private void ApplyRoleVisibility()
        {
            try
            {
                // Show transaction area only for Customer, show details only for Vendor
                if (string.Equals(UserSession.UserRole, "Customer", StringComparison.OrdinalIgnoreCase))
                {
                    guna2GroupBox3.Visible = true;   // transaction area
                    guna2GroupBox2.Visible = false;  // details area
                }
                else if (string.Equals(UserSession.UserRole, "Vendor", StringComparison.OrdinalIgnoreCase))
                {
                    guna2GroupBox3.Visible = false;
                    guna2GroupBox2.Visible = true;
                }
                else
                {
                    // default: show both
                    guna2GroupBox3.Visible = true;
                    guna2GroupBox2.Visible = true;
                }
            }
            catch
            {
                // ignore errors and leave both visible
                guna2GroupBox3.Visible = true;
                guna2GroupBox2.Visible = true;
            }
        }

        private void LoadCategories()
        {
            using (MySqlConnection conn = dbhelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT id, name FROM categories WHERE is_active = 1 ORDER BY name";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    comboBox1.DisplayMember = "name";
                    comboBox1.ValueMember = "id";
                    comboBox1.DataSource = dt;
                    comboBox1.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Load Categories: " + ex.Message);
                }
            }
        }

        //private void guna2Button1_Click(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrEmpty(UserSession.UserID))
        //    {
        //        MessageBox.Show("Sesi login habis, silahkan login ulang.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    if (string.IsNullOrEmpty(guna2TextBox1.Text))
        //    {
        //        MessageBox.Show("Nama produk tidak boleh kosong!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    int statusValue = (comboBox2.Text == "active") ? 1 : 0;

        //    using (MySqlConnection conn = dbhelper.GetConnection())
        //    {
        //        try
        //        {
        //            string query = "INSERT INTO products (products_name, price_per_unit, unit_stock, vendor_id, is_active, category_id) VALUES (@name, @price, @stock, @vendorId, @active, @categoryId)";

        //            MySqlCommand cmd = new MySqlCommand(query, conn);
        //            cmd.Parameters.AddWithValue("@name", guna2TextBox1.Text);
        //            cmd.Parameters.AddWithValue("@price", guna2NumericUpDown1.Value);
        //            cmd.Parameters.AddWithValue("@stock", guna2NumericUpDown2.Value);
        //            cmd.Parameters.AddWithValue("@vendorId", UserSession.UserID);
        //            cmd.Parameters.AddWithValue("@active", comboBox2.Text);
        //            cmd.Parameters.AddWithValue("@categoryId",comboBox1.Text);

        //            conn.Open();
        //            cmd.ExecuteNonQuery();

        //            MessageBox.Show("Produk berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

        //            LoadData();
        //            ClearForm();
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("Error Simpan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //    }
        //}

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

            // 1. Konversi status menjadi integer (comboBox2 holds status in designer)
            int statusValue = (comboBox2.Text == "active") ? 1 : 0;

            using (MySqlConnection conn = dbhelper.GetConnection())
            {
                try
                {
                    // Pastikan kolom di database sesuai (is_active dan category_id)
                    string query = "INSERT INTO products (products_name, price_per_unit, unit_stock, vendor_id, is_active, category_id, unit_type) " +
                                   "VALUES (@name, @price, @stock, @vendorId, @active, @categoryId, @type)";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", guna2TextBox1.Text);
                    cmd.Parameters.AddWithValue("@price", guna2NumericUpDown1.Value);
                    cmd.Parameters.AddWithValue("@stock", guna2NumericUpDown2.Value);
                    cmd.Parameters.AddWithValue("@vendorId", UserSession.UserID);


                    // 2. Kirim statusValue (int), bukan comboBox2.Text (string)
                    cmd.Parameters.AddWithValue("@active", statusValue);

                    // 3. Ambil SelectedValue (ID) dari comboBox2 yang berisi kategori
                    if (comboBox1.SelectedValue != null)
                    {
                        cmd.Parameters.AddWithValue("@categoryId", comboBox1.SelectedValue);
                    }
                    else
                    {
                        MessageBox.Show("Silahkan pilih kategori yang valid!");
                        return;
                    }

                    // Ambil unit_type dari radio button
                    string unitType = null;
                    if (guna2RadioButton1.Checked) unitType = "countable";
                    else if (guna2RadioButton2.Checked) unitType = "measureable";
                    cmd.Parameters.AddWithValue("@type", (object)unitType ?? DBNull.Value);

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
                    string query;
                    // Build query based on role: Customer sees vendor + product columns, Vendor sees product + status
                    if (string.Equals(UserSession.UserRole, "Customer", StringComparison.OrdinalIgnoreCase))
                    {
                        // include vendor_id so we can create transactions
                        query = "SELECT p.id, p.vendor_id, u.vendor_name, p.products_name, c.name AS category_name, p.unit_type, p.price_per_unit, p.unit_stock " +
                                "FROM products p " +
                                "LEFT JOIN categories c ON p.category_id = c.id " +
                                "LEFT JOIN users u ON p.vendor_id = u.id " +
                                "WHERE p.deleted_at IS NULL";
                    }
                    else if (string.Equals(UserSession.UserRole, "Vendor", StringComparison.OrdinalIgnoreCase))
                    {
                        query = "SELECT p.id, p.products_name, c.name AS category_name, p.unit_type, p.price_per_unit, p.unit_stock, p.is_active " +
                                "FROM products p " +
                                "LEFT JOIN categories c ON p.category_id = c.id " +
                                "WHERE p.deleted_at IS NULL";
                    }
                    else
                    {
                        // default: full set
                        query = "SELECT p.id, u.vendor_name, p.products_name, c.name AS category_name, p.unit_type, p.price_per_unit, p.unit_stock, p.is_active, p.category_id " +
                                "FROM products p " +
                                "LEFT JOIN categories c ON p.category_id = c.id " +
                                "LEFT JOIN users u ON p.vendor_id = u.id " +
                                "WHERE p.deleted_at IS NULL";
                    }

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
            selectedID = 0;
        }


        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = guna2DataGridView1.Rows[e.RowIndex];

            selectedID = Convert.ToInt32(row.Cells["id"].Value);
            guna2TextBox1.Text = row.Cells["products_name"].Value.ToString();
            guna2NumericUpDown1.Value = Convert.ToDecimal(row.Cells["price_per_unit"].Value);
            guna2NumericUpDown2.Value = Convert.ToDecimal(row.Cells["unit_stock"].Value);
            // map numeric is_active to text values expected by comboBox2 (e.g. "active"/"inactive")
            try
            {
                var isActiveVal = row.Cells["is_active"].Value;
                if (isActiveVal != null && isActiveVal != DBNull.Value)
                {
                    int ia = Convert.ToInt32(isActiveVal);
                    comboBox2.Text = (ia == 1) ? "active" : "inactive";
                }
                else
                {
                    comboBox2.SelectedIndex = -1;
                } 
            }
            catch
            {
                comboBox2.SelectedIndex = -1;
            }
            // set category selection if available
            try
            {
                var catVal = row.Cells["category_id"].Value;
                if (catVal != null && catVal != DBNull.Value)
                {
                    comboBox1.SelectedValue = Convert.ToInt32(catVal);
                }
                else
                {
                    comboBox1.SelectedIndex = -1;
                }
            }
            catch
            {
                comboBox1.SelectedIndex = -1;
            }

            // try get vendor_name if present in grid
            try
            {
                var v = row.Cells["vendor_name"];
                if (v != null && v.Value != null && v.Value != DBNull.Value)
                {
                    selectedVendorName = v.Value.ToString();
                }
                else
                {
                    selectedVendorName = null;
                }
            }
            catch
            {
                selectedVendorName = null;
            }

            // set unit_type radio buttons
            try
            {
                var ut = row.Cells["unit_type"].Value;
                if (ut != null && ut != DBNull.Value)
                {
                    string uts = ut.ToString().ToLower();
                    if (uts.Contains("count"))
                    {
                        guna2RadioButton1.Checked = true;
                        guna2RadioButton2.Checked = false;
                    }
                    else if (uts.Contains("measure"))
                    {
                        guna2RadioButton2.Checked = true;
                        guna2RadioButton1.Checked = false;
                    }
                    else
                    {
                        guna2RadioButton1.Checked = false;
                        guna2RadioButton2.Checked = false;
                    }
                }
                else
                {
                    guna2RadioButton1.Checked = false;
                    guna2RadioButton2.Checked = false;
                }
            }
            catch
            {
                guna2RadioButton1.Checked = false;
                guna2RadioButton2.Checked = false;
            }
            

            price = Convert.ToDouble(row.Cells["price_per_unit"].Value);
            stock = Convert.ToDouble(row.Cells["unit_stock"].Value);

            guna2NumericUpDown3.Value = 1;
            HitungTotal();


        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

            if (selectedID == 0)
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
                            is_active = @active,
                            category_id = @categoryId,
                            unit_type = @type,
                            updated_at = NOW()
                            WHERE id = @id";

                    MySqlCommand cmd = new MySqlCommand(query, conn);


                    cmd.Parameters.AddWithValue("@name", guna2TextBox1.Text);
                    cmd.Parameters.AddWithValue("@price", guna2NumericUpDown1.Value);
                    cmd.Parameters.AddWithValue("@stock", guna2NumericUpDown2.Value);
                    // send numeric status value for update (comboBox2 holds status)
                    int statusValue = (comboBox2.Text == "active") ? 1 : 0;
                    cmd.Parameters.AddWithValue("@active", statusValue);
                    object categoryVal = comboBox1.SelectedValue ?? (object)DBNull.Value;
                    cmd.Parameters.AddWithValue("@categoryId", categoryVal);

                    // unit_type from radio buttons
                    string unitType = null;
                    if (guna2RadioButton1.Checked) unitType = "countable";
                    else if (guna2RadioButton2.Checked) unitType = "measureable";
                    cmd.Parameters.AddWithValue("@type", (object)unitType ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@id", selectedID);


                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Data berhasil diupdate!", "Sukses");

                    LoadData();
                    ClearForm();
                    selectedID = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Update: " + ex.Message);
                }
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            if (selectedID == 0)
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

                        string query = "UPDATE products SET deleted_at = NOW() WHERE id=@id";
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", selectedID);

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Data berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                            selectedID = 0;
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
            if (selectedID == 0)
            {
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


        // guard to prevent reentrant ValueChanged when we set the numeric value programmatically
        private bool _suppressQuantityEvent = false;
        private void guna2NumericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (_suppressQuantityEvent) return;
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
                    // clamp silently here; actual user warning is shown when attempting to buy
                    try
                    {
                        _suppressQuantityEvent = true;
                        guna2NumericUpDown3.Value = (decimal)stock;
                        qty = stock;
                    }
                    finally
                    {
                        _suppressQuantityEvent = false;
                    }
                }

                // Hitung total
                double total = price * qty;

                // PPN 11% sebagai biaya tambahan (deliveryCost digunakan sebagai PPN di permintaan)
                double deliveryCost = Math.Round(total * 0.11, 2);
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
            // Determine current selected row in products grid
            DataGridViewRow row = null;
            if (guna2DataGridView1.CurrentRow != null && guna2DataGridView1.CurrentRow.Index >= 0)
                row = guna2DataGridView1.CurrentRow;
            else if (selectedID != 0)
            {
                foreach (DataGridViewRow r in guna2DataGridView1.Rows)
                {
                    try
                    {
                        if (r.Cells["id"].Value != null && Convert.ToInt32(r.Cells["id"].Value) == selectedID)
                        {
                            row = r; break;
                        }
                    }
                    catch { }
                }
            }

            if (row == null)
            {
                MessageBox.Show("Silahkan pilih produk terlebih dahulu dari tabel.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Read product data from the row
            string productName = string.Empty;
            string vendorName = selectedVendorName ?? UserSession.Username;
            double pricePerUnit = price;
            double qty = Convert.ToDouble(guna2NumericUpDown3.Value);

            try { productName = row.Cells["products_name"].Value?.ToString() ?? row.Cells["product_name"].Value?.ToString() ?? guna2TextBox1.Text; } catch { productName = guna2TextBox1.Text; }
            try { vendorName = row.Cells["vendor_name"].Value?.ToString() ?? vendorName; } catch { }
            try { pricePerUnit = Convert.ToDouble(row.Cells["price_per_unit"].Value ?? pricePerUnit); } catch { }

            // validate qty
            if (qty <= 0)
            {
                MessageBox.Show("Quantity harus lebih dari 0.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (qty > stock)
            {
                // Inform user and adjust the numeric control to the remaining stock.
                MessageBox.Show("Stock tidak cukup!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                try
                {
                    _suppressQuantityEvent = true;
                    guna2NumericUpDown3.Value = (decimal)stock;
                }
                finally
                {
                    _suppressQuantityEvent = false;
                }
                // update totals/labels to reflect adjusted qty
                HitungTotal();
                return;
            }

            double totalWithoutDelivery = pricePerUnit * qty;
            
            double deliveryCost = Math.Round(totalWithoutDelivery * 0.11, 2);

            var main = this.FindForm() as MainForm;
            if (main != null)
            {
                try
                {
                    // First, decrement stock in database
                    int productId = 0;
                    try
                    {
                        productId = Convert.ToInt32(row.Cells["id"].Value);
                    }
                    catch { productId = selectedID; }

                    using (MySqlConnection conn = dbhelper.GetConnection())
                    {
                        conn.Open();
                        string updateQuery = "UPDATE products SET unit_stock = unit_stock - @qty, updated_at = NOW() WHERE id = @id";
                        MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn);
                        updateCmd.Parameters.AddWithValue("@qty", qty);
                        updateCmd.Parameters.AddWithValue("@id", productId);
                        int affected = updateCmd.ExecuteNonQuery();
                        if (affected <= 0)
                        {
                            MessageBox.Show("Gagal mengurangi stok produk. ID mungkin tidak ditemukan.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // refresh product list so UI shows reduced stock
                    LoadData();

                    // update local stock variable to reflect change
                    stock = Math.Max(0, stock - qty);
                    

                    // add to pending transactions view
                    // determine vendor id
                    int vendorId = 0;
                    try { vendorId = Convert.ToInt32(row.Cells["vendor_id"].Value); } catch { vendorId = 0; }

                    main.ShowTransactionWithPendingRow(productName, vendorName, qty, pricePerUnit, totalWithoutDelivery, deliveryCost, productId: productId, vendorId: vendorId);
                    MessageBox.Show("Item berhasil dibeli dan ditambahkan ke Pending Transactions.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menambahkan transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Tidak dapat menemukan MainForm untuk menampilkan transaksi.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {


        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            
               
            
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
