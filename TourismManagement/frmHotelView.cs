using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TourismManagement
{
    public partial class frmHotelView : Form
    {
        string countryName, cityName,TravelerID,RoomType;
        DateTime checkIn, checkOut;
        int personN;
        public frmHotelView(string country,string city,int person,DateTime checkIN,DateTime checkOUT,string id,string roomType)
        {
            InitializeComponent();
            countryName = country;
            cityName = city;
            personN = person;
            checkIn = checkIN;
            checkOut = checkOUT;
            TravelerID = id;
            RoomType = roomType;
        }
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        private void frmHotelView_Load(object sender, EventArgs e)
        {
            LoadHotelData(countryName, cityName, personN, currentPage, RoomType, checkBox1.Checked, checkBox2.Checked);
        }

        bool isNextClick = false;
        private int currentPage = 0;
        private int pageSize = 8;
        public void LoadHotelData(string country,string city, int roomsNeeded, int page,string roomType,bool breakfast,bool wifi)
        {
            string query = @"
SELECT h.HotelID, h.HotelName, h.AvailableRooms, h.Category, h.Picture, 
       r.PricePerNight
FROM Hotel_Info h
CROSS APPLY (
    SELECT TOP 1 PricePerNight
    FROM Room_Info
    WHERE HotelID = h.HotelID AND RoomType = @RoomType
) r
WHERE h.City = @City
  AND h.Country = @Country
  AND h.AvailableRooms >= @RoomsNeeded
";

            // Add conditions dynamically
            if (breakfast)
                query += " AND h.Breakfast = 1";
            if (wifi)
                query += " AND h.Wifi = 1";

            query += @"
ORDER BY r.PricePerNight ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Offset", page * pageSize);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Country", country);
            cmd.Parameters.AddWithValue("@City", city);
            cmd.Parameters.AddWithValue("@RoomType", roomType);
            cmd.Parameters.AddWithValue("@RoomsNeeded", roomsNeeded);

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            ClearHotelPanels();

            panel1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = false;
            panel5.Visible = false;
            panel6.Visible = false;
            panel7.Visible = false;
            panel8.Visible = false;

           

            int index = 1;
            while (reader.Read())
            {
                string hotelId = reader["HotelID"].ToString();
                string hotelName = reader["HotelName"].ToString();
                string category = reader["Category"].ToString();
                decimal price = Convert.ToDecimal(reader["PricePerNight"]);

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
                    panel1.Visible = true;
                    label26.Text = hotelId;
                    label2.Text = hotelName;
                    label4.Text = category;
                    label3.Text = price.ToString("F2") + "TK";
                    pictureBox1.Image = img;
                }
                else if (index == 2)
                {
                    panel2.Visible = true;
                    label27.Text = hotelId;
                    label7.Text = hotelName;
                    label5.Text = category;
                    label6.Text = price.ToString("F2") + "TK";
                    pictureBox2.Image = img;
                }
                else if (index == 3)
                {
                    panel3.Visible = true;
                    label28.Text = hotelId;
                    label10.Text = hotelName;
                    label8.Text = category;
                    label9.Text = price.ToString("F2") + "TK";
                    pictureBox3.Image = img;
                }
                else if (index == 4)
                {
                    panel4.Visible = true;
                    label29.Text = hotelId;
                    label13.Text = hotelName;
                    label11.Text = category;
                    label12.Text = price.ToString("F2") + "TK";
                    pictureBox4.Image = img;
                }
                else if (index == 5)
                {
                    panel5.Visible = true;
                    label30.Text = hotelId;
                    label16.Text = hotelName;
                    label4.Text = category;
                    label15.Text = price.ToString("F2") + "TK";
                    pictureBox5.Image = img;
                }
                else if (index == 6)
                {
                    panel6.Visible = true;
                    label31.Text = hotelId;
                    label19.Text = hotelName;
                    label17.Text = category;
                    label18.Text = price.ToString("F2") + "TK";
                    pictureBox6.Image = img;
                }
                else if (index == 7)
                {
                    panel7.Visible = true;
                    label32.Text = hotelId;
                    label22.Text = hotelName;
                    label20.Text = category;
                    label21.Text = price.ToString("F2") + "TK";
                    pictureBox7.Image = img;
                }
                else if (index == 8)
                {
                    panel8.Visible = true;
                    label33.Text = hotelId;
                    label25.Text = hotelName;
                    label23.Text = category;
                    label24.Text = price.ToString("F2") + "TK";
                    pictureBox8.Image = img;
                }

                index++;
            }
            if (!reader.HasRows)
            {
                MessageBox.Show("No hotels with enough rooms in this city!!",
        "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (isNextClick)
                {
                    MessageBox.Show("No more hotels.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    currentPage--;
                }

                reader.Close();
                con.Close();

                // ✅ Return to Traveler.cs
                frmTraveler travelerForm = new frmTraveler(TravelerID);
                travelerForm.Show();
                this.Close();  // close current form (HotelView)
                return;
            }

            con.Close();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            currentPage++;
            isNextClick = true;
            LoadHotelData(countryName, cityName, personN, currentPage, RoomType, checkBox1.Checked, checkBox2.Checked);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                isNextClick = false;
                LoadHotelData(countryName, cityName, personN, currentPage, RoomType, checkBox1.Checked, checkBox2.Checked);
            }
            else
            {
                MessageBox.Show("Already at the first page.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            

            bool breakfast = checkBox1.Checked;
            bool wifi = checkBox2.Checked;

            
            LoadHotelData(countryName, cityName, personN, currentPage, RoomType, breakfast, wifi);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (TravelerID != null)
            {
                string hotelID = label27.Text; // Label containing selected Hotel ID
                int capacityPerRoom = 0;

                // ✅ Assign capacity based on room type
                switch (RoomType.ToLower())
                {
                    case "single":
                        capacityPerRoom = 2;
                        break;
                    case "double":
                        capacityPerRoom = 4;
                        break;
                    case "triple":
                        capacityPerRoom = 6;
                        break;
                    default:
                        capacityPerRoom = 1; // fallback
                        break;
                }

                // ✅ Calculate required rooms
                int requiredRooms = (int)Math.Ceiling((double)personN / capacityPerRoom);

                // ✅ Show confirmation
                DialogResult result = MessageBox.Show(
                    $"For {personN} person(s), the system will book {requiredRooms} {RoomType} room(s).\n\n" +
                    "Do you want to continue?",
                    "Confirm Booking",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    ServiceBooking sb = new ServiceBooking();
                    sb.BookingHotel(hotelID, TravelerID, checkIn, checkOut, requiredRooms, RoomType);

                    MessageBox.Show("Hotel booked successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                string hotelID = label28.Text; // Label containing selected Hotel ID
                int capacityPerRoom = 0;

                // ✅ Assign capacity based on room type
                switch (RoomType.ToLower())
                {
                    case "single":
                        capacityPerRoom = 2;
                        break;
                    case "double":
                        capacityPerRoom = 4;
                        break;
                    case "triple":
                        capacityPerRoom = 6;
                        break;
                    default:
                        capacityPerRoom = 1; // fallback
                        break;
                }

                // ✅ Calculate required rooms
                int requiredRooms = (int)Math.Ceiling((double)personN / capacityPerRoom);

                // ✅ Show confirmation
                DialogResult result = MessageBox.Show(
                    $"For {personN} person(s), the system will book {requiredRooms} {RoomType} room(s).\n\n" +
                    "Do you want to continue?",
                    "Confirm Booking",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    ServiceBooking sb = new ServiceBooking();
                    sb.BookingHotel(hotelID, TravelerID, checkIn, checkOut, requiredRooms, RoomType);

                    MessageBox.Show("Hotel booked successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                string hotelID = label29.Text; // Label containing selected Hotel ID
                int capacityPerRoom = 0;

                // ✅ Assign capacity based on room type
                switch (RoomType.ToLower())
                {
                    case "single":
                        capacityPerRoom = 2;
                        break;
                    case "double":
                        capacityPerRoom = 4;
                        break;
                    case "triple":
                        capacityPerRoom = 6;
                        break;
                    default:
                        capacityPerRoom = 1; // fallback
                        break;
                }

                // ✅ Calculate required rooms
                int requiredRooms = (int)Math.Ceiling((double)personN / capacityPerRoom);

                // ✅ Show confirmation
                DialogResult result = MessageBox.Show(
                    $"For {personN} person(s), the system will book {requiredRooms} {RoomType} room(s).\n\n" +
                    "Do you want to continue?",
                    "Confirm Booking",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    ServiceBooking sb = new ServiceBooking();
                    sb.BookingHotel(hotelID, TravelerID, checkIn, checkOut, requiredRooms, RoomType);

                    MessageBox.Show("Hotel booked successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void button8_Click(object sender, EventArgs e)
        {
            if (TravelerID != null)
            {
                string hotelID = label30.Text; // Label containing selected Hotel ID
                int capacityPerRoom = 0;

                // ✅ Assign capacity based on room type
                switch (RoomType.ToLower())
                {
                    case "single":
                        capacityPerRoom = 2;
                        break;
                    case "double":
                        capacityPerRoom = 4;
                        break;
                    case "triple":
                        capacityPerRoom = 6;
                        break;
                    default:
                        capacityPerRoom = 1; // fallback
                        break;
                }

                // ✅ Calculate required rooms
                int requiredRooms = (int)Math.Ceiling((double)personN / capacityPerRoom);

                // ✅ Show confirmation
                DialogResult result = MessageBox.Show(
                    $"For {personN} person(s), the system will book {requiredRooms} {RoomType} room(s).\n\n" +
                    "Do you want to continue?",
                    "Confirm Booking",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    ServiceBooking sb = new ServiceBooking();
                    sb.BookingHotel(hotelID, TravelerID, checkIn, checkOut, requiredRooms, RoomType);

                    MessageBox.Show("Hotel booked successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void button7_Click(object sender, EventArgs e)
        {
            if (TravelerID != null)
            {
                string hotelID = label31.Text; // Label containing selected Hotel ID
                int capacityPerRoom = 0;

                // ✅ Assign capacity based on room type
                switch (RoomType.ToLower())
                {
                    case "single":
                        capacityPerRoom = 2;
                        break;
                    case "double":
                        capacityPerRoom = 4;
                        break;
                    case "triple":
                        capacityPerRoom = 6;
                        break;
                    default:
                        capacityPerRoom = 1; // fallback
                        break;
                }

                // ✅ Calculate required rooms
                int requiredRooms = (int)Math.Ceiling((double)personN / capacityPerRoom);

                // ✅ Show confirmation
                DialogResult result = MessageBox.Show(
                    $"For {personN} person(s), the system will book {requiredRooms} {RoomType} room(s).\n\n" +
                    "Do you want to continue?",
                    "Confirm Booking",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    ServiceBooking sb = new ServiceBooking();
                    sb.BookingHotel(hotelID, TravelerID, checkIn, checkOut, requiredRooms, RoomType);

                    MessageBox.Show("Hotel booked successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void button6_Click(object sender, EventArgs e)
        {
            if (TravelerID != null)
            {
                string hotelID = label32.Text; // Label containing selected Hotel ID
                int capacityPerRoom = 0;

                // ✅ Assign capacity based on room type
                switch (RoomType.ToLower())
                {
                    case "single":
                        capacityPerRoom = 2;
                        break;
                    case "double":
                        capacityPerRoom = 4;
                        break;
                    case "triple":
                        capacityPerRoom = 6;
                        break;
                    default:
                        capacityPerRoom = 1; // fallback
                        break;
                }

                // ✅ Calculate required rooms
                int requiredRooms = (int)Math.Ceiling((double)personN / capacityPerRoom);

                // ✅ Show confirmation
                DialogResult result = MessageBox.Show(
                    $"For {personN} person(s), the system will book {requiredRooms} {RoomType} room(s).\n\n" +
                    "Do you want to continue?",
                    "Confirm Booking",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    ServiceBooking sb = new ServiceBooking();
                    sb.BookingHotel(hotelID, TravelerID, checkIn, checkOut, requiredRooms, RoomType);

                    MessageBox.Show("Hotel booked successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void button5_Click(object sender, EventArgs e)
        {
            if (TravelerID != null)
            {
                string hotelID = label33.Text; // Label containing selected Hotel ID
                int capacityPerRoom = 0;

                // ✅ Assign capacity based on room type
                switch (RoomType.ToLower())
                {
                    case "single":
                        capacityPerRoom = 2;
                        break;
                    case "double":
                        capacityPerRoom = 4;
                        break;
                    case "triple":
                        capacityPerRoom = 6;
                        break;
                    default:
                        capacityPerRoom = 1; // fallback
                        break;
                }

                // ✅ Calculate required rooms
                int requiredRooms = (int)Math.Ceiling((double)personN / capacityPerRoom);

                // ✅ Show confirmation
                DialogResult result = MessageBox.Show(
                    $"For {personN} person(s), the system will book {requiredRooms} {RoomType} room(s).\n\n" +
                    "Do you want to continue?",
                    "Confirm Booking",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    ServiceBooking sb = new ServiceBooking();
                    sb.BookingHotel(hotelID, TravelerID, checkIn, checkOut, requiredRooms, RoomType);

                    MessageBox.Show("Hotel booked successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            checkBox1.CheckedChanged += FilterChanged;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            checkBox2.CheckedChanged += FilterChanged;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (TravelerID != null)
            {
                string hotelID = label26.Text; // Label containing selected Hotel ID
                int capacityPerRoom = 0;

                // ✅ Assign capacity based on room type
                switch (RoomType.ToLower())
                {
                    case "single":
                        capacityPerRoom = 2;
                        break;
                    case "double":
                        capacityPerRoom = 4;
                        break;
                    case "triple":
                        capacityPerRoom = 6;
                        break;
                    default:
                        capacityPerRoom = 1; // fallback
                        break;
                }

                // ✅ Calculate required rooms
                int requiredRooms = (int)Math.Ceiling((double)personN / capacityPerRoom);

                // ✅ Show confirmation
                DialogResult result = MessageBox.Show(
                    $"For {personN} person(s), the system will book {requiredRooms} {RoomType} room(s).\n\n" +
                    "Do you want to continue?",
                    "Confirm Booking",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    ServiceBooking sb = new ServiceBooking();
                    sb.BookingHotel(hotelID, TravelerID, checkIn, checkOut, requiredRooms, RoomType);

                    MessageBox.Show("Hotel booked successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void ClearHotelPanels()
        {
            label2.Text = ""; label10.Text = ""; label18.Text = ""; label26.Text = "";
            label3.Text = ""; label11.Text = ""; label19.Text = ""; label27.Text = "";
            label4.Text = ""; label12.Text = ""; label20.Text = ""; label28.Text = "";
            label5.Text = ""; label13.Text = ""; label21.Text = ""; label29.Text = "";
            label6.Text = ""; label14.Text = ""; label22.Text = ""; label30.Text = "";
            label7.Text = ""; label15.Text = ""; label23.Text = ""; label31.Text = "";
            label8.Text = ""; label16.Text = ""; label24.Text = ""; label32.Text = "";
            label9.Text = ""; label17.Text = ""; label25.Text = ""; label33.Text = "";
            pictureBox1.Image = null; pictureBox2.Image = null; pictureBox3.Image = null;
            pictureBox4.Image = null; pictureBox5.Image = null; pictureBox6.Image = null;
            pictureBox7.Image = null; pictureBox8.Image = null;

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
    }
}
