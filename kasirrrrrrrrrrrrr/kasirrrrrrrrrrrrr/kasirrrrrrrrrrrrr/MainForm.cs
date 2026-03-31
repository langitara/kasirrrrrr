using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kasirrrrrrrrrrrrr
{
    public partial class MainForm : Form
    {
        private transactioncontrol _transactionControlInstance;
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            guna2HtmlLabel1.Text = "Welcome, " + $"<font color='#808080'>{UserSession.Username}</font>";

           
            guna2HtmlLabel2.Text = "Login As, " + $"<font color='#808080'>{UserSession.UserRole}</font>";

            
        }

        private void addUserControl(UserControl userControl)
        {
            userControl.Dock = DockStyle.Fill; 
            guna2Panel2.Controls.Clear();        
            guna2Panel2.Controls.Add(userControl); 
            userControl.BringToFront();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            ProfileControl profile = new ProfileControl();
            addUserControl(profile);
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            productcontrol pc = new productcontrol();
            addUserControl(pc);

            
        }

        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            if (_transactionControlInstance == null)
            {
                _transactionControlInstance = new transactioncontrol();
            }
            addUserControl(_transactionControlInstance);
        }

        // Called by other controls to show transaction view and add a pending row
        public void ShowTransactionWithPendingRow(string productName, string vendorName, double quantity, double pricePerUnit, double totalPrice, double deliveryCost, int productId = 0, int vendorId = 0)
        {
            if (_transactionControlInstance == null)
            {
                _transactionControlInstance = new transactioncontrol();
            }
            addUserControl(_transactionControlInstance);
            try
            {
                _transactionControlInstance.AddPendingRow(productName, vendorName, quantity, pricePerUnit, totalPrice, deliveryCost, productId, vendorId);
            }
            catch
            {
                // ignore errors
            }
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            LoginForm newLogin = new LoginForm();
            newLogin.Show();
            this.Close();

        }
    }
}
