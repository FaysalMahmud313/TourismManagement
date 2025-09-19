using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TourismManagement
{
    public class SendEmail
    {
        public void Email(string userID, string name, string type, string email, string password)
        {
            string body = $@"
Hello {name},

Congratulations! 🎉 Your account has been created successfully.

Here are your details:
----------------------------------
User ID         : {userID}
Name            : {name}
Email           : {email}
Registered As   : {type}
Password        : {password}
----------------------------------
Remember your 'User ID' and 'Password' for your next login.
Please keep this information safe.

Regards,
Team Brainstomers
";

            
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("teambrainstomers05@gmail.com"); 
            mail.To.Add(email);
            mail.Subject = "Registration Successful - Your Account Details";
            mail.Body = body;
            mail.IsBodyHtml = false; 

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential("teambrainstomers05@gmail.com", "bgeo elqz itqw wyvs");
            smtp.EnableSsl = true;

            try
            {
                smtp.Send(mail);
                MessageBox.Show("Registration email sent successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Email sending failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void EmailVendorActivation(string userID, string name, string type, string email, string password)
        {
            string body = $@"
Hello {name},

Good news! 🎉 Your vendor account has been successfully activated by the Brainstormers team. 
You can now log in and start using your account.

Here are your details:
----------------------------------
Vendor ID       : {userID}
Name            : {name}
Email           : {email}
Vendor Type     : {type}
Password        : {password}
----------------------------------
👉 Please keep your login details safe and do not share them with anyone.

Regards,
Team Brainstormers
";

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("teambrainstomers05@gmail.com");
            mail.To.Add(email);
            mail.Subject = "Account Activated - Welcome to Brainstormers Vendor Platform";
            mail.Body = body;
            mail.IsBodyHtml = false;

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential("teambrainstomers05@gmail.com", "bgeo elqz itqw wyvs");
            smtp.EnableSsl = true;

            try
            {
                smtp.Send(mail);
                MessageBox.Show("Activation email sent successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Email sending failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void EmailVendorPending(string name, string email)
        {
            string body = $@"
Hello {name},

Thank you for registering as a vendor with Brainstormers! 🌟

Your application is currently under review. 
Once your account is verified and approved by our team, you will receive a confirmation email with your login details.

We appreciate your patience and look forward to having you on board.

Regards,
Team Brainstormers
";

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("teambrainstomers05@gmail.com");
            mail.To.Add(email);
            mail.Subject = "Vendor Registration Under Review";
            mail.Body = body;
            mail.IsBodyHtml = false;

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential("teambrainstomers05@gmail.com", "bgeo elqz itqw wyvs");
            smtp.EnableSsl = true;

            try
            {
                smtp.Send(mail);
                MessageBox.Show("Pending registration email sent successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Email sending failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    
}
