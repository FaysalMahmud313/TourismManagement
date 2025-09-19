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
    public partial class frmBankPayment : Form
    {
        private string travelerId;
        private string bookingId;
        private decimal Amount;
        private string paymentId;

        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        public frmBankPayment(decimal amount, string travelerID,string bookingId)
        {
            InitializeComponent();
            this.Amount = amount;
            this.travelerId = travelerID;
            this.bookingId = bookingId;
            this.paymentId= GeneratePaymentID();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string accountNumber = textBox1.Text.Trim(); // Account number input
            string enteredPin = textBox2.Text.Trim();              // PIN input
            string travelerID = travelerId;                          // Traveler ID
            string bookingID = bookingId;                             // Current booking ID
            decimal totalBill = Amount;                               // Total bill amount
            string paymentID = paymentId;                             // Generate payment ID

            if (string.IsNullOrWhiteSpace(accountNumber) || string.IsNullOrWhiteSpace(enteredPin))
            {
                MessageBox.Show("Please enter both Account Number and PIN!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                con.Open();

                // 1️⃣ Verify account number and PIN, get BankName
                SqlCommand cmd = new SqlCommand(@"
SELECT BankName 
FROM Payment_Regi 
WHERE AccountNumber = @acc AND AccPassword = @pin", con);
                cmd.Parameters.AddWithValue("@acc", accountNumber);
                cmd.Parameters.AddWithValue("@pin", enteredPin);

                object result = cmd.ExecuteScalar();

                if (result == null)
                {
                    MessageBox.Show("❌ Invalid account number or PIN!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string bankName = result.ToString();

                // 2️⃣ Restrict Bkash and Nagad
                if (bankName.Equals("Bkash", StringComparison.OrdinalIgnoreCase) ||
                    bankName.Equals("Nagad", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Account number not valid for this payment!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 3️⃣ Insert payment record
                string insertPaymentQuery = @"
INSERT INTO Payment_Info (PaymentID, TravelerID, TotalBill, PaymentStatus, BookingID, PaymentMethod)
VALUES (@PaymentID, @TravelerID, @TotalBill, @PaymentStatus, @BookingID, @PaymentMethod)";

                SqlCommand insertCmd = new SqlCommand(insertPaymentQuery, con);
                insertCmd.Parameters.AddWithValue("@PaymentID", paymentID);
                insertCmd.Parameters.AddWithValue("@TravelerID", travelerID);
                insertCmd.Parameters.AddWithValue("@TotalBill", totalBill);
                insertCmd.Parameters.AddWithValue("@PaymentStatus", "Paid");
                insertCmd.Parameters.AddWithValue("@BookingID", bookingID);
                insertCmd.Parameters.AddWithValue("@PaymentMethod", bankName);

                insertCmd.ExecuteNonQuery();

                MessageBox.Show("✅ Payment confirmed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 4️⃣ Call PaymentHandler to update booking/payment status
                PaymentHandler handler = new PaymentHandler();
                handler.ProcessSuccessfulPayment(paymentID, bookingID, travelerID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }

        }

        private string GeneratePaymentID()
        {
            string paymentID = "";
            bool isUnique = false;
            Random rnd = new Random();

            try
            {
                con.Open();

                while (!isUnique)
                {
                    // Generate a random Payment ID

                    paymentID = "BNKR" + rnd.Next(1000000, 9999999).ToString();

                    // Check if it already exists in Payment table
                    string checkQuery = "SELECT COUNT(*) FROM Payment_Info WHERE PaymentID = @PaymentID";
                    using (SqlCommand cmd = new SqlCommand(checkQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@PaymentID", paymentID);

                        object result = cmd.ExecuteScalar();
                        int count = 0;

                        if (result != null && result != DBNull.Value)
                        {
                            count = Convert.ToInt32(result);
                        }

                        if (count == 0)
                        {
                            // Unique ID found
                            isUnique = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating Payment ID: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
            }

            return paymentID;
        }

        private void frmBankPayment_Load(object sender, EventArgs e)
        {
            
        }
    }
}
