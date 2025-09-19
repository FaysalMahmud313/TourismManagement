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
    public partial class frmTravelerDashBoard : Form
    {
        private string travelerID;
        private string travelerName;
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        public frmTravelerDashBoard(string id,string name)
        {
            InitializeComponent();

            this.travelerID = id;
            this.travelerName = name;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            loadForm(new frmUpcomingEvent(travelerID));
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

        private void Unreviwed()
        {
            using (SqlCommand cmd = new SqlCommand(@"
    SELECT COUNT(*) 
    FROM (
        SELECT HB.BookingID
        FROM HotelBooking_Info HB
        WHERE HB.TravelerID = @TravelerID AND HB.CheckOutDate < GETDATE() AND HB.Reviewed = 0 AND HB.BookingStatus= 'Booked'

        UNION ALL

        SELECT FB.BookingID
        FROM Booking_Info FB
        JOIN Flight_Info F ON FB.ServiceID = F.FlightID
        WHERE FB.TravelerID = @TravelerID AND F.FlightDate < GETDATE() AND FB.Reviewed = 0 AND FB.BookingStatus= 'Booked'
    ) AS Unreviewed
", con))
            {
                cmd.Parameters.AddWithValue("@TravelerID", travelerID);
                con.Open();
                int unreviewedCount = (int)cmd.ExecuteScalar();
                con.Close();

                label3.Text = unreviewedCount.ToString(); // your label
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            new frmTraveler(travelerID).Show();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            loadForm(new frmPastEvents(travelerID));
        }

        private void frmTravelerDashBoard_Load(object sender, EventArgs e)
        {
            label2.Text = travelerName.ToString();
            Unreviwed();
            loadForm(new frmUpcomingEvent(travelerID));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            loadForm(new frmTravelerUpdateProfile(travelerID));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            loadForm(new frmReview(travelerID));
        }
    }
}

