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
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace TourismManagement
{
    public partial class frmForgetPassword : Form
    {
        public frmForgetPassword()
        {
            InitializeComponent();
            textBox2.ReadOnly= true;
            textBox3.ReadOnly = true;
            textBox4.ReadOnly = true;
            button2.Enabled = false;
            button3.Enabled = false;
        }

        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        private string generatedOTP;
        private void button1_Click(object sender, EventArgs e)
        {
            string userID = textBox1.Text;

            if (IsValidUserID(userID))
            {
                textBox1.ReadOnly = true;
                button1.Enabled = false;

                textBox2.ReadOnly = false;
                button2.Enabled = true;

                
                generatedOTP = GenerateOTP();
                SendOTPToUser(userID, generatedOTP);

                //MessageBox.Show("OTP has been sent to your registered email.");
            }
            else
            {
                MessageBox.Show("Invalid User ID!");
            }
        }

        private bool IsValidUserID(string userID)
        {
            try
            {
                con.Open();
                string query = "";

                    
                    if (userID.StartsWith("A"))
                    {
                        query = "SELECT COUNT(*) FROM Admin_Info WHERE AdminID='"+userID+"'";
                    }
                    else if (userID.StartsWith("T"))
                    {
                        query = "SELECT COUNT(*) FROM Traveler_Info WHERE TravelerID='"+userID+"'";
                    }
                    else 
                    {
                        query = "SELECT COUNT(*) FROM Vendor_Info WHERE VendorID='"+userID+"'";
                    }
               
                SqlCommand cmd = new SqlCommand(query, con);
                 int count = (int)cmd.ExecuteScalar();
                   return count > 0; 
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("DB Error: " + ex.Message);
                return false;
            }
            finally
            {

                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        private string GenerateOTP()
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString();
        }

        private void SendOTPToUser(string userID, string otp)
        {
            string email = "";
            string name = "";

            try
            {
                con.Open();
                string query = "";

                if (userID.StartsWith("A"))
                {
                    query = "SELECT Name,Email FROM Admin_Info WHERE AdminID=@UserID";
                }
                else if (userID.StartsWith("T"))
                {
                    query = "SELECT Name,Email FROM Traveler_Info WHERE TravelerID=@UserID";
                }
                else
                {
                    query = "SELECT Name,Email FROM Vendor_Info WHERE VendorID=@UserID";
                }

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserID", userID);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    email = reader["Email"].ToString();
                    name= reader["Name"].ToString();
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            

            finally
            {

                if (con.State == ConnectionState.Open) 
                con.Close();
            }


            if (!string.IsNullOrEmpty(email))
            {
                string body = $@"
Hello {name},

Here is your OTP to reset Password:
----------------------------------
OTP       : {otp}
----------------------------------
Regards,
Team Brainstomers
";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("teambrainstomers05@gmail.com");
                mail.To.Add(email);
                mail.Subject = "Reset Password OTP";
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

        private void button2_Click(object sender, EventArgs e)
        {
           
            string enteredOTP = textBox2.Text.Trim();

            if (enteredOTP == generatedOTP)
            {
                MessageBox.Show("OTP verified successfully! You can reset your password now.");

                textBox2.ReadOnly = true;
                button2.Enabled = false;
                textBox3.ReadOnly = false;
                textBox4.ReadOnly = false;
                button3.Enabled = true;

               
                
            }
            else
            {
                MessageBox.Show("Invalid OTP. Please try again.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox3.ReadOnly = false;
            textBox4.ReadOnly = false;
            button3.Enabled = true;

            string newPassword = textBox3.Text.Trim();
            string confirmPassword = textBox4.Text.Trim();
            string userID = textBox1.Text.Trim();

            if (newPassword == "" || confirmPassword == "")
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (newPassword == confirmPassword)
            {
               
                if (UpdatePassword(userID, newPassword))
                {
                    MessageBox.Show("Password updated successfully!");
                    new frmLogin().Show();
                    this.Close(); 
                }
                else
                {
                    MessageBox.Show("Error updating password. Try again.");
                }
            }
            else
            {
                MessageBox.Show("Passwords do not match. Please try again.");
            }

            
        }

        private bool UpdatePassword(string userID, string newPassword)
        {
            try
            {

                con.Open();
                string query = "";
                string query1 = "";
                if (userID.StartsWith("A")) 
                {
                    query = "UPDATE Admin_Info SET Password=@Password WHERE AdminID=@UserID";
                    query1 = "UPDATE Login SET Password=@Password WHERE UserID=@UserID";
                }
                else if (userID.StartsWith("T")) 
                {
                    query = "UPDATE Traveler_Info SET Password=@Password WHERE TravelerID=@UserID";
                    query1 = "UPDATE Login SET Password=@Password WHERE UserID=@UserID";
                }
                else 
                {
                    query = "UPDATE Vendor_Info SET Password=@Password WHERE VendorID=@UserID";
                    query1 = "UPDATE Login SET Password=@Password WHERE UserID=@UserID";
                }

                SqlCommand cmd = new SqlCommand(query, con);
                SqlCommand cmd1 = new SqlCommand(query1, con);


                cmd.Parameters.AddWithValue("@Password", newPassword);
                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd1.Parameters.AddWithValue("@UserID", userID);
                cmd1.Parameters.AddWithValue("@Password", newPassword);
                
                    int rows = cmd.ExecuteNonQuery();
                    cmd1.ExecuteNonQuery();
                    return rows > 0; 
                

            }
            catch (Exception ex)
            {
                MessageBox.Show("DB Error: " + ex.Message);
                return false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new frmLogin().Show();
            this.Close();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Enter UserID to Send Email ")
            {
                textBox1.Text = "";
                textBox1.ForeColor = Color.Black;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "Enter UserID to Send Email ";
                textBox1.ForeColor = Color.Gray;
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "Enter OTP from email")
            {
                textBox2.Text = "";
                textBox2.ForeColor = Color.Black;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                textBox2.Text = "Enter OTP from email";
                textBox2.ForeColor = Color.Gray;
            }
        }

        private void textBox3_Enter(object sender, EventArgs e)
        {
            if (textBox3.Text == "New Password")
            {
                textBox3.Text = "";
                textBox3.ForeColor = Color.Black;
            }
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                textBox3.Text = "New Password";
                textBox3.ForeColor = Color.Gray;
            }
        }

        private void textBox4_Enter(object sender, EventArgs e)
        {
            if (textBox4.Text == "Confirm Password")
            {
                textBox4.Text = "";
                textBox4.ForeColor = Color.Black;
            }
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                textBox4.Text = "Confirm Password";
                textBox4.ForeColor = Color.Gray;
            }
        }
    }
}
