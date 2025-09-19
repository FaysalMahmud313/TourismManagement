using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace TourismManagement
{
    public partial class frmBkashPin : Form
    {
        private frmBkashPayment parent;
        private string BookingID;
        private string TravelerID;
        private string PaymentID;
        private decimal amount;
        public frmBkashPin(frmBkashPayment p,string bookingId,string travelerId,string paymentId,decimal totalBill)
        {
            InitializeComponent();
            this.parent = p;
            this.BookingID = bookingId;
            this.TravelerID = travelerId;
            this.PaymentID = paymentId;
            this.amount = totalBill;
        }
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");

        private void button1_Click(object sender, EventArgs e)
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
                    gifMassageBox.Show("Payment Successful!");
                    
                    string paymentID = PaymentID; 
                    string travelerID = TravelerID; 
                    decimal totalBill = amount;   
                    string paymentStatus = "Paid";

                    string insertPaymentQuery = @"
            INSERT INTO Payment_Info (PaymentID, TravelerID, TotalBill, PaymentStatus, BookingID, PaymentMethod)
            VALUES (@PaymentID, @TravelerID,  @TotalBill, @PaymentStatus,@BookingID, @PaymentMethod)";

                    SqlCommand insertCmd = new SqlCommand(insertPaymentQuery, con);
                    insertCmd.Parameters.AddWithValue("@PaymentID", paymentID);
                    insertCmd.Parameters.AddWithValue("@TravelerID", travelerID);
                    insertCmd.Parameters.AddWithValue("@BookingID", BookingID); 
                    insertCmd.Parameters.AddWithValue("@TotalBill", totalBill);
                    insertCmd.Parameters.AddWithValue("@PaymentStatus", paymentStatus);
                    insertCmd.Parameters.AddWithValue("@PaymentMethod", "Bkash"); 

                    insertCmd.ExecuteNonQuery();

                   

                    
                    PaymentHandler handler = new PaymentHandler();
                    handler.ProcessSuccessfulPayment(paymentID, BookingID,TravelerID);

                   


                }
                else
                {
                    MessageBox.Show("❌ Invalid PIN!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error111: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void button2_Click(object sender, EventArgs e)
        {
            parent.loadForm(new frmBkashOtp(parent,BookingID,TravelerID,PaymentID,amount));
        }

       
    }
}
