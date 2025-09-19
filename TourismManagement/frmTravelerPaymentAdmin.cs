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
    public partial class frmTravelerPaymentAdmin : Form
    {
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        public frmTravelerPaymentAdmin()
        {
            InitializeComponent();
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void frmTravelerPaymentAdmin_Load(object sender, EventArgs e)
        {
            LoadPaymentInfo();
        }

        private void LoadPaymentInfo()
        {
            try
            {
                con.Open();
                string query = "SELECT * FROM Payment_Info";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt; // show results in DataGridView

                FormatPaymentGrid(); // call styling method
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        private void FormatPaymentGrid()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 10, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 9);

            // Set column headers (rename as per your actual DB columns)
            if (dataGridView1.Columns.Contains("PaymentID"))
                dataGridView1.Columns["PaymentID"].HeaderText = "Payment ID";

            if (dataGridView1.Columns.Contains("TravelerID"))
                dataGridView1.Columns["TravelerID"].HeaderText = "Traveler ID";

            if (dataGridView1.Columns.Contains("BookingID"))
                dataGridView1.Columns["BookingID"].HeaderText = "Booking ID";

            if (dataGridView1.Columns.Contains("Amount"))
                dataGridView1.Columns["Amount"].HeaderText = "Payment Amount";

            if (dataGridView1.Columns.Contains("PaymentDate"))
                dataGridView1.Columns["PaymentDate"].HeaderText = "Payment Date";

            if (dataGridView1.Columns.Contains("PaymentMethod"))
                dataGridView1.Columns["PaymentMethod"].HeaderText = "Method";

            // Header color
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGreen;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }
    }
}
