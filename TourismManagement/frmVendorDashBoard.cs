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

namespace TourismManagement
{
    public partial class frmVendorDashBoard : Form
    {
        private string vendorId;
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        public frmVendorDashBoard(string id)
        {
            InitializeComponent();
            this.vendorId = id;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            loadForm(new frmVendorServiceList(vendorId));
        }

        public void ShowName(string id)
        {


            con.Open();
            SqlCommand cmd = new SqlCommand("Select Name, Picture from Vendor_Info Where VendorID='" + id + "'", con);

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
             
                label4.Text = reader["Name"].ToString();

                
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

        private void button5_Click(object sender, EventArgs e)
        {
            
        }
    }
}
