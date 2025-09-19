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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TourismManagement
{
    public partial class frmVendorFlight : Form
    {
        private string vendorId;
        public frmVendorFlight(string id)
        {
            InitializeComponent();

            this.vendorId = id;
            
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

        
        

        private string GenerateFlightID()
        {
            Random rnd = new Random();
            int number = rnd.Next(1000, 9999);
            return "FL" + number;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

           
            if (ofd.ShowDialog() == DialogResult.OK) // user selected a file
            {
                pictureBox1.Image = Image.FromFile(ofd.FileName);
                button1.Hide(); // ✅ hide only when a file is chosen
            }
            else
            {
                button1.Show(); // ✅ keep visible if canceled
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string fromCity = textBox1.Text;
            string toCity = textBox2.Text;
            DateTime flightDate = dateTimePicker1.Value;
            string duration = textBox3.Text;
            string time = textBox4.Text;
            
            string reportingtime= textBox6.Text;
            
            string flightID = GenerateFlightID();
            string vendorID = vendorId;

            if (!int.TryParse(textBox5.Text, out int seat) || seat <= 0)
            {
                MessageBox.Show("Please enter a valid number of seats!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            
            if (!decimal.TryParse(textBox7.Text, out decimal fare) || fare <= 0)
            {
                MessageBox.Show("Please enter a valid fare!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(pictureBox1.Image == null)
    {
                MessageBox.Show("Please add a flight image before saving.");
                return; // exit the method
            }

            byte[] imgData = null;
            if (pictureBox1.Image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    pictureBox1.Image.Save(ms, pictureBox1.Image.RawFormat); 
                    imgData = ms.ToArray();
                }
                //button1.Hide();
            }

            if (string.IsNullOrWhiteSpace(fromCity) ||string.IsNullOrWhiteSpace(toCity) ||string.IsNullOrWhiteSpace(duration))
            {
                MessageBox.Show("Fill up all the fields properly!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string query = @"INSERT INTO Flight_Info 
(FlightID, VendorID, FromCity, ToCity, FlightDate, Duration, Time, Seat, ReportingTime, Fare, Picture)
VALUES
(@FlightID, @VendorID, @FromCity, @ToCity, @FlightDate, @Duration, @Time, @Seat, @ReportingTime, @Fare, @Picture)";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@FlightID", flightID);
            cmd.Parameters.AddWithValue("@VendorID", vendorID);
            cmd.Parameters.AddWithValue("@FromCity", fromCity);
            cmd.Parameters.AddWithValue("@ToCity", toCity);
            cmd.Parameters.AddWithValue("@FlightDate", flightDate);
            cmd.Parameters.AddWithValue("@Duration", duration);
            cmd.Parameters.AddWithValue("@Time", time);
            cmd.Parameters.AddWithValue("@Seat", seat);
            cmd.Parameters.AddWithValue("@ReportingTime", reportingtime);
            cmd.Parameters.AddWithValue("@Fare", fare);
            if (imgData != null)
            {
                cmd.Parameters.AddWithValue("@Picture", imgData);

            }

            else
                cmd.Parameters.AddWithValue("@Picture", DBNull.Value);

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            GenerateSeats(flightID, seat);

            MessageBox.Show("Flight schedule added successfully!\nFlight ID: " + flightID);
            imgData= null;
            //pictureBox1 = null; 
            
            BindData();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string flightId = textBox8.Text.Trim();
            string vendorID = vendorId;

            string query = @"SELECT FlightID, FromCity, ToCity, FlightDate
                     FROM Flight_Info
                     WHERE FlightID = '"+flightId+"'AND VendorID='"+vendorID+"'";

            SqlCommand cmd = new SqlCommand(query, con);
            

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                dataGridView1.DataSource = dt;
            }
            else
            {
                MessageBox.Show("No flights found for the given criteria.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dataGridView1.DataSource = null;
            }
        }

        private void BindDataSeat()
        {
            string vendorID = vendorId;

            // ✅ Query to show seat details (join with Flight_Info to filter by vendor if needed)
            string query = @"
        SELECT S.SeatNumber, S.FlightID, S.BookedBy, S.IsBooked, S.BookingDate
        FROM Seat_Info S
        INNER JOIN Flight_Info F ON S.FlightID = F.FlightID
        WHERE F.VendorID = @VendorID";

            SqlCommand cmd1 = new SqlCommand(query, con);
            cmd1.Parameters.AddWithValue("@VendorID", vendorID);

            SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
            DataTable dt1 = new DataTable();
            da1.Fill(dt1);

            if (dt1.Rows.Count > 0)
            {
                dataGridView1.DataSource = dt1;
            }
            else
            {
                dataGridView1.DataSource = null;
            }

            // ✅ Make table adjustable
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AllowUserToResizeColumns = true;
            dataGridView1.AllowUserToResizeRows = true;

           
            dataGridView1.BackgroundColor = Color.Honeydew;
            dataGridView1.DefaultCellStyle.BackColor = Color.Honeydew;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.MintCream;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.LightGreen;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        private void BindData()
        {
            string vendorID = vendorId;
            string query = @"SELECT FlightID, FromCity, ToCity, FlightDate
                     FROM Flight_Info
                     WHERE VendorID = @VendorID";

            SqlCommand cmd1 = new SqlCommand(query, con);
            cmd1.Parameters.AddWithValue("@VendorID", vendorID);

            SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
            DataTable dt1 = new DataTable();
            da1.Fill(dt1);

            if (dt1.Rows.Count > 0)
            {
                dataGridView1.DataSource = dt1;
            }
            else
            {
                dataGridView1.DataSource = null;
            }

            // ✅ Make table adjustable
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AllowUserToResizeColumns = true;
            dataGridView1.AllowUserToResizeRows = true;

            // ✅ Style: very light green background
            dataGridView1.BackgroundColor = Color.Honeydew; // very light green
            dataGridView1.DefaultCellStyle.BackColor = Color.Honeydew;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.MintCream; // subtle alternating
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.LightGreen; // when selected
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Optional: center align text
            dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a flight to delete.", "No Flight Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get FlightID of the selected row
            string flightID = dataGridView1.SelectedRows[0].Cells["FlightID"].Value.ToString();

            DialogResult dr = MessageBox.Show("Are you sure you want to delete Flight ID: " + flightID + "?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.No) return;

            try
            {
                SqlCommand cmdSeats = new SqlCommand("DELETE FROM Seat_Info WHERE FlightID = @FlightID", con);
                cmdSeats.Parameters.AddWithValue("@FlightID", flightID);

                SqlCommand cmdBooking = new SqlCommand("DELETE FROM Booking_Info WHERE FlightID = @FlightID", con);
                cmdBooking.Parameters.AddWithValue("@FlightID", flightID);

                SqlCommand cmdFlight = new SqlCommand("DELETE FROM Flight_Info WHERE FlightID = @FlightID", con);
                cmdFlight.Parameters.AddWithValue("@FlightID", flightID);

                

                con.Open();
                cmdSeats.ExecuteNonQuery();
                cmdBooking.ExecuteNonQuery();
                cmdFlight.ExecuteNonQuery();
                con.Close();

                

                MessageBox.Show("Flight deleted successfully!", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                
                dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
                BindData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting flight: " + ex.Message);
            }
        }

        private void GenerateSeats(string flightID, int totalSeats)
        {
            try
            {
                con.Open();

                for (int i = 1; i <= totalSeats; i++)
                {
                    
                    string seatNumber = "S" + i; 
                    SqlCommand cmd = new SqlCommand("INSERT INTO Seat_Info (FlightID, SeatNumber, IsBooked) VALUES (@FlightID, @SeatNumber, 0)", con);
                    cmd.Parameters.AddWithValue("@FlightID", flightID);
                    cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    cmd.ExecuteNonQuery();
                }

                con.Close();
                MessageBox.Show("Seats generated successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating seats: " + ex.Message);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "From")
            {
                textBox1.Text = "";
                textBox1.ForeColor = Color.Black;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "From";
                textBox1.ForeColor = Color.Gray;
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "To")
            {
                textBox2.Text = "";
                textBox2.ForeColor = Color.Black;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                textBox2.Text = "To";
                textBox2.ForeColor = Color.Gray;
            }
        }

        private void textBox3_Enter(object sender, EventArgs e)
        {
            if (textBox3.Text == "Duration")
            {
                textBox3.Text = "";
                textBox3.ForeColor = Color.Black;
            }
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                textBox3.Text = "Duration";
                textBox3.ForeColor = Color.Gray;
            }
        }

        private void textBox4_Enter(object sender, EventArgs e)
        {
            if (textBox4.Text == "Flight Time")
            {
                textBox4.Text = "";
                textBox4.ForeColor = Color.Black;
            }
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                textBox4.Text = "Flight Time";
                textBox4.ForeColor = Color.Gray;
            }
        }

        private void textBox5_Enter(object sender, EventArgs e)
        {
            if (textBox5.Text == "Number of Seats")
            {
                textBox5.Text = "";
                textBox5.ForeColor = Color.Black;
            }
        }

        private void textBox5_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox5.Text))
            {
                textBox5.Text = "Number of Seats";
                textBox5.ForeColor = Color.Gray;
            }
        }

        private void textBox6_Enter(object sender, EventArgs e)
        {
            if (textBox6.Text == "Reporting Time")
            {
                textBox6.Text = "";
                textBox6.ForeColor = Color.Black;
            }
        }

        private void textBox6_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox6.Text))
            {
                textBox6.Text = "Reporting Time";
                textBox6.ForeColor = Color.Gray;
            }
        }

        private void textBox7_Enter(object sender, EventArgs e)
        {
            if (textBox7.Text == "Fear/Seat")
            {
                textBox7.Text = "";
                textBox7.ForeColor = Color.Black;
            }
        }

        private void textBox7_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox7.Text))
            {
                textBox7.Text = "Fear/Seat";
                textBox7.ForeColor = Color.Gray;
            }
        }

        private void textBox8_Enter(object sender, EventArgs e)
        {
            if (textBox8.Text == "Enter Flight ID to Search")
            {
                textBox8.Text = "";
                textBox8.ForeColor = Color.Black;
            }
        }

        private void textBox8_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox8.Text))
            {
                textBox8.Text = "Enter Flight ID to Search";
                textBox8.ForeColor = Color.Gray;
            }
        }

        private void frmVendorFlight_Load(object sender, EventArgs e)
        {
            BindData();

           
        }

        private void button8_Click(object sender, EventArgs e)
        {
            new frmUser().Show();
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            BindDataSeat();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            BindData();
        }
    }
}
