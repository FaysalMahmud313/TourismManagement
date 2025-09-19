using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TourismManagement
{
    public partial class frmPaymentMethod : Form
    {
        private decimal Amount;
        private string Tid;
        private string Bid;
        public frmPaymentMethod(decimal amount,string id, string bookingID)
        {
            InitializeComponent();
            
            
            this.Bid = bookingID;
            this.Amount = amount;
            this.Tid = id;
           

        }

        private void button1_Click(object sender, EventArgs e)
        {
            frmBkashPayment fbp = new frmBkashPayment(Amount,Bid,Tid);
            fbp.Show();
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            frmPaymentRegistration fpr = new frmPaymentRegistration(Tid,Bid,Amount);
            fpr.Show();
            this.Close();
        }

        private void frmPaymentMethod_Load(object sender, EventArgs e)
        {
            label4.Text = Amount.ToString("F2") + " TK";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frmNagadPayment fnp = new frmNagadPayment(Amount,Bid,Tid);
            fnp.Show();
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            frmBankPayment fbp = new frmBankPayment(Amount, Tid, Bid);
            fbp.Show();
            this.Close();
        }
    }
}
