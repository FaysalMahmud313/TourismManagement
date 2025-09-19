using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TourismManagement
{
    public partial class frmFlight : Form
    {

        private string tId;
        public frmFlight(string id)
        {
            InitializeComponent();
            this.tId = id;

        }

        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

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

        private void comboBox1_Enter(object sender, EventArgs e)
        {
            if (comboBox1.Text == "DHAKA")
            {
                comboBox1.Text = "";
                comboBox1.ForeColor = Color.Black;
            }
        }

        private void comboBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(comboBox1.Text))
            {
                comboBox1.Text = "DHAKA";
                comboBox1.ForeColor = Color.Gray;
            }
        }

        private void comboBox2_Enter(object sender, EventArgs e)
        {
            if (comboBox2.Text == "COX'S BAZAR")
            {
                comboBox2.Text = "";
                comboBox2.ForeColor = Color.Black;
            }
        }

        private void comboBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(comboBox2.Text))
            {
                comboBox2.Text = "COX'S BAZAR";
                comboBox2.ForeColor = Color.Gray;
            }
        }

        private void frmFlight_Load(object sender, EventArgs e)
        {
            LoadCity(comboBox1,"FromCity");
            LoadCity(comboBox2, "ToCity");
        }

        private void LoadCity(System.Windows.Forms.ComboBox combo, string columnName)
        {
            combo.Items.Clear();
            string query = $"SELECT DISTINCT {columnName} FROM Flight_Info ORDER BY {columnName}";
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
            string fromCity = comboBox1.SelectedItem?.ToString();
            string toCity = comboBox2.SelectedItem?.ToString();
            DateTime flightDate = dateTimePicker1.Value.Date;

            int seatsNeeded = 0;
            int.TryParse(textBox1.Text, out seatsNeeded);

            // Validation
            if (string.IsNullOrWhiteSpace(fromCity) || string.IsNullOrWhiteSpace(toCity))
            {
                MessageBox.Show("Please select both From and To cities.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Stop execution
            }

            if (seatsNeeded <= 0)
            {
                MessageBox.Show("Please enter a valid number of seats.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Stop execution
            }

            string checkDateQuery = @"
SELECT COUNT(*) 
FROM Flight_Info
WHERE FromCity = @FromCity 
  AND ToCity = @ToCity 
  AND CAST(FlightDate AS DATE) = @FlightDate";

            using (SqlCommand checkCmd = new SqlCommand(checkDateQuery, con))
            {
                checkCmd.Parameters.AddWithValue("@FromCity", fromCity);
                checkCmd.Parameters.AddWithValue("@ToCity", toCity);
                checkCmd.Parameters.AddWithValue("@FlightDate", flightDate);

                con.Open();
                int count = (int)checkCmd.ExecuteScalar();
                con.Close();

                if (count == 0)
                {
                    MessageBox.Show("No flight available on this date. Please enter another date.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

           
            frmFlightView view = new frmFlightView(tId);
            view.ReceiveDetail(fromCity, toCity, flightDate, seatsNeeded);

            view.Show();
            

        }

        

    }
}
