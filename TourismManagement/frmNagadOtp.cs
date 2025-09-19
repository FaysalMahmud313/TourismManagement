using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace TourismManagement
{
    public partial class frmNagadOtp : Form
    {
        private frmNagadPayment parent;

        private string bookingId;
        private string travelerId;
        private string paymentId;
        private decimal amount;

        private int timeLeft = 30; // 30 seconds
        private Timer otpTimer;
        public frmNagadOtp(frmNagadPayment p, string bookingID, string travelerID, string paymentID, decimal totalBill)
        {
            InitializeComponent();
            this.parent = p;
            this.bookingId = bookingID;
            this.amount = totalBill;
            this.travelerId = travelerID;
            this.paymentId = paymentID;

            timeLeft = 30;
            label1.Text = "Time left: 30s";

            otpTimer = new Timer();
            otpTimer.Interval = 1000; // 1 second
            otpTimer.Tick += timer1_Tick;
            otpTimer.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string otp = textBox2.Text;

            if (string.IsNullOrWhiteSpace(otp))
            {
                MessageBox.Show("Please enter OTP!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // stop execution
            }

            if (otp == parent.CurrentOtp)
            {
                parent.loadForm(new frmNagadPin(parent,bookingId,travelerId,paymentId,amount));
            }
            else
            {
                MessageBox.Show("Invalid OTP!");
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "e.g xxxxxx")
            {
                textBox2.Text = "";
                textBox2.ForeColor = Color.Black;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                textBox2.Text = "e.g xxxxxx";
                textBox2.ForeColor = Color.Gray;

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            parent.loadForm(new frmNagodNumber(parent,bookingId,travelerId,paymentId,amount));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timeLeft > 0)
            {
                timeLeft--;
                label1.Text = "Time left: " + timeLeft + "s";
            }
            else
            {
                otpTimer.Stop();
                label1.Text = "OTP expired!";
                button2.Enabled = false; // disable verify button
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string Otp = GenerateOTP();
            parent.CurrentOtp = Otp;

            frmNagodNumber fbn = new frmNagodNumber(parent, bookingId, travelerId, paymentId, amount);
            fbn.SendOTP(parent.CurrentEmail, Otp, parent.CurrentName);
            timeLeft = 30;
            otpTimer.Start();
            button1.Enabled = true;
        }

        private string GenerateOTP()
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString(); // 6-digit OTP
        }
    }
}
