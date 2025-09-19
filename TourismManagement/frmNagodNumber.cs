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

namespace TourismManagement
{
    public partial class frmNagodNumber : Form
    {
        private frmNagadPayment parent;

        private string bookingId;
        private string travelerId;
        private string paymentId;
        private decimal amount;
        public frmNagodNumber(frmNagadPayment p, string bookingID, string travelerID, string paymentID, decimal totalBill)
        {
            InitializeComponent();
            this.parent = p;
            this.bookingId = bookingID;
            this.travelerId = travelerID;
            this.paymentId = paymentID;
            this.amount = totalBill;
        }
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        private void label7_Click(object sender, EventArgs e)
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

        private void button2_Click(object sender, EventArgs e)
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

                        parent.CurrentEmail = email;
                        string bankName = reader["BankName"].ToString();
                        if (bankName.ToLower().Contains("nagad"))
                        {

                            // ✅ Generate OTP
                            string otp = GenerateOTP();
                            parent.CurrentOtp = otp;
                            parent.CurrentAcc = accNo;

                            // ✅ Send to email (include name if you want)
                            SendOTP(email, otp, name);

                            // ✅ Go to OTP form
                            parent.loadForm(new frmNagadOtp(parent,bookingId,travelerId,paymentId,amount));
                        }
                        else
                        {
                            MessageBox.Show("This is not a Nagad account!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid Account Number!");
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

        private string GenerateOTP()
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString(); // 6-digit OTP
        }

        public void SendOTP(string email, string otp, string name)
        {



            if (!string.IsNullOrEmpty(email))
            {
                string body = $@"
Hello {name},

Here is your Nagad Payment OTP:
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

        private void button1_Click(object sender, EventArgs e)
        {
            parent.Close();
        }
    }
}
