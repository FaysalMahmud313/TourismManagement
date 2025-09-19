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
    public partial class frmVendorVehicle : Form
    {
        public frmVendorVehicle()
        {
            InitializeComponent();
        }

        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");

        public void ShowName(string id)
        {
            con.Open();
            SqlCommand cmd = new SqlCommand("Select Name, Picture from Vendor_Info Where VendorID='" + id + "'", con);

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                // Assign Name
                label3.Text = reader["Name"].ToString();

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
    }
}
