using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TourismManagement
{
    public partial class frmVendorRting : Form
    {
        public frmVendorRting()
        {
            InitializeComponent();
        }
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");

        public void BindData()
        {
            DataTable dt = new DataTable();

            string query = @"SELECT ReviewID, VendorID,BookingID, ServiceType, Rating, ReviewText, ReviewDate 
                     FROM Review";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            dataGridView1.DataSource = dt;
            FormatVendorValidationGrid(); 
        }

        private void FormatVendorValidationGrid()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 10, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 9);

            if (dataGridView1.Columns.Contains("ReviewID"))
                dataGridView1.Columns["ReviewID"].HeaderText = "Review ID";

            if (dataGridView1.Columns.Contains("VendorID"))
                dataGridView1.Columns["VendorID"].HeaderText = "Vendor ID";

            if (dataGridView1.Columns.Contains("ServiceType"))
                dataGridView1.Columns["ServiceType"].HeaderText = "Service Type";

            if (dataGridView1.Columns.Contains("Rating"))
                dataGridView1.Columns["Rating"].HeaderText = "Rating";

            if (dataGridView1.Columns.Contains("ReviewText"))
            {
                dataGridView1.Columns["ReviewText"].HeaderText = "Review Text";
                dataGridView1.Columns["ReviewText"].DefaultCellStyle.WrapMode = DataGridViewTriState.True; // ✅ Wrap text
            }

            if (dataGridView1.Columns.Contains("ReviewDate"))
                dataGridView1.Columns["ReviewDate"].HeaderText = "Review Date";

            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.IndianRed;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

           
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string vendorID = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(vendorID))
            {
                MessageBox.Show("Please enter a Vendor ID to search!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataTable dt = new DataTable();

            try
            {
                con.Open();

                
                string query = @"SELECT ReviewID, VendorID, ServiceType, Rating, ReviewText, ReviewDate
                         FROM Review
                         WHERE VendorID = @VendorID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@VendorID", vendorID);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    dataGridView1.DataSource = dt;
                    FormatVendorValidationGrid();

                    MessageBox.Show("User found!!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    
                    label1.Text = vendorID; 
                }
                else
                {
                    MessageBox.Show("No reviews found for this Vendor!", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    label1.Text = ""; 
                }
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

        private void button2_Click(object sender, EventArgs e)
        {
            string vendorID = label1.Text; 

            if (string.IsNullOrEmpty(vendorID))
            {
                MessageBox.Show("Please search a vendor first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult confirm = MessageBox.Show("Are you sure you want to delete this vendor?",
                                                   "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            con.Open();
            SqlTransaction tran = con.BeginTransaction();

            try
            {
                SqlCommand cmd1 = new SqlCommand("DELETE FROM Vendor_Info WHERE VendorID = @VendorID", con, tran);
                cmd1.Parameters.AddWithValue("@VendorID", vendorID);
                cmd1.ExecuteNonQuery();

                SqlCommand cmd2 = new SqlCommand("DELETE FROM Login WHERE UserID = @VendorID", con, tran);
                cmd2.Parameters.AddWithValue("@VendorID", vendorID);
                cmd2.ExecuteNonQuery();

                tran.Commit();

                MessageBox.Show("Vendor removed successfully!", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                
                textBox1.Clear();
                label1.Text = "";

               
                BindData();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        private void frmVendorRting_Load(object sender, EventArgs e)
        {
            BindData();
            label1.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string vendorID = label1.Text;
            if (string.IsNullOrEmpty(vendorID))
            {
                MessageBox.Show("Please search a vendor first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            
            string vendorName = "";
            string vendorEmail = "";

            try
            {
               
                string query = "SELECT Name, Email FROM Vendor_Info WHERE VendorID = @VendorID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@VendorID", vendorID);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    vendorName = dr["Name"].ToString();
                    vendorEmail = dr["Email"].ToString();
                }
                else
                {
                    MessageBox.Show("Vendor not found in database!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    con.Close();
                    return;
                }
                con.Close();

                
                string body = $@"
Hello {vendorName},

⚠️ Warning Notice ⚠️

We have observed that your recent reviews are continuously negative. 
This is a matter of concern, and we strongly advise you to take immediate steps to improve your service.

Details:
----------------------------------
Vendor ID   : {vendorID}
Name        : {vendorName}
Email       : {vendorEmail}
----------------------------------

👉 Please improve the quality of your service at the earliest. 
Failure to do so may result in further administrative action against your account.

Regards,
Tourism Management Admin Team
";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("teambrainstomers05@gmail.com"); 
                mail.To.Add(vendorEmail);                           
                mail.Subject = "⚠️ Warning Notice - Negative Reviews";
                mail.Body = body;

                
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential("teambrainstomers05@gmail.com", "bgeo elqz itqw wyvs");
                smtp.EnableSsl = true;

                smtp.Send(mail);

                MessageBox.Show("Warning email sent successfully!", "Email Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending email: " + ex.Message, "Email Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                con.Close();
            }
        }
    }
}
