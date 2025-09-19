using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TourismManagement
{
    public partial class frmAdmin : Form
    {
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");

        private string adminID;
        public frmAdmin(string id)
        {
            InitializeComponent();

            this.adminID = id;
        }

        public void ShowName(string id)
        {
            
            
                con.Open();
                SqlCommand cmd = new SqlCommand("Select Name, Picture from Admin_Info Where AdminID='"+id+"'",con);
                
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    // Assign Name
                    label4.Text = reader["Name"].ToString();

                    // Assign Picture (if not null)
                    if (!(reader["Picture"] is DBNull))
                    {
                        byte[] pictureData = (byte[])reader["Picture"];
                        if (pictureData.Length > 0)
                        {
                            using (MemoryStream ms = new MemoryStream(pictureData))
                            {
                                pictureBox2.Image = Image.FromStream(ms);
                            }
                        }
                    }
                }

                reader.Close();
                con.Close();
          
           
        }
       
        private void button12_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            loadForm(new frmUserList());
        }

        private void button13_Click(object sender, EventArgs e)
        {
            
            loadForm(new frmAddAdmin(adminID));
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            new frmUser().Show();
            this.Close();
        }



        private void button4_Click(object sender, EventArgs e)
        {

            new frmTravelerRegister(1,adminID).Show();
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            loadForm(new frmTravelerPaymentAdmin());
        }

        private void UpdateInvalidVendorCount()
        {
            int invalidCount = 0;

            string query = "SELECT COUNT(*) FROM Vendor_Info WHERE Status = 'Invalid'";

            
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    invalidCount = (int)cmd.ExecuteScalar();
                    con.Close();
               }

            label1.Text = invalidCount.ToString();
        }

        private void frmAdmin_Load(object sender, EventArgs e)
        {
            UpdateInvalidVendorCount();
            loadForm(new frmUserList());

            ShowName(adminID);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            
            loadForm(new frmVendorValidation());

        }

        private void button15_Click(object sender, EventArgs e)
        {
            
            loadForm(new frmCompanyEarning());
        }

        private void button3_Click(object sender, EventArgs e)
        {
           
            loadForm(new frmVendorWallet());
        }

        private void button14_Click(object sender, EventArgs e)
        {
            loadForm(new frmVendorRting());
        }

        public void loadForm(Form form)
        {

            if (this.panel2.Controls.Count > 0)
                this.panel2.Controls.Clear();

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;

            this.panel2.Controls.Add(form);
            this.panel2.Tag = form;
            form.Show();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            new frmAboutUs().Show();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            new frmHelp().Show();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            new frmServices().Show();
        }
    }
}
