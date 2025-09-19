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
    public partial class frmTravelerUpdateProfile : Form
    {

        string selectedImagePath = "";
        private string travelerID;
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        public frmTravelerUpdateProfile(string id)
        {
            InitializeComponent();
            this.travelerID = id;
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void LoadTravelerDetails()
        {
            
                string query = "SELECT TravelerID, Name, Email, Nid_PassportNumber, PhoneNumber, Gender, Password, Picture FROM Traveler_Info WHERE TravelerID=@TravelerID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@TravelerID", travelerID);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    label3.Text = reader["TravelerID"].ToString();
                    label4.Text = reader["Name"].ToString();
                    label5.Text = reader["Email"].ToString();
                    label6.Text = reader["Nid_PassportNumber"].ToString();
                    label8.Text = reader["Password"].ToString();
                    textBox1.Text = reader["PhoneNumber"].ToString();
                    label7.Text = reader["Gender"].ToString();

                    if (reader["Picture"] != DBNull.Value)
                    {
                        byte[] imgData = (byte[])reader["Picture"];
                        using (MemoryStream ms = new MemoryStream(imgData))
                        {
                            pictureBox1.Image = Image.FromStream(ms);
                        }
                    }
                }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedImagePath = ofd.FileName;

                
                pictureBox1.Image = new Bitmap(selectedImagePath);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            string phone = textBox1.Text?.Trim();

            if (string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Phone number cannot be empty!", "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "UPDATE Traveler_Info SET PhoneNumber=@Phone, Picture=@Picture WHERE TravelerID=@TravelerID";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@TravelerID", travelerID);

            
                if (!string.IsNullOrEmpty(selectedImagePath))
                {
                   
                    byte[] imgData = File.ReadAllBytes(selectedImagePath);
                    cmd.Parameters.AddWithValue("@Picture", imgData);
                }
                else if (pictureBox1.Image != null)
                {
                    
                    cmd.Parameters.AddWithValue("@Picture", GetCurrentImage());
                }
                else
                {
                    
                    cmd.Parameters.AddWithValue("@Picture", DBNull.Value);
                }

                if (con.State == ConnectionState.Closed)
                    con.Open();

                cmd.ExecuteNonQuery();

                if (con.State == ConnectionState.Open)
                    con.Close();

            }


            MessageBox.Show("Profile updated successfully!");

        }

        private byte[] GetCurrentImage()
        {
            if (pictureBox1.Image == null) return null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (Bitmap bmp = new Bitmap(pictureBox1.Image))
                {
                   
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                }
                return ms.ToArray();
            }
        }

        private void frmTravelerUpdateProfile_Load(object sender, EventArgs e)
        {
            LoadTravelerDetails();
        }
    }
}
