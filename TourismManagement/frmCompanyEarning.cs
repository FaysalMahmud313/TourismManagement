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
    public partial class frmCompanyEarning : Form
    {
        public frmCompanyEarning()
        {
            InitializeComponent();
        }

        

        private void LoadCompanyEarnings()
        {
            SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"); 
            
                string query = "SELECT TransactionID, ReceivedAmount, TotalBalance, TransactionDate FROM Company_Wallet ORDER BY TransactionID ASC";
                using (SqlDataAdapter da = new SqlDataAdapter(query, con))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;

                FormatCompanyGrid();
                }
            
        }

       

        private void FormatCompanyGrid()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 10, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 9);

            dataGridView1.Columns["TransactionID"].HeaderText = "Transaction ID";
            dataGridView1.Columns["ReceivedAmount"].HeaderText = "Company Share (15%)";
            dataGridView1.Columns["TotalBalance"].HeaderText = "Total Balance";
            dataGridView1.Columns["TransactionDate"].HeaderText = "Date";

            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        private void frmCompanyEarning_Load(object sender, EventArgs e)
        {
            LoadCompanyEarnings();
        }
    }
}
