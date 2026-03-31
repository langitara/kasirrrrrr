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
    public partial class transactioncontrol : UserControl
    {
        DatabaseHelper dbhelper = new DatabaseHelper();
        private int selectedTransactionId = 0;
        private int selectedProductId = 0;
        private double selectedQuantity = 0;

        public transactioncontrol()
        {
            InitializeComponent();
            // apply role-specific UI and load data when control becomes visible
            ApplyRoleButtons();
            this.Load += transactioncontrol_Load;
            this.VisibleChanged += Transactioncontrol_VisibleChanged;
            // wire action buttons (ensure handlers registered)
            try
            {
                this.guna2Button1.Click -= guna2Button1_Click_Approve;
                this.guna2Button1.Click += guna2Button1_Click_Approve;
                this.guna2Button2.Click -= guna2Button2_Click_Decline;
                this.guna2Button2.Click += guna2Button2_Click_Decline;
                this.guna2Button3.Click -= guna2Button3_Click_Cancel;
                this.guna2Button3.Click += guna2Button3_Click_Cancel;
            }
            catch { }
        }

        // productId and vendorId are optional (0 = unknown). This will insert a transaction row into DB and add to Pending grid.
        public void AddPendingRow(string productName, string vendorName, double quantity, double pricePerUnit, double totalPrice, double deliveryCost, int productId = 0, int vendorId = 0)
        {
            // Ensure columns exist
            if (dataGridView2.Columns.Count == 0)
            {
                dataGridView2.Columns.Add("product_name", "product_name");
                dataGridView2.Columns.Add("vendor_name", "vendor_name");
                dataGridView2.Columns.Add("quantity", "quantity");
                dataGridView2.Columns.Add("price_per_unit", "price_per_unit");
                dataGridView2.Columns.Add("total_price", "total_price");
                dataGridView2.Columns.Add("delivery_cost", "delivery_cost");
            }

            // Insert into transactions table
            try
            {
                using (MySqlConnection conn = dbhelper.GetConnection())
                {
                    conn.Open();

                    // Note: some DB schemas use column name 'customor_id' (typo). Use that name to match your DB.
                    string insertQuery = "INSERT INTO transactions (vendor_id, customor_id, product_id, delivery_cost, status, quantity, total_price, created_at) " +
                                         "VALUES (@vendorId, @customerId, @productId, @delivery, @status, @qty, @total, NOW())";

                    MySqlCommand cmd = new MySqlCommand(insertQuery, conn);
                    int customerId = 0;
                    try { customerId = Convert.ToInt32(UserSession.UserID); } catch { customerId = 0; }

                    // If vendorId not provided, try to resolve from product record
                    if ((vendorId == 0 || vendorId == -1) && productId > 0)
                    {
                        try
                        {
                            using (MySqlCommand findVendor = new MySqlCommand("SELECT vendor_id FROM products WHERE id = @pid", conn))
                            {
                                findVendor.Parameters.AddWithValue("@pid", productId);
                                object vObj = findVendor.ExecuteScalar();
                                if (vObj != null && vObj != DBNull.Value)
                                {
                                    int resolved = 0;
                                    if (int.TryParse(vObj.ToString(), out resolved)) vendorId = resolved;
                                }
                            }
                        }
                        catch { /* ignore */ }
                    }

                    cmd.Parameters.AddWithValue("@vendorId", vendorId == 0 ? (object)DBNull.Value : vendorId);
                    cmd.Parameters.AddWithValue("@customerId", customerId == 0 ? (object)DBNull.Value : customerId);
                    cmd.Parameters.AddWithValue("@productId", productId == 0 ? (object)DBNull.Value : productId);
                    cmd.Parameters.AddWithValue("@delivery", deliveryCost);
                    // default status 0 = pending
                    cmd.Parameters.AddWithValue("@status", 0);
                    cmd.Parameters.AddWithValue("@qty", quantity);
                    double dbTotal = totalPrice + deliveryCost;
                    cmd.Parameters.AddWithValue("@total", dbTotal);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan transaksi ke database: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Add row to UI Pending grid — use same total as stored in database
            try
            {
                double displayedTotal = totalPrice + deliveryCost; // dbTotal
                dataGridView2.Rows.Add(productName, vendorName, quantity.ToString(), pricePerUnit.ToString(), displayedTotal.ToString(), deliveryCost.ToString());
            }
            catch { }

            // refresh history grid (datagridview1) so it matches database
            try { LoadTransactionsIfNeeded(); } catch { }
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            // when history row selected, show details but no action buttons
            try
            {
                if (dataGridView1.CurrentRow == null) return;
                var row = dataGridView1.CurrentRow;
                try { guna2HtmlLabel6.Text = row.Cells["product_name"].Value?.ToString() ?? ""; } catch { }
                // for history show the other party name
                if (string.Equals(UserSession.UserRole, "Vendor", StringComparison.OrdinalIgnoreCase))
                {
                    try { guna2HtmlLabel8.Text = row.Cells["customer_name"].Value?.ToString() ?? ""; } catch { }
                }
                else
                {
                    try { guna2HtmlLabel8.Text = row.Cells["vendor_name"].Value?.ToString() ?? ""; } catch { }
                }
                try { guna2HtmlLabel9.Text = row.Cells["quantity"].Value?.ToString() ?? ""; } catch { }
                try { guna2HtmlLabel12.Text = row.Cells["total_price"].Value?.ToString() ?? ""; } catch { }
                try { guna2HtmlLabel13.Text = row.Cells["delivery_cost"].Value?.ToString() ?? ""; } catch { }

                // disable action buttons for history selection
                guna2Button1.Enabled = false;
                guna2Button2.Enabled = false;
                guna2Button3.Enabled = false;
            }
            catch { }
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2.CurrentRow == null) return;
                var row = dataGridView2.CurrentRow;

                // expected columns when loaded from DB: id, product_name, customer_name/vendor_name, quantity, total_price, delivery_cost, product_id
                // try by column name then by index
                object oid = null;
                try { oid = row.Cells["id"].Value; } catch { }
                if (oid == null)
                {
                    // if DataGridView was populated manually, selectedTransactionId remains 0
                    selectedTransactionId = 0;
                }
                else int.TryParse(oid.ToString(), out selectedTransactionId);

                // product id
                selectedProductId = 0;
                try { var pid = row.Cells["product_id"].Value; if (pid != null) int.TryParse(pid.ToString(), out selectedProductId); } catch { }

                // quantity
                selectedQuantity = 0;
                try { var q = row.Cells["quantity"].Value; if (q != null) double.TryParse(q.ToString(), out selectedQuantity); } catch { }

                // fill details labels
                try { guna2HtmlLabel6.Text = row.Cells["product_name"].Value?.ToString() ?? ""; } catch { }
                // vendor/customer name on right label depends on role
                if (string.Equals(UserSession.UserRole, "Vendor", StringComparison.OrdinalIgnoreCase))
                {
                    try { guna2HtmlLabel8.Text = row.Cells["customer_name"].Value?.ToString() ?? ""; } catch { }
                }
                else
                {
                    try { guna2HtmlLabel8.Text = row.Cells["vendor_name"].Value?.ToString() ?? ""; } catch { }
                }

                try { guna2HtmlLabel9.Text = selectedQuantity.ToString(); } catch { }
                try { guna2HtmlLabel12.Text = row.Cells["total_price"].Value?.ToString() ?? ""; } catch { }
                try { guna2HtmlLabel13.Text = row.Cells["delivery_cost"].Value?.ToString() ?? ""; } catch { }

                // enable buttons according to role
                if (string.Equals(UserSession.UserRole, "Vendor", StringComparison.OrdinalIgnoreCase))
                {
                    guna2Button1.Enabled = true; // approve
                    guna2Button2.Enabled = true; // decline
                    guna2Button3.Enabled = false;
                }
                else if (string.Equals(UserSession.UserRole, "Customer", StringComparison.OrdinalIgnoreCase))
                {
                    guna2Button1.Enabled = false;
                    guna2Button2.Enabled = false;
                    guna2Button3.Enabled = true; // cancel
                }
            }
            catch { }
        }

        private void guna2HtmlLabel2_Click(object sender, EventArgs e)
        {

        }

        private void guna2GroupBox3_Click(object sender, EventArgs e)
        {
            
        }

        private void guna2HtmlLabel6_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }

        private void ApplyRoleButtons()
        {
            try
            {
                // default hide both
                guna2Button1.Visible = false;
                guna2Button2.Visible = false;

                if (string.Equals(UserSession.UserRole, "Vendor", StringComparison.OrdinalIgnoreCase))
                {
                    // vendor: show Approve + Decline
                    guna2Button1.Visible = true;
                    guna2Button2.Visible = true;
                    guna2Button3.Visible = false;
                    try { guna2Button1.Text = "Approve"; } catch { }
                    try { guna2Button2.Text = "Decline"; } catch { }
                }
                else if (string.Equals(UserSession.UserRole, "Customer", StringComparison.OrdinalIgnoreCase))
                {
                    // customer: show single Cancel Transaction (use guna2Button2)
                    guna2Button1.Visible = false;
                    guna2Button2.Visible = false;
                    guna2Button3.Visible = true;
                    try { guna2Button3.Text = "Cancel Transaction"; } catch { }
                }
                else
                {
                    // default show both
                    guna2Button1.Visible = true;
                    guna2Button2.Visible = true;
                    guna2Button3.Visible = true;
                }
            }
            catch { }
        }

        private void guna2Button1_Click_Approve(object sender, EventArgs e)
        {
            // Approve selected pending transaction
            if (selectedTransactionId == 0)
            {
                MessageBox.Show("Pilih transaksi terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = dbhelper.GetConnection())
                {
                    conn.Open();
                    string q = "UPDATE transactions SET status = 1, update_at = NOW() WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@id", selectedTransactionId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Transaksi disetujui.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTransactionsIfNeeded();
                LoadPendingTransactions();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal approve transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Button2_Click_Decline(object sender, EventArgs e)
        {
            // Decline selected pending transaction and restock
            if (selectedTransactionId == 0 || selectedProductId == 0 || selectedQuantity <= 0)
            {
                MessageBox.Show("Pilih transaksi yang valid terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = dbhelper.GetConnection())
                {
                    conn.Open();
                    // set transaction failed
                    string q = "UPDATE transactions SET status = 2, update_at = NOW() WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@id", selectedTransactionId);
                    cmd.ExecuteNonQuery();

                    // restock product
                    string up = "UPDATE products SET unit_stock = unit_stock + @qty, updated_at = NOW() WHERE id = @pid";
                    MySqlCommand ucmd = new MySqlCommand(up, conn);
                    ucmd.Parameters.AddWithValue("@qty", selectedQuantity);
                    ucmd.Parameters.AddWithValue("@pid", selectedProductId);
                    ucmd.ExecuteNonQuery();
                }

                MessageBox.Show("Transaksi ditolak dan stok dikembalikan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTransactionsIfNeeded();
                LoadPendingTransactions();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal decline transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Button3_Click_Cancel(object sender, EventArgs e)
        {
            // Customer cancels pending transaction -> set failed and restock
            if (selectedTransactionId == 0 || selectedProductId == 0 || selectedQuantity <= 0)
            {
                MessageBox.Show("Pilih transaksi yang valid terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dr = MessageBox.Show("Yakin ingin membatalkan transaksi ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr != DialogResult.Yes) return;

            try
            {
                using (MySqlConnection conn = dbhelper.GetConnection())
                {
                    conn.Open();
                    string q = "UPDATE transactions SET status = 2, update_at = NOW() WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@id", selectedTransactionId);
                    cmd.ExecuteNonQuery();

                    // restock
                    string up = "UPDATE products SET unit_stock = unit_stock + @qty, updated_at = NOW() WHERE id = @pid";
                    MySqlCommand ucmd = new MySqlCommand(up, conn);
                    ucmd.Parameters.AddWithValue("@qty", selectedQuantity);
                    ucmd.Parameters.AddWithValue("@pid", selectedProductId);
                    ucmd.ExecuteNonQuery();
                }

                MessageBox.Show("Transaksi dibatalkan dan stok dikembalikan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTransactionsIfNeeded();
                LoadPendingTransactions();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membatalkan transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Transactioncontrol_VisibleChanged(object sender, EventArgs e)
        {
            ApplyRoleButtons();
            LoadTransactionsIfNeeded();
        }

        private void transactioncontrol_Load(object sender, EventArgs e)
        {
            ApplyRoleButtons();
            LoadTransactionsIfNeeded();
        }

        private void LoadTransactionsIfNeeded()
        {
            if (!this.Visible) return;
            if (string.Equals(UserSession.UserRole, "Vendor", StringComparison.OrdinalIgnoreCase) || string.Equals(UserSession.UserRole, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                LoadTransactions();
            }
        }

        private void LoadTransactions()
        {
            try
            {
                using (MySqlConnection conn = dbhelper.GetConnection())
                {
                    conn.Open();
                    // unified history query (status 1 and 2) for both roles
                    string query = @"
                        SELECT
                            t.id,
                            t.product_id,
                            p.products_name AS product_name,
                            v.vendor_name AS vendor_name,
                            c.cust_name AS customer_name,
                            t.quantity,
                            t.total_price,
                            t.delivery_cost,
                            t.status,
                            CASE WHEN t.status = 0 THEN 'Pending' WHEN t.status = 1 THEN 'Approved' WHEN t.status = 2 THEN 'Failed' ELSE 'Unknown' END AS status_text,
                            t.created_at
                        FROM transactions t
                        LEFT JOIN products p ON t.product_id = p.id
                        LEFT JOIN users v ON t.vendor_id = v.id
                        LEFT JOIN users c ON t.customor_id = c.id
                        WHERE ((t.vendor_id = @userId OR t.customor_id = @userId) OR (t.product_id IN (SELECT id FROM products WHERE vendor_id = @userId))) AND t.status IN (1,2)
                        ORDER BY t.created_at DESC
                    ";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    int uid = 0; try { uid = Convert.ToInt32(UserSession.UserID); } catch { uid = 0; }
                    cmd.Parameters.AddWithValue("@userId", uid);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dataGridView1.DataSource = dt;
                    // also load pending list
                    LoadPendingTransactions();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPendingTransactions()
        {
            try
            {
                using (MySqlConnection conn = dbhelper.GetConnection())
                {
                    conn.Open();
                    // unified pending query (status = 0) for both roles
                    string query = @"
                        SELECT
                            t.id,
                            t.product_id,
                            p.products_name AS product_name,
                            v.vendor_name AS vendor_name,
                            c.cust_name AS customer_name,
                            t.quantity,
                            t.total_price,
                            t.delivery_cost,
                            t.status,
                            CASE WHEN t.status = 0 THEN 'Pending' WHEN t.status = 1 THEN 'Approved' WHEN t.status = 2 THEN 'Failed' ELSE 'Unknown' END AS status_text,
                            t.created_at
                        FROM transactions t
                        LEFT JOIN products p ON t.product_id = p.id
                        LEFT JOIN users v ON t.vendor_id = v.id
                        LEFT JOIN users c ON t.customor_id = c.id
                        WHERE ((t.vendor_id = @userId OR t.customor_id = @userId) OR (t.product_id IN (SELECT id FROM products WHERE vendor_id = @userId))) AND t.status = 0
                        ORDER BY t.created_at DESC
                    ";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    int uid = 0; try { uid = Convert.ToInt32(UserSession.UserID); } catch { uid = 0; }
                    cmd.Parameters.AddWithValue("@userId", uid);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dataGridView2.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat pending transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
