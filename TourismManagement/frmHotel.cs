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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TourismManagement
{
    public partial class frmHotel : Form
    {
        private string TravelerID;
        public frmHotel(string id)
        {
            InitializeComponent();
            this.TravelerID = id;
        }
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        private void comboBox1_Enter(object sender, EventArgs e)
        {
            if (comboBox1.Text == "Bangladesh")
            {
                comboBox1.Text = "";
                comboBox1.ForeColor = Color.Black;
            }
        }

        private void comboBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(comboBox1.Text))
            {
                comboBox1.Text = "Bangladesh";
                comboBox1.ForeColor = Color.Gray;
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "00")
            {
                textBox1.Text = "";
                textBox1.ForeColor = Color.Black;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "00";
                textBox1.ForeColor = Color.Gray;
            }
        }

        private void comboBox2_Enter(object sender, EventArgs e)
        {
            if (comboBox2.Text == "DHAKA")
            {
                comboBox2.Text = "";
                comboBox2.ForeColor = Color.Black;
            }
        }

        private void comboBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(comboBox2.Text))
            {
                comboBox2.Text = "DHAKA";
                comboBox2.ForeColor = Color.Gray;
            }
        }

        private void LoadCountry(System.Windows.Forms.ComboBox combo, string columnName)
        {

            combo.Items.Clear();
            string query = $"SELECT DISTINCT {columnName} FROM Hotel_Info ORDER BY {columnName}";
            SqlCommand cmd = new SqlCommand(query, con);
            try
            {
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (!string.IsNullOrWhiteSpace(reader[columnName].ToString()))
                        combo.Items.Add(reader[columnName].ToString());
                }

                reader.Close();
            }
            finally
            {
                con.Close();
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            string country = comboBox1.SelectedItem?.ToString();
            string city = comboBox2.SelectedItem?.ToString();
            string roomType = string.Empty;

            if (radioButton1.Checked)
            {
                roomType = "Single";
            }
            else if (radioButton2.Checked)
            {
                roomType = "Double";
            }
            else if (radioButton3.Checked)
            {
                roomType = "Triple";
            }

            // Now call your method with roomType
            

                int roomsNeeded = 0;
             int.TryParse(textBox1.Text, out roomsNeeded);

            // Get check-in and check-out dates
            DateTime checkInDate = dateTimePicker1.Value.Date;
                DateTime checkOutDate = dateTimePicker2.Value.Date;

                if (string.IsNullOrEmpty(country))
                {
                    MessageBox.Show("Please select a country.", "Missing Field",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(city))
                {
                    MessageBox.Show("Please select a city.", "Missing Field",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(roomType))
                {
                    MessageBox.Show("Please select a room type.", "Missing Field",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (roomsNeeded <= 0)
                {
                    MessageBox.Show("Please enter a valid number of rooms.", "Invalid Input",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (checkOutDate <= checkInDate)
                {
                    MessageBox.Show("Check-out date must be after Check-in date.", "Invalid Dates",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                // Pass all data to frmHotelView
                frmHotelView view = new frmHotelView(country, city, roomsNeeded, checkInDate, checkOutDate, TravelerID,roomType);
                view.Show();
                this.Hide();
            
               
        }

        private void frmHotel_Load(object sender, EventArgs e)
        {
            LoadCountry(comboBox1, "Country");
            LoadCountry(comboBox2, "City");
        }
    }
}
