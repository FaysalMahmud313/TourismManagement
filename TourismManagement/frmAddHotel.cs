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
    public partial class frmAddHotel : Form
    {
        private string vendorId;
        public frmAddHotel(string id)
        {
            InitializeComponent();
            this.vendorId = id;
        }

        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }
        /*public void ShowName(string id)
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

        }*/

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
        //public string VendorID { get; set; }
        public void ShowNameH(string id)
        {
            vendorId = id;
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
        private string GenerateHotelID()
        {
            Random rnd = new Random();
            int number = rnd.Next(10000, 99999);
            return "H" + number;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            string hotelName = textBox2.Text;
            string country = textBox11.Text;
            string city = textBox12.Text;

            int totalRooms = int.Parse(textBox1.Text);
            int availableRooms = int.Parse(textBox3.Text);

            int singleRooms = int.Parse(textBox4.Text);
            int doubleRooms = int.Parse(textBox6.Text);
            int tripleRooms = int.Parse(textBox7.Text);

            decimal singlePrice = decimal.Parse(textBox9.Text);
            decimal doublePrice = decimal.Parse(textBox5.Text);
            decimal triplePrice = decimal.Parse(textBox10.Text);

            string category = comboBox1.Text;
            string shortDescription = textBox8.Text;

            bool breakfast = checkBox1.Checked;
            bool wifi = checkBox2.Checked;

            
            string hotelID = GenerateHotelID(); 
            string vendorID = vendorId;

            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Please add a hotel image before saving.");
                return; 
            }
            if (string.IsNullOrWhiteSpace(hotelName) || string.IsNullOrWhiteSpace(country) ||string.IsNullOrWhiteSpace(city) ||totalRooms <= 0 ||availableRooms < 0 ||singleRooms < 0 ||
                doubleRooms < 0 ||
                tripleRooms < 0 ||
                singlePrice <= 0 ||
                doublePrice <= 0 ||
                triplePrice <= 0 ||
                string.IsNullOrWhiteSpace(category) ||
                string.IsNullOrWhiteSpace(shortDescription))
            {
                MessageBox.Show("Fill up all the fields properly!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (totalRooms<=0||availableRooms>totalRooms||(singleRooms+doubleRooms+tripleRooms)!=totalRooms)
            {
                MessageBox.Show("Enter the number of rooms fiels Properly!\n Your math is weak!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte[] hotelPic = null;
            if (pictureBox1.Image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    pictureBox1.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    hotelPic = ms.ToArray();
                }
            }

            
            
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

            try
            {

                string queryHotel = @"INSERT INTO Hotel_Info 
                (HotelID, VendorID, HotelName, Country, City, TotalRooms, AvailableRooms, Category, ShortDescription, Breakfast, Wifi, Picture) 
                VALUES (@HotelID, @VendorID, @HotelName, @Country, @City, @TotalRooms, @AvailableRooms, @Category, @ShortDescription, @Breakfast, @Wifi, @Picture)";

                SqlCommand cmdHotel = new SqlCommand(queryHotel, con, tran);
                cmdHotel.Parameters.AddWithValue("@HotelID", hotelID);
                cmdHotel.Parameters.AddWithValue("@VendorID", vendorID);
                cmdHotel.Parameters.AddWithValue("@HotelName", hotelName);
                cmdHotel.Parameters.AddWithValue("@Country", country);
                cmdHotel.Parameters.AddWithValue("@City", city);
                cmdHotel.Parameters.AddWithValue("@TotalRooms", totalRooms);
                cmdHotel.Parameters.AddWithValue("@AvailableRooms", availableRooms);
                cmdHotel.Parameters.AddWithValue("@Category", category);
                cmdHotel.Parameters.AddWithValue("@ShortDescription", shortDescription);
                cmdHotel.Parameters.AddWithValue("@Breakfast", breakfast);
                cmdHotel.Parameters.AddWithValue("@Wifi", wifi);
                cmdHotel.Parameters.AddWithValue("@Picture", hotelPic ?? (object)DBNull.Value);
                cmdHotel.ExecuteNonQuery();


                InsertRooms(con, tran, hotelID, singleRooms, doubleRooms, tripleRooms, singlePrice, doublePrice, triplePrice);


                tran.Commit();
                con.Close();
                MessageBox.Show("Hotel registered successfully!");
            }
            catch (Exception ex)
            {
                tran.Rollback();
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
              pictureBox1.Image = null;
            button1.Visible = true;
            
        }

        
        private void InsertRooms(SqlConnection con, SqlTransaction tran, string hotelID, int singleRooms, int doubleRooms, int tripleRooms,
                           decimal singlePrice, decimal doublePrice, decimal triplePrice)
        {
            
           
            for (int i = 1; i <= singleRooms; i++)
            {
                string roomNumber = "RS" + i;
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Room_Info (HotelID,RoomNumber, RoomType, PricePerNight) " +
                    "VALUES (@HotelID,@RoomNumber, @RoomType, @PricePerNight)", con, tran);
                cmd.Parameters.AddWithValue("@HotelID", hotelID);
                cmd.Parameters.AddWithValue("@RoomType", "Single");
                cmd.Parameters.AddWithValue("@PricePerNight", singlePrice);
                cmd.Parameters.AddWithValue("@RoomNumber", roomNumber);
               
                cmd.ExecuteNonQuery();
                
            }

            for (int i = 1; i <= doubleRooms; i++)
            {

                string roomNumber = "RD" + i;
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Room_Info (HotelID,RoomNumber, RoomType, PricePerNight) " +
                    "VALUES (@HotelID,@RoomNumber, @RoomType, @PricePerNight)", con,tran);
                cmd.Parameters.AddWithValue("@HotelID", hotelID);
                cmd.Parameters.AddWithValue("@RoomType", "Double");
                cmd.Parameters.AddWithValue("@PricePerNight", doublePrice);
                cmd.Parameters.AddWithValue("@RoomNumber", roomNumber);
               
                cmd.ExecuteNonQuery();
               
            }

            // Insert 3-Bed Rooms
            for (int i = 1; i <= tripleRooms; i++)
            {
                string roomNumber = "RT" + i;
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Room_Info (HotelID,RoomNumber, RoomType, PricePerNight) " +
                    "VALUES (@HotelID,@RoomNumber, @RoomType, @PricePerNight)", con,tran);
                cmd.Parameters.AddWithValue("@HotelID", hotelID);
                cmd.Parameters.AddWithValue("@RoomType", "Tripple");
                cmd.Parameters.AddWithValue("@PricePerNight", triplePrice);
                cmd.Parameters.AddWithValue("@RoomNumber", roomNumber);
                
                cmd.ExecuteNonQuery();
                
                
            }
           

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(ofd.FileName);
            }
            button1.Hide();
        }

        private void BindData()
        {

           
            try
            {
                

             
                string query = @"
        SELECT R.HotelID, R.RoomNumber, R.RoomStatus, R.RoomType
        FROM Room_Info R
        INNER JOIN Hotel_Info H ON R.HotelID = H.HotelID
        WHERE H.VendorID = @VendorID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@VendorID", vendorId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    dataGridView1.DataSource = dt;
                }
                else
                {
                    dataGridView1.DataSource = null;
                }

                
            }
            catch (Exception ex)
            {
                
                MessageBox.Show("Error444: " + ex.Message);
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BindData();
        }

        private void frmAddHotel_Load(object sender, EventArgs e)
        {
            BindData();
            ShowNameH(vendorId);
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "Name of Your Hotel")
            {
                textBox2.Text = "";
                textBox2.ForeColor = Color.Black;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                textBox2.Text = "Name of Your Hotel";
                textBox2.ForeColor = Color.Gray;
            }
        }

        private void textBox11_Enter(object sender, EventArgs e)
        {
            if (textBox11.Text == "Country")
            {
                textBox11.Text = "";
                textBox11.ForeColor = Color.Black;
            }
        }

        private void textBox11_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox11.Text))
            {
                textBox11.Text = "Country";
                textBox11.ForeColor = Color.Gray;
            }
        }

        private void textBox12_Enter(object sender, EventArgs e)
        {
            if (textBox12.Text == "City")
            {
                textBox12.Text = "";
                textBox12.ForeColor = Color.Black;
            }
        }

        private void textBox12_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox12.Text))
            {
                textBox12.Text = "City";
                textBox12.ForeColor = Color.Gray;
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Total Number of Tooms")
            {
                textBox1.Text = "";
                textBox1.ForeColor = Color.Black;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "Total Number of Tooms";
                textBox1.ForeColor = Color.Gray;
            }
        }

        private void comboBox1_Enter(object sender, EventArgs e)
        {
            if (comboBox1.Text == "Category")
            {
                comboBox1.Text = "";
                comboBox1.ForeColor = Color.Black;
            }
        }

        private void comboBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(comboBox1.Text))
            {
                comboBox1.Text = "Category";
                comboBox1.ForeColor = Color.Gray;
            }
        }

        private void textBox3_Enter(object sender, EventArgs e)
        {
            if (textBox3.Text == "Available Rooms")
            {
                textBox3.Text = "";
                textBox3.ForeColor = Color.Black;
            }
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                textBox3.Text = "Available Rooms";
                textBox3.ForeColor = Color.Gray;
            }
        }

        private void textBox4_Enter(object sender, EventArgs e)
        {
            if (textBox4.Text == "Number Of Single Rooms")
            {
                textBox4.Text = "";
                textBox4.ForeColor = Color.Black;
            }
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                textBox4.Text = "Number Of Single Rooms";
                textBox4.ForeColor = Color.Gray;
            }
        }

        private void textBox6_Enter(object sender, EventArgs e)
        {
            if (textBox6.Text == "Number Of Double Rooms")
            {
                textBox6.Text = "";
                textBox6.ForeColor = Color.Black;
            }
        }

        private void textBox6_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox6.Text))
            {
                textBox6.Text = "Number Of Double Rooms";
                textBox6.ForeColor = Color.Gray;
            }
        }

        private void textBox7_Enter(object sender, EventArgs e)
        {
            if (textBox7.Text == "Number of 3 Bed Rooms")
            {
                textBox7.Text = "";
                textBox7.ForeColor = Color.Black;
            }
        }

        private void textBox7_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox7.Text))
            {
                textBox7.Text = "Number of 3 Bed Rooms";
                textBox7.ForeColor = Color.Gray;
            }
        }

        private void textBox9_Enter(object sender, EventArgs e)
        {
            if (textBox9.Text == "Rent Of Single Rooms")
            {
                textBox9.Text = "";
                textBox9.ForeColor = Color.Black;
            }
        }

        private void textBox9_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox9.Text))
            {
                textBox9.Text = "Rent Of Single Rooms";
                textBox9.ForeColor = Color.Gray;
            }
        }

        private void textBox5_Enter(object sender, EventArgs e)
        {
            if (textBox5.Text == "Rent Of Double Rooms")
            {
                textBox5.Text = "";
                textBox5.ForeColor = Color.Black;
            }
        }

        private void textBox5_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox5.Text))
            {
                textBox5.Text = "Rent Of Double Rooms";
                textBox5.ForeColor = Color.Gray;
            }
        }

        private void textBox10_Enter(object sender, EventArgs e)
        {
            if (textBox10.Text == "Rent of 3 Bed Rooms")
            {
                textBox10.Text = "";
                textBox10.ForeColor = Color.Black;
            }
        }

        private void textBox10_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox10.Text))
            {
                textBox10.Text = "Rent of 3 Bed Rooms";
                textBox10.ForeColor = Color.Gray;
            }
        }

        private void textBox8_Enter(object sender, EventArgs e)
        {
            if (textBox8.Text == "Description Within 20 Words")
            {
                textBox8.Text = "";
                textBox8.ForeColor = Color.Black;
            }
        }

        private void textBox8_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox8.Text))
            {
                textBox8.Text = "Description Within 20 Words";
                textBox8.ForeColor = Color.Gray;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string hotelID = textBox13.Text.Trim();

            if (string.IsNullOrEmpty(hotelID))
            {
                MessageBox.Show("Please enter a Hotel ID to search!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                con.Open();

                string query = @"SELECT HotelID, HotelName, TotalRooms, AvailableRooms 
                               
                         FROM Hotel_Info WHERE HotelID = @HotelID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@HotelID", hotelID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    //dataGridView1.DataSource = "";
                    dataGridView1.DataSource = dt;
                    label6.Tag = hotelID;
                }
                else
                {
                    MessageBox.Show("No hotel found with this ID.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    label6.Tag = null;
                    BindData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error333: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string hotelID = label6.Tag as string;

            if (string.IsNullOrEmpty(hotelID))
            {
                MessageBox.Show("Please search a hotel first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DialogResult confirm = MessageBox.Show("Are you sure you want to delete this hotel and all its rooms?",
                                 "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            con.Open();
            SqlTransaction tran = con.BeginTransaction();

            try
            {
                // 1. Delete rooms
                SqlCommand cmdRooms = new SqlCommand("DELETE FROM Room_Info WHERE HotelID = @HotelID", con, tran);
                cmdRooms.Parameters.AddWithValue("@HotelID", hotelID);
                cmdRooms.ExecuteNonQuery();

                // 2. Delete hotel
                SqlCommand cmdHotel = new SqlCommand("DELETE FROM Hotel_Info WHERE HotelID = @HotelID", con, tran);
                cmdHotel.Parameters.AddWithValue("@HotelID", hotelID);
                cmdHotel.ExecuteNonQuery();

                tran.Commit();
                MessageBox.Show("Hotel and its rooms deleted successfully!");

                // Clear UI
                dataGridView1.DataSource = null;
                BindData();
                textBox13.Clear();
                
                label6.Tag = null;
            }
            catch (Exception ex)
            {
                tran.Rollback();
                MessageBox.Show("Error111: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                con.Open();

                string query = @"SELECT HotelID, HotelName, TotalRooms, AvailableRooms 
                               
                         FROM Hotel_Info WHERE VendorID = @VendorID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@VendorID", vendorId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    
                    dataGridView1.DataSource = dt;
                    
                }
                else
                {
                    MessageBox.Show("No hotel found with this ID.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    BindData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error222: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            new frmUser().Show();
            this.Close();
        }
    }
    
}
