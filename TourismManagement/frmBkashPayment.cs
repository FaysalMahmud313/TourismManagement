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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TourismManagement
{
    public partial class frmBkashPayment : Form
    {
        private decimal netAmount;
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        private string bookingId;
        private string travelerId;
        private string paymentId;
        public frmBkashPayment(decimal amount, string bookingID, string travelerID)
        {
            InitializeComponent();
            this.netAmount = amount;
            this.paymentId = GeneratePaymentID();
            label2.Text = paymentId;
            label3.Text=amount.ToString("F2");
            this.travelerId = travelerID;
            this.bookingId = bookingID;

        }

        private void frmBkashPayment_Load(object sender, EventArgs e)
        {
            loadForm(new frmBkashNumber(this,bookingId,travelerId, paymentId,netAmount));
        }

        public void ClearPanel()
        {
            panel2.Controls.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        public void loadForm(Form form)
        {

            if (this.panel2.Controls.Count > 0)
                this.panel2.Controls.Clear();

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
           form.Dock = DockStyle.Fill;

            this.panel2.Controls.Add(form);
            this.panel2.Tag = form;
            form.Show();
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
                    
                    paymentID = "BKXWR" + rnd.Next(1000000, 9999999).ToString();

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

        public string CurrentOtp {  get; set; }
        public string CurrentEmail {  get; set; }
        public string CurrentAcc {  get; set; }

        public string CurrentName {  get; set; }
       
    }
}
