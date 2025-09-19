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
    public partial class frmVehicle : Form
    {
        public frmVehicle()
        {
            InitializeComponent();
        }

        private void comboBox1_Enter(object sender, EventArgs e)
        {
            if (comboBox1.Text == "Bangladesh")
            {
                comboBox1.Text = "";
                comboBox1.ForeColor = Color.Black;
            }
        }

        private void comboBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(comboBox1.Text))
            {
                comboBox1.Text = "Bangladesh";
                comboBox1.ForeColor = Color.Gray;
            }
        }

        private void comboBox2_Enter(object sender, EventArgs e)
        {
            if (comboBox2.Text == "DHAKA")
            {
                comboBox2.Text = "";
                comboBox2.ForeColor = Color.Black;
            }
        }

        private void comboBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(comboBox2.Text))
            {
                comboBox2.Text = "DHAKA";
                comboBox2.ForeColor = Color.Gray;
            }
        }

        private void comboBox3_Enter(object sender, EventArgs e)
        {
            if (comboBox3.Text == "CAR")
            {
                comboBox3.Text = "";
                comboBox3.ForeColor = Color.Black;
            }
        }

        private void comboBox3_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(comboBox3.Text))
            {
                comboBox3.Text = "CAR";
                comboBox3.ForeColor = Color.Gray;
            }
        }
    }
}
