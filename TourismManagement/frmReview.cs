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
    public partial class frmReview : Form
    {
        private string travelerID;

        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        public frmReview(string id)
        {
            InitializeComponent();
            this.travelerID = id;
        }

        private void frmReview_Load(object sender, EventArgs e)
        {
            LoadUnreviewedTrips(travelerID);
            StyleGrid();
            this.dataGridView1.RowPrePaint += new DataGridViewRowPrePaintEventHandler(this.dataGridView1_RowPrePaint);
        }

        private void LoadUnreviewedTrips(string travelerID)
        {
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT HB.BookingID, 'Hotel' AS ServiceType, H.HotelName AS ServiceName, HB.CheckOutDate AS CheckOut, 
               H.Picture AS ServiceImage, H.VendorID
        FROM HotelBooking_Info HB
        JOIN Hotel_Info H ON HB.HotelID = H.HotelID
        WHERE HB.TravelerID = @TravelerID AND HB.CheckOutDate < GETDATE() AND HB.Reviewed = 0 AND HB.BookingStatus= 'Booked'

        UNION ALL

        SELECT FB.BookingID, 'Flight' AS ServiceType, F.FlightID AS ServiceName, F.FlightDate, 
               F.Picture AS ServiceImage, F.VendorID
        FROM Booking_Info FB
        JOIN Flight_Info F ON FB.ServiceID = F.FlightID
        WHERE FB.TravelerID = @TravelerID AND F.FlightDate < GETDATE() AND FB.Reviewed = 0 AND FB.BookingStatus= 'Booked'", con))
            {
                cmd.Parameters.AddWithValue("@TravelerID", travelerID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Convert byte[] to Image
                dt.Columns.Add("ImageDisplay", typeof(Image));
                foreach (DataRow row in dt.Rows)
                {
                    if (row["ServiceImage"] != DBNull.Value)
                    {
                        byte[] imgBytes = (byte[])row["ServiceImage"];
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        {
                            row["ImageDisplay"] = Image.FromStream(ms);
                        }
                    }
                    else
                    {
                        row["ImageDisplay"] = null;
                    }
                }

                dataGridView1.DataSource = dt;

                // Hide byte[] column
                dataGridView1.Columns["ServiceImage"].Visible = false;

                // Adjust image column
                DataGridViewImageColumn imgCol = (DataGridViewImageColumn)dataGridView1.Columns["ImageDisplay"];
                imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;

                // Add review text column if not exists
                if (!dataGridView1.Columns.Contains("ReviewText"))
                {
                    DataGridViewTextBoxColumn reviewCol = new DataGridViewTextBoxColumn();
                    reviewCol.Name = "ReviewText";
                    reviewCol.HeaderText = "Your Review";
                    reviewCol.Width = 200;
                    dataGridView1.Columns.Add(reviewCol);
                }

                // Add rating dropdown if not exists
                if (!dataGridView1.Columns.Contains("Rating"))
                {
                    DataGridViewComboBoxColumn ratingCol = new DataGridViewComboBoxColumn();
                    ratingCol.Name = "Rating";
                    ratingCol.HeaderText = "Rating (1-5)";
                    ratingCol.Items.AddRange("1", "2", "3", "4", "5");
                    ratingCol.Width = 100;
                    dataGridView1.Columns.Add(ratingCol);
                }
            }
        }

        private void StyleGrid()
        {
            // General Grid Style
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255); // light blue
            dataGridView1.DefaultCellStyle.BackColor = Color.WhiteSmoke;
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.Font = new Font("Segoe UI", 10);

            // Column headers
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215); // blue
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Row selection style
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 153, 255);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;

            // Row height (good for images)
            dataGridView1.RowTemplate.Height = 80;

            // Keep your designer size (don’t auto-fill)
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // Grid lines
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView1.GridColor = Color.LightGray;

            // Make ReviewText column editable with white background
            if (dataGridView1.Columns.Contains("ReviewText"))
            {
                dataGridView1.Columns["ReviewText"].ReadOnly = false;
                dataGridView1.Columns["ReviewText"].DefaultCellStyle.BackColor = Color.White;
                dataGridView1.Columns["ReviewText"].DefaultCellStyle.ForeColor = Color.Black;
            }

            // Make Rating column editable with light yellow background
            if (dataGridView1.Columns.Contains("Rating"))
            {
                dataGridView1.Columns["Rating"].ReadOnly = false;
                dataGridView1.Columns["Rating"].DefaultCellStyle.BackColor = Color.LightYellow;
                dataGridView1.Columns["Rating"].DefaultCellStyle.ForeColor = Color.Black;
            }

            // Make all other columns readonly
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                if (col.Name != "ReviewText" && col.Name != "Rating")
                    col.ReadOnly = true;
            }

            // Image column layout
            if (dataGridView1.Columns.Contains("ImageDisplay"))
            {
                DataGridViewImageColumn imgCol = (DataGridViewImageColumn)dataGridView1.Columns["ImageDisplay"];
                imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                imgCol.ReadOnly = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            con.Open();
            SqlTransaction tran = con.BeginTransaction();

            try
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    var reviewText = row.Cells["ReviewText"].Value?.ToString();
                    var rating = row.Cells["Rating"].Value?.ToString();

                    if (!string.IsNullOrEmpty(reviewText) && !string.IsNullOrEmpty(rating))
                    {
                        string bookingID = row.Cells["BookingID"].Value.ToString();
                        string serviceType = row.Cells["ServiceType"].Value.ToString();

                        // Insert review
                       

                        string insertQuery = @"INSERT INTO Review 
                       (TravelerID, BookingID, ServiceType, Rating, ReviewText, VendorID) 
                       VALUES (@TravelerID, @BookingID, @ServiceType, @Rating, @ReviewText, @VendorID)";
                        using (SqlCommand cmd = new SqlCommand(insertQuery, con, tran))
                        {
                            cmd.Parameters.AddWithValue("@TravelerID", travelerID);
                            cmd.Parameters.AddWithValue("@BookingID", bookingID);
                            cmd.Parameters.AddWithValue("@ServiceType", serviceType);
                            cmd.Parameters.AddWithValue("@Rating", rating);
                            cmd.Parameters.AddWithValue("@ReviewText", reviewText);
                            cmd.Parameters.AddWithValue("@VendorID", row.Cells["VendorID"].Value.ToString());
                            cmd.ExecuteNonQuery();
                        }
                        // Update reviewed flag
                        string updateQuery = serviceType == "Hotel"
                            ? "UPDATE HotelBooking_Info SET Reviewed=1 WHERE BookingID=@BookingID"
                            : "UPDATE Booking_Info SET Reviewed=1 WHERE BookingID=@BookingID";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, con, tran))
                        {
                            cmd.Parameters.AddWithValue("@BookingID", bookingID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                tran.Commit();
                MessageBox.Show("Reviews saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

           
            LoadUnreviewedTrips(travelerID);
        }

        private void dataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].Cells["ServiceType"].Value != null)
            {
                string type = dataGridView1.Rows[e.RowIndex].Cells["ServiceType"].Value.ToString();

                if (type == "Hotel")
                {
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Honeydew;   // light green
                }
                else if (type == "Flight")
                {
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.MistyRose; // light red/orange
                }
            }
        }
    }
    
}
