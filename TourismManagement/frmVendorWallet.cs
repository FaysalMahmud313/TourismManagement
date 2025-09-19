using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TourismManagement
{
    public partial class frmVendorWallet : Form
    {
        public frmVendorWallet()
        {
            InitializeComponent();
        }


        private void LoadVendorWallet()
        {
            SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
            
                string query = @"SELECT WalletID, VendorID, TotalEarning 
                         FROM Vendor_Wallet
                         ORDER BY WalletID ASC"; // ascending order
                using (SqlDataAdapter da = new SqlDataAdapter(query, con))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;

                    FormatVendorGrid();
                }
            
        }

        private void FormatVendorGrid()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 10, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 9);
                
            dataGridView1.Columns["WalletID"].HeaderText = "Wallet ID";
            dataGridView1.Columns["VendorID"].HeaderText = "Vendor ID";
            dataGridView1.Columns["TotalEarning"].HeaderText = "Total Earnings";

            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGreen;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        private void frmVendorWallet_Load(object sender, EventArgs e)
        {
            LoadVendorWallet();
        }
    }
}
