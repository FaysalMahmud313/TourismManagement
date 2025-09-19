using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TourismManagement
{
    public partial class frmFlightView : Form
    {
        public string searchFrom, searchTo;
        public DateTime flightDate;
       public int numberOfPerson;
        private string TravelerID;
        public frmFlightView(string travelerID)
        {
            InitializeComponent();
            this.TravelerID = travelerID;

            
        }
        public void ReceiveDetail(string fromCity, string toCity, DateTime date, int person)
        {
            searchFrom = fromCity;
            searchTo = toCity;
            flightDate = date;
            numberOfPerson = person;
        }
        bool isNextClick = false;
        private int currentPage = 0;
        private int pageSize = 4;
        SqlConnection con= new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");

        
      
      

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void frmFlightView_Load(object sender, EventArgs e)
        {
            LoadFlightData(searchFrom,searchTo,flightDate,numberOfPerson,currentPage);
        }

        public void LoadFlightData(string from, string to,DateTime fldate,int seat,int page)
        {

            string query = @"
            SELECT FlightID, FromCity, ToCity, FlightDate,Duration,Time,Fare, Picture
            FROM Flight_Info
                           WHERE FromCity = @FromCity 
                           AND ToCity = @ToCity 
                           AND CAST(FlightDate AS DATE) = @FlightDate
                           AND Seat >= @SeatsNeeded
            ORDER BY Fare ASC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Offset", page * pageSize);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@FromCity", from);
            cmd.Parameters.AddWithValue("@ToCity", to);
            cmd.Parameters.AddWithValue("@FlightDate", fldate);
            cmd.Parameters.AddWithValue("@SeatsNeeded", seat);

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            ClearPanels();

            panel1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = false;

            if (!reader.HasRows)
            {
                MessageBox.Show("No flights with enough seat on this route!!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
               
                if (isNextClick)
                {
                    MessageBox.Show("No more flights.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    currentPage--;
                    reader.Close();
                    con.Close();
                    LoadFlightData(searchFrom, searchTo, flightDate, numberOfPerson, currentPage);
                }
                return;
            }

           

            int index = 1;

            while (reader.Read())
            {
                string flightId = reader["FlightID"].ToString();
                string fromCity = reader["FromCity"].ToString();
                string toCity = reader["ToCity"].ToString();
                string date = Convert.ToDateTime(reader["FlightDate"]).ToString("dd-MMM-yyyy");
                string duration = reader["Duration"].ToString();
                string time = reader["Time"].ToString();
                decimal fare = Convert.ToDecimal(reader["Fare"]);
                

                Image img = null;
                if (reader["Picture"] != DBNull.Value)
                {
                    byte[] imgData = (byte[])reader["Picture"];
                    using (MemoryStream ms = new MemoryStream(imgData))
                    {
                        img = Image.FromStream(ms);
                    }
                }

                if (index == 1)
                {
                    panel1.Visible=true;
                    label2.Text = flightId;
                    label3.Text = fromCity; 
                    label4.Text = toCity;
                    label5.Text = date;
                    label7.Text = duration;
                    label6.Text = time;
                    label8.Text = fare.ToString("F2") + " BDT";
                    pictureBox1.Image = img;
                }
                else if (index == 2)
                {
                    panel2.Visible=true;
                    label15.Text = flightId;
                    label14.Text = fromCity;
                    label13.Text = toCity;
                    label12.Text = date;
                    label9.Text = duration;
                    label11.Text = time;
                    label10.Text = fare.ToString("F2") + " BDT";
                    pictureBox4.Image = img;
                }
                else if (index == 3)
                {
                    panel3.Visible=true;
                    label22.Text = flightId;
                    label21.Text = fromCity;
                    label20.Text = toCity;
                    label19.Text = date;
                    label16.Text = duration;
                    label18.Text = time;
                    label17.Text = fare.ToString("F2") + " BDT";
                    pictureBox6.Image = img;
                }
                else if (index == 4)
                {
                    panel4.Visible=true;
                    label29.Text = flightId;
                    label28.Text = fromCity;
                    label27.Text = toCity;
                    label26.Text = date;
                    label23.Text = duration;
                    label25.Text = time;
                    label24.Text = fare.ToString("F2") + " BDT";
                    pictureBox8.Image = img;
                }

                index++;
            }
            con.Close();
        }

        private void ClearPanels()
        {
           
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";
            label5.Text = "";
            label6.Text = "";
            label7.Text = "";
            label8.Text = "";
            pictureBox1.Image = null;

            
            label15.Text = "";
            label14.Text = "";
            label13.Text = "";
            label12.Text = "";
            label9.Text = "";
            label11.Text = "";
            label10.Text = "";
            pictureBox4.Image = null;

            
            label22.Text = "";
            label21.Text = "";
            label20.Text = "";
            label19.Text = "";
            label16.Text = "";
            label18.Text = "";
            label17.Text = "";
            pictureBox6.Image = null;

            
            label29.Text = "";
            label28.Text = "";
            label27.Text = "";
            label26.Text = "";
            label23.Text = "";
            label25.Text = "";
            label24.Text = "";
            pictureBox8.Image = null;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            currentPage++;
            isNextClick = true;
            LoadFlightData(searchFrom, searchTo, flightDate, numberOfPerson, currentPage);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                isNextClick = false;
                LoadFlightData(searchFrom, searchTo, flightDate, numberOfPerson, currentPage);
            }
            else
            {
                MessageBox.Show("Already at the first page.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
       

       
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (TravelerID != null)
            {
                string flightID = label2.Text; // Replace with the label containing the selected Flight ID

                DialogResult result = MessageBox.Show(
                    "Do you want to book this flight?",
                    "Confirm Booking",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    ServiceBooking sb = new ServiceBooking();
                    sb.BookingFlight(flightID, numberOfPerson, TravelerID, flightDate);

                }
                else
                {
                    MessageBox.Show("Booking cancelled.", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                ShowLoginPrompt();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (TravelerID != null)
            {
                string flightID = label15.Text; // Replace with the label containing the selected Flight ID

                DialogResult result = MessageBox.Show(
                    "Do you want to book this flight?",
                    "Confirm Booking",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    ServiceBooking sb = new ServiceBooking();
                    sb.BookingFlight(flightID, numberOfPerson, TravelerID, flightDate);

                }
                else
                {
                    MessageBox.Show("Booking cancelled.", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                ShowLoginPrompt();
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (TravelerID != null)
            {
                string flightID = label22.Text; // Replace with the label containing the selected Flight ID

                DialogResult result = MessageBox.Show(
                    "Do you want to book this flight?",
                    "Confirm Booking",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    ServiceBooking sb = new ServiceBooking();
                    sb.BookingFlight(flightID, numberOfPerson, TravelerID, flightDate);

                }
                else
                {
                    MessageBox.Show("Booking cancelled.", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                ShowLoginPrompt();
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (TravelerID != null)
            {
                string flightID = label29.Text; // Replace with the label containing the selected Flight ID

                DialogResult result = MessageBox.Show(
                    "Do you want to book this flight?",
                    "Confirm Booking",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    ServiceBooking sb = new ServiceBooking();
                    sb.BookingFlight(flightID, numberOfPerson, TravelerID, flightDate);
;
                }
                else
                {
                    MessageBox.Show("Booking cancelled.", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                ShowLoginPrompt();
            }
        }

        private void ShowLoginPrompt()
        {
            DialogResult result = MessageBox.Show(
                "To book any service you must login. Do you want to log in?",
                "Login Required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                frmLogin login = new frmLogin();
                login.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("You must be logged in to continue booking.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
