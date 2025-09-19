using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TourismManagement
{
    public partial class frmVendorValidation : Form
    {
        public frmVendorValidation()
        {
            InitializeComponent();
        }
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmVendorValidation_Load(object sender, EventArgs e)
        {
            BindData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string vendorId = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(vendorId))
            {
                MessageBox.Show("Please enter a Vendor ID to search.");
                return;
            }

            DataTable dt = new DataTable();

            string query = @"SELECT VendorID, Name, Email, Gender, VendorType, Status 
                     FROM Vendor_Info 
                     WHERE VendorID = @VendorID AND Status = 'Invalid'";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@VendorID", vendorId);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            if (dt.Rows.Count > 0)
            {
                dataGridView1.DataSource = dt; // show the invalid vendor
                FormatVendorValidationGrid();  // apply formatting
            }
            else
            {
                BindData();
            }

        }

       


        private void button2_Click(object sender, EventArgs e)
        {

            SendEmail se = new SendEmail();

            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Please search a vendor first.");
                return;
            }

            string vendorId = dataGridView1.Rows[0].Cells["VendorID"].Value.ToString();

            string query = "UPDATE Vendor_Info SET Status = 'Valid' WHERE VendorID = @VendorID";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@VendorID", vendorId);
                con.Open();
                int rows = cmd.ExecuteNonQuery();
                con.Close();

                if (rows > 0)
                {
                    MessageBox.Show("Vendor validated successfully!");

                    // ✅ Fetch vendor info
                    string selectQuery = @"SELECT VendorID, Name, Email, VendorType, Password 
                               FROM Vendor_Info 
                               WHERE VendorID = @VendorID";

                    using (SqlCommand cmd2 = new SqlCommand(selectQuery, con))
                    {
                        cmd2.Parameters.AddWithValue("@VendorID", vendorId);
                        con.Open();
                        using (SqlDataReader reader = cmd2.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string vId = reader["VendorID"].ToString();
                                string name = reader["Name"].ToString();
                                string email = reader["Email"].ToString();
                                string userType = reader["VendorType"].ToString();
                                string password = reader["Password"].ToString();

                               se.EmailVendorActivation(vId, name, userType, email, password);
                            }
                        }
                        con.Close();
                    }

                    dataGridView1.DataSource = null; 
                }
                else
                {
                    MessageBox.Show("Failed to update vendor status.");
                }
            }
        }

        public void BindData()
        {
            DataTable dt = new DataTable();

            string query = @"SELECT VendorID, Name, Email, Gender, VendorType, Status 
                     FROM Vendor_Info 
                     WHERE Status = 'Invalid'";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            dataGridView1.DataSource = dt;
            FormatVendorValidationGrid(); // apply formatting
        }

        private void FormatVendorValidationGrid()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 10, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 9);

            if (dataGridView1.Columns.Contains("VendorID"))
                dataGridView1.Columns["VendorID"].HeaderText = "Vendor ID";

            if (dataGridView1.Columns.Contains("Name"))
                dataGridView1.Columns["Name"].HeaderText = "Vendor Name";

            if (dataGridView1.Columns.Contains("Email"))
                dataGridView1.Columns["Email"].HeaderText = "Email Address";

            if (dataGridView1.Columns.Contains("Gender"))
                dataGridView1.Columns["Gender"].HeaderText = "Gender";

            if (dataGridView1.Columns.Contains("VendorType"))
                dataGridView1.Columns["VendorType"].HeaderText = "Vendor Type";

            if (dataGridView1.Columns.Contains("Status"))
                dataGridView1.Columns["Status"].HeaderText = "Account Status";

            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSkyBlue;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }


    }
}
