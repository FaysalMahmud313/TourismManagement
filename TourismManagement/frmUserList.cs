using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TourismManagement
{
    public partial class frmUserList : Form
    {
        
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        public frmUserList()
        {
            InitializeComponent();
            
           
        }

        private void frmUserList_Load(object sender, EventArgs e)
        {
            LoadAdminTravelerVendor();
        }

        private void LoadAdminTravelerVendor()
        {
            try
            {
                string query = @"
            WITH AdminCTE AS (
                SELECT AdminID, ROW_NUMBER() OVER (ORDER BY AdminID) AS RowNum
                FROM Admin_Info),
            TravelerCTE AS (
                SELECT TravelerID, ROW_NUMBER() OVER (ORDER BY TravelerID) AS RowNum
                FROM Traveler_Info),
            VendorCTE AS (
                SELECT VendorID, ROW_NUMBER() OVER (ORDER BY VendorID) AS RowNum
                FROM Vendor_Info
            )
            SELECT 
                a.AdminID AS Admin,
                t.TravelerID AS Traveler,
                v.VendorID AS Vendor
            FROM AdminCTE a
            FULL OUTER JOIN TravelerCTE t 
                ON a.RowNum = t.RowNum
            FULL OUTER JOIN VendorCTE v 
                ON COALESCE(a.RowNum, t.RowNum) = v.RowNum
            ORDER BY COALESCE(a.RowNum, t.RowNum);
        ";

                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                dataGridView1.DataSource = dataTable;

                FormatAdminTravelerVendorGrid(); // call formatting method
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void FormatAdminTravelerVendorGrid()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 10, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 9);

            if (dataGridView1.Columns.Contains("Admin"))
                dataGridView1.Columns["Admin"].HeaderText = "Admin ID";

            if (dataGridView1.Columns.Contains("Traveler"))
                dataGridView1.Columns["Traveler"].HeaderText = "Traveler ID";

            if (dataGridView1.Columns.Contains("Vendor"))
                dataGridView1.Columns["Vendor"].HeaderText = "Vendor ID";

            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGreen;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }
        private void button15_Click(object sender, EventArgs e)
        {
            string userID = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(userID))
            {
                MessageBox.Show("Please enter an Admin ID to search!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            label1.Text = "";
            label2.Text = "";
            label3.Text = "";
            pictureBox1.Image = null;
            label5.Text = "";
            con.Open();


            SqlCommand cmd = new SqlCommand("SELECT AdminID AS ID, Name,Picture FROM Admin_Info WHERE AdminID = '" + userID + "'", con);
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                label2.Text = reader["Name"].ToString();
                label1.Text = reader["ID"].ToString();
                label3.Text = "Admin";

                if (!(reader["Picture"] is DBNull))
                {
                    byte[] imgData = (byte[])reader["Picture"];
                    using (MemoryStream ms = new MemoryStream(imgData))
                    {
                        pictureBox1.Image = Image.FromStream(ms);
                    }


                }
                reader.Close();
                con.Close();
                MessageBox.Show("User found!!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                label5.Text = "Admin"; // hidden label to remember type for Delete
                return;
            }
            reader.Close();


            SqlCommand cmd1 = new SqlCommand("SELECT VendorID AS ID, Name,Picture,VendorType FROM Vendor_Info WHERE VendorID = '" + userID + "'", con);
            SqlDataReader reader1 = cmd1.ExecuteReader();

            if (reader1.Read())
            {
                label2.Text = reader1["Name"].ToString();
                label1.Text = reader1["ID"].ToString();
                label3.Text = reader1["VendorType"].ToString(); ;

                if (!(reader1["Picture"] is DBNull))
                {
                    byte[] imgData = (byte[])reader1["Picture"];
                    using (MemoryStream ms = new MemoryStream(imgData))
                    {
                        pictureBox1.Image = Image.FromStream(ms);
                    }

                }
                reader1.Close();
                con.Close();
                MessageBox.Show("User found!!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                label5.Text = "Vendor"; // hidden label to remember type for Delete
                return;

            }
            reader1.Close();

            SqlCommand cmd3 = new SqlCommand("SELECT TravelerID AS ID, Name,Picture,Type FROM Traveler_Info WHERE TravelerID = '" + userID + "'", con);
            SqlDataReader reader3 = cmd3.ExecuteReader();

            if (reader3.Read())
            {
                label2.Text = reader3["Name"].ToString();
                label1.Text = reader3["ID"].ToString();
                label3.Text = reader3["Type"].ToString();

                if (!(reader3["Picture"] is DBNull))
                {
                    byte[] imgData = (byte[])reader3["Picture"];
                    using (MemoryStream ms = new MemoryStream(imgData))
                    {
                        pictureBox1.Image = Image.FromStream(ms);
                    }

                }
                reader3.Close();
                con.Close();
                MessageBox.Show("User found!!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                label5.Text = "Traveler"; // hidden label to remember type for Delete
                return;

            }
            reader3.Close();

            MessageBox.Show("No user found with this ID.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            con.Close();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            string userID = textBox1.Text.Trim();
            string userType = label5.Text;

            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(userType))
            {
                MessageBox.Show("Please search a user first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DialogResult confirm = MessageBox.Show("Are you sure you want to delete this user?",
                                          "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;
            con.Open();

            SqlTransaction tran = con.BeginTransaction();

            try
            {
                if (userType == "Admin")
                {

                    SqlCommand cmd1 = new SqlCommand("DELETE FROM Admin_Info WHERE AdminID = '" + userID + "'", con, tran);
                    cmd1.ExecuteNonQuery();


                    SqlCommand cmd2 = new SqlCommand("DELETE FROM Login WHERE UserID = '" + userID + "'", con, tran);
                    cmd2.ExecuteNonQuery();
                }
                else if (userType == "Vendor")
                {

                    SqlCommand cmd1 = new SqlCommand("DELETE FROM Vendor_Info WHERE VendorID = '" + userID + "'", con, tran);
                    cmd1.ExecuteNonQuery();


                    SqlCommand cmd2 = new SqlCommand("DELETE FROM Login WHERE UserID = '" + userID + "'", con, tran);
                    cmd2.ExecuteNonQuery();
                }
                else if (userType == "Traveler")
                {

                    SqlCommand cmd1 = new SqlCommand("DELETE FROM Traveler_Info WHERE TravelerID = '" + userID + "'", con, tran);
                    cmd1.ExecuteNonQuery();

                    // Delete from Login
                    SqlCommand cmd2 = new SqlCommand("DELETE FROM Login WHERE UserID ='" + userID + "'", con, tran);
                    cmd2.ExecuteNonQuery();
                }

                tran.Commit();

                MessageBox.Show("User removed successfully!", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear UI after deletion
                textBox1.Clear();
                label2.Text = "";
                label1.Text = "";
                label3.Text = "";
                pictureBox1.Image = null;
                label5.Text = "";
                con.Close();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                MessageBox.Show("Error: " + ex.Message);
            }

        }

       
    }
}
