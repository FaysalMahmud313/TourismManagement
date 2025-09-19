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
    public partial class frmUpcomingEvent : Form
    {

        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");

        private string travelerID;
        public frmUpcomingEvent(string travelerId)
        {
            InitializeComponent();
            this.travelerID = travelerId;
        }

        private void frmUpcomingEvent_Load(object sender, EventArgs e)
        {
            LoadUpcomingEventsByTraveler(travelerID);
        }

        private void LoadUpcomingEventsByTraveler(string travelerId)
        {
            try
            {
                string query = @"
            SELECT 
                t.Name AS TravelerName,
                b.BookingID,
                'Flight' AS EventType,
                f.FromCity,
                f.ToCity,
                f.FlightDate,
                f.Time AS FlightTime,
                b.SeatNumber,
                NULL AS Country,
                NULL AS City,
                NULL AS CheckInDate,
                NULL AS RoomNumbers,
                NULL AS HotelName
            FROM Booking_Info b
            INNER JOIN Traveler_Info t ON b.TravelerID = t.TravelerID
            INNER JOIN Flight_Info f ON b.ServiceID = f.FlightID
            WHERE f.FlightDate > CAST(GETDATE() AS DATE)
              AND b.BookingStatus = 'Booked'
              AND t.TravelerID = @TravelerID

            UNION ALL

           SELECT 
                t.Name AS TravelerName,
                hb.BookingID,
                'Hotel' AS EventType,
                NULL AS FromCity,
                NULL AS ToCity,
                NULL AS FlightDate,
                NULL AS FlightTime,
                NULL AS SeatNumber,
                h.Country,
                h.City,
                hb.CheckInDate,
                hb.RoomNumbers,
                h.HotelName
            FROM HotelBooking_Info hb
            INNER JOIN Traveler_Info t ON hb.TravelerID = t.TravelerID
            INNER JOIN Hotel_Info h ON hb.HotelID = h.HotelID
            WHERE hb.CheckInDate > CAST(GETDATE() AS DATE)
              AND hb.BookingStatus = 'Booked'
              AND t.TravelerID = @TravelerID

            ORDER BY TravelerName, BookingID;";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@TravelerID", travelerId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView1.DataSource = dt;
                    FormatUpcomingEventsGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void FormatUpcomingEventsGrid()
        {
            // Make columns scrollable horizontally
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None; // important
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dataGridView1.ScrollBars = ScrollBars.Both; // enable both vertical & horizontal scrolling

            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 10, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = new Font("Arial Rounded MT Bold", 9);

            // Optional: set a default column width
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.Width = 120; // adjust as needed
            }

            if (dataGridView1.Columns.Contains("TravelerName"))
                dataGridView1.Columns["TravelerName"].HeaderText = "Traveler Name";

            if (dataGridView1.Columns.Contains("BookingID"))
                dataGridView1.Columns["BookingID"].HeaderText = "Booking ID";

            if (dataGridView1.Columns.Contains("EventType"))
                dataGridView1.Columns["EventType"].HeaderText = "Event Type";

            if (dataGridView1.Columns.Contains("FromCity"))
                dataGridView1.Columns["FromCity"].HeaderText = "From City";

            if (dataGridView1.Columns.Contains("ToCity"))
                dataGridView1.Columns["ToCity"].HeaderText = "To City";

            if (dataGridView1.Columns.Contains("FlightDate"))
                dataGridView1.Columns["FlightDate"].HeaderText = "Flight Date";

            if (dataGridView1.Columns.Contains("FlightTime"))
                dataGridView1.Columns["FlightTime"].HeaderText = "Flight Time";

            if (dataGridView1.Columns.Contains("SeatNumber"))
                dataGridView1.Columns["SeatNumber"].HeaderText = "Seat No.";

            if (dataGridView1.Columns.Contains("Country"))
                dataGridView1.Columns["Country"].HeaderText = "Country";

            if (dataGridView1.Columns.Contains("City"))
                dataGridView1.Columns["City"].HeaderText = "City";

            if (dataGridView1.Columns.Contains("CheckInDate"))
                dataGridView1.Columns["CheckInDate"].HeaderText = "Check-In Date";

            if (dataGridView1.Columns.Contains("RoomNumbers"))
                dataGridView1.Columns["RoomNumbers"].HeaderText = "Room Numbers";

            if (dataGridView1.Columns.Contains("HotelName"))
                dataGridView1.Columns["HotelName"].HeaderText = "Hotel Name";

            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGreen;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            dataGridView1.AllowUserToOrderColumns = true; // user can drag columns
            dataGridView1.AllowUserToResizeColumns = true; // user can resize columns
        }
    }
}
