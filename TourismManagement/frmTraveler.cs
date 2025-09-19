using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace TourismManagement
{
    public partial class frmTraveler : Form
    {

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;
        private string TravelerID; 
        private string TravelerName;
        public frmTraveler(string travelerId)
        {
            InitializeComponent();
            this.TravelerID = travelerId;
            this.TravelerName = null;
            this.Load += new System.EventHandler(this.frmTraveler_Load);
        }

       

        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");

        private void frmTraveler_Load(object sender, EventArgs e)
        {
            loadForm(new frmFlight(TravelerID));
            button13.Visible = false;
            ShowName(TravelerID);

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

        private void button1_Click(object sender, EventArgs e)
        {
            
            loadForm(new frmFlight(TravelerID));
            

        }
        private void button2_Click(object sender, EventArgs e)
        {
            loadForm(new frmHotel(TravelerID));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            loadForm(new frmVehicle());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            loadForm(new frmPackage());
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.WindowState |= FormWindowState.Minimized;
        }

        private void frmTraveler_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        
        
        public void ShowName(string id)
        {
            bool dataFound = false;
            con.Open();
            SqlCommand cmd = new SqlCommand("Select Name, Picture from Traveler_Info Where TravelerID='" + id + "'", con);

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                TravelerName= reader["Name"].ToString();
                label3.Text = reader["Name"].ToString();

               
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
                dataFound = true;
            }

            reader.Close();
            con.Close();

            button13.Visible = dataFound;

        }

        private void button7_Click(object sender, EventArgs e)
        {
            new frmUser().Show();
            this.Close();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            new frmLogin().Show();
            this.Hide();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            new frmTravelerDashBoard(TravelerID,TravelerName).Show();
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
