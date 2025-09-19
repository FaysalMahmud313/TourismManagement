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
    public partial class frmNagadPin : Form
    {
        private frmNagadPayment parent;

        private string bookingId;
        private string travelerId;
        private string paymentId;
        private decimal amount;
        public frmNagadPin(frmNagadPayment p, string bookingID, string travelerID, string paymentID, decimal totalBill)
        {
            InitializeComponent();
            this.parent = p;
            this.bookingId = bookingID;
            this.travelerId = travelerID;
            this.paymentId = paymentID;
            this.amount = totalBill;
        }

        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");

        private void button2_Click(object sender, EventArgs e)
        {
            string enteredPin = "";
            enteredPin = textBox2.Text.Trim();

            if (string.IsNullOrWhiteSpace(enteredPin))
            {
                MessageBox.Show("Please enter PIN!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                con.Open();

                // 2️⃣ Verify PIN
                SqlCommand cmd = new SqlCommand(@"
        SELECT COUNT(*) 
        FROM Payment_Regi 
        WHERE AccountNumber = @acc AND AccPassword = @pin", con);
                cmd.Parameters.AddWithValue("@acc", parent.CurrentAcc);
                cmd.Parameters.AddWithValue("@pin", enteredPin);

                int count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    MessageBox.Show("✅ PIN matched! Payment confirmed.");

                    // 3️⃣ Insert payment record
                    string paymentID = paymentId; // Your method to create payment ID
                    string travelerID = travelerId;  // Set the traveler ID
                    decimal totalBill = amount;   // Set the total bill
                    string paymentStatus = "Paid";

                    string insertPaymentQuery = @"
            INSERT INTO Payment_Info (PaymentID, TravelerID, TotalBill, PaymentStatus, BookingID, PaymentMethod)
            VALUES (@PaymentID, @TravelerID,  @TotalBill, @PaymentStatus,@BookingID, @PaymentMethod)";

                    SqlCommand insertCmd = new SqlCommand(insertPaymentQuery, con);
                    insertCmd.Parameters.AddWithValue("@PaymentID", paymentID);
                    insertCmd.Parameters.AddWithValue("@TravelerID", travelerID);
                    insertCmd.Parameters.AddWithValue("@BookingID", bookingId); // Set the current booking ID
                    insertCmd.Parameters.AddWithValue("@TotalBill", totalBill);
                    insertCmd.Parameters.AddWithValue("@PaymentStatus", paymentStatus);
                    insertCmd.Parameters.AddWithValue("@PaymentMethod", "Nagad"); // e.g., "Bkash" or "Nagad"

                    insertCmd.ExecuteNonQuery();

                    //MessageBox.Show("Payment recorded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 4️⃣ Optional: Call PaymentHandler to update booking and seat/room status
                    PaymentHandler handler = new PaymentHandler();
                    handler.ProcessSuccessfulPayment(paymentID, bookingId, travelerID);
                }
                else
                {
                    MessageBox.Show("❌ Invalid PIN!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "******")
            {
                textBox2.Text = "";
                textBox2.ForeColor = Color.Black;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                textBox2.Text = "******";
                textBox2.ForeColor = Color.Gray;

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            parent.loadForm(new frmNagadOtp(parent,bookingId,travelerId,paymentId,amount));
        }
    }
}
