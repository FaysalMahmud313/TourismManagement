using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TourismManagement
{
    public partial class frmBkashNumber : Form
    {
        private frmBkashPayment parent;

        private string bookingId;
        private string travelerId;
        private string paymentId;
        private decimal amount;
        public frmBkashNumber(frmBkashPayment p, string bookingID, string travelerID, string paymentID, decimal totalBill)
        {
            InitializeComponent();
            parent = p;
            bookingId = bookingID;
            travelerId = travelerID;
            paymentId = paymentID;
            amount = totalBill;
        }
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        private void frmBkashNumber_Load(object sender, EventArgs e)
        {

        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "e.g. 01XXXXXXXX")
            {
                textBox2.Text = "";
                textBox2.ForeColor = Color.Black;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                textBox2.Text = "e.g. 01XXXXXXXX";
                textBox2.ForeColor = Color.Gray;

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string accNo = textBox2.Text.Trim();

            if (string.IsNullOrWhiteSpace(accNo))
            {
                MessageBox.Show("Please enter an account number!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; 
            }

            try
            {
                con.Open(); // ✅ Open the connection

                // First, get account details including bank name
                SqlCommand cmd = new SqlCommand(@"
                SELECT t.Email, t.Name, p.BankName
                FROM Payment_Regi p
                JOIN Traveler_Info t ON p.TravelerID = t.TravelerID
                WHERE p.AccountNumber = @acc", con);

                cmd.Parameters.AddWithValue("@acc", accNo);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string email = reader["Email"].ToString();
                        string name = reader["Name"].ToString();
                        string bankName = reader["BankName"].ToString();

                        if (bankName != null && bankName.ToLower().Contains("bkash"))
                        {
                            // ✅ It's a Bkash account
                            parent.CurrentEmail = email;
                            parent.CurrentName=name;
                            // ✅ Generate OTP
                            string otp = GenerateOTP();
                            parent.CurrentOtp = otp;
                            parent.CurrentAcc = accNo;

                            // ✅ Send to email
                            SendOTP(email, otp, name);

                            // ✅ Go to OTP form
                            parent.loadForm(new frmBkashOtp(parent,bookingId,travelerId,paymentId,amount));
                        }
                        else
                        {
                            // ❌ Account exists but not Bkash
                            MessageBox.Show("This is not a Bkash account!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        // ❌ Account number doesn't exist
                        MessageBox.Show("Invalid Account Number!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                con.Close(); // ✅ Close the connection
            }
        }

        

        private void button2_Click(object sender, EventArgs e)
        {
            parent.Close();
        }

        private string GenerateOTP()
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString(); // 6-digit OTP
        }
        public void SendOTP(string email,string otp,string name)
        {



            if (!string.IsNullOrEmpty(email))
            {
                string body = $@"
Hello {name},

Here is your Bkash Payment OTP:
----------------------------------
OTP       : {otp}
----------------------------------
Regards,
Team Brainstomers
";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("teambrainstomers05@gmail.com");
                mail.To.Add(email);
                mail.Subject = "Payment Varification OTP";
                mail.Body = body;
                mail.IsBodyHtml = false;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential("teambrainstomers05@gmail.com", "bgeo elqz itqw wyvs");
                smtp.EnableSsl = true;

                try
                {
                    smtp.Send(mail);
                    MessageBox.Show("OTP has been sent to your registered email!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("OTP sending failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Email not found for this user ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
}
