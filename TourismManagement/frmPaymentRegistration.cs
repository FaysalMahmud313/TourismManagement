using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TourismManagement
{
    public partial class frmPaymentRegistration : Form
    {
        private string TravelerID;
        private decimal Amount;
        private string bookingID;
        public frmPaymentRegistration(string travelerID, string bookingId, decimal amount)
        {
            InitializeComponent();
            
            this.TravelerID =travelerID ;
            this.Amount = amount ;
            this.bookingID = bookingId;
            

        }
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        private void button1_Click(object sender, EventArgs e)
        {
            string accountNumber = textBox1.Text.Trim();
            string bankName = textBox2.Text.Trim();
            string password = textBox3.Text.Trim();
            string travelerId = TravelerID; // take from login form

            if (string.IsNullOrEmpty(accountNumber) || string.IsNullOrEmpty(bankName) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("All fields are required!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

           
                con.Open();

                // Check if account number already exists
                string checkQuery = "SELECT COUNT(*) FROM Payment_Regi WHERE AccountNumber = @AccountNumber";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@AccountNumber", accountNumber);

                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    MessageBox.Show("This account number already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox1.Clear();
                textBox1.Focus();
                con.Close();
                return;
                }

                // Insert new payment info
                string insertQuery = @"INSERT INTO Payment_Regi(AccountNumber, TravelerID, BankName, AccPassword)
                               VALUES (@AccountNumber, @TravelerID, @BankName, @Password)";
                SqlCommand insertCmd = new SqlCommand(insertQuery, con);
                insertCmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                insertCmd.Parameters.AddWithValue("@TravelerID", travelerId);
                insertCmd.Parameters.AddWithValue("@BankName", bankName);
                insertCmd.Parameters.AddWithValue("@Password", password); // consider hashing later

                insertCmd.ExecuteNonQuery();

                MessageBox.Show("Payment account registered successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            con.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frmPaymentMethod fpm = new frmPaymentMethod(Amount,TravelerID,bookingID);
            fpm.Show();
            this.Close();
        }
    }
}
