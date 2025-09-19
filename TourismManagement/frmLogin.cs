using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace TourismManagement
{
    public partial class frmLogin : Form
    {
        SqlConnection con=new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        
        public frmLogin()
        {
            InitializeComponent();
            
        }

        public string TravelerID;
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "User ID")
            {
                textBox1.Text = "";
                textBox1.ForeColor = Color.Black;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "User ID";
                textBox1.ForeColor = Color.Gray;
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "Password")
            {
                textBox2.Text = "";
                textBox2.ForeColor = Color.Black;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                textBox2.Text = "Password";
                textBox2.ForeColor = Color.Gray;
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            con.Open();
            SqlCommand cmd=new SqlCommand("Select UserID , Role From Login Where UserID='"+ textBox1.Text + "' AND Password='"+ textBox2.Text + "'",con);
          
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string id = reader["UserID"].ToString();
                int role = Convert.ToInt32(reader["Role"]);

                reader.Close();
                if(role==1)
                {
                    frmAdmin admin = new frmAdmin(id);
                    admin.Show();
                    this.Hide();
                }
                else if (role == 2) // Vendor
                {
                    string status = "";

                    
                        string query = "SELECT Status FROM Vendor_Info WHERE VendorID = @VendorID";
                        using (SqlCommand cmd1 = new SqlCommand(query, con))
                        {
                            cmd1.Parameters.AddWithValue("@VendorID", id);
                            
                            object result = cmd1.ExecuteScalar();
                            

                            if (result != null)
                                status = result.ToString();
                        }
                    

                    if (status == "Valid")
                    {
                        if (id.StartsWith("VA"))
                        {
                            frmVendorFlight vf = new frmVendorFlight(id);
                          
                            vf.Show();
                            this.Hide();
                        }
                        else if (id.StartsWith("VH"))
                        {
                            frmAddHotel vh = new frmAddHotel(id);
                            //vh.ShowNameH(id);
                            vh.Show();
                            this.Hide();
                        }
                        else
                        {
                            frmVendorVehicle vv = new frmVendorVehicle();
                            vv.ShowName(id);
                            vv.Show();
                            this.Hide();
                        }

                        /*frmVendorDashBoard fvb = new frmVendorDashBoard(id);
                        fvb.Show();
                        this.Hide();  */  
                    }
                    else
                    {
                        MessageBox.Show("Your account activation is pending.\nPlease wait until approval by Brainstomers.",
                                        "Activation Pending",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                    }
                }
                else if(role == 3)
                {
                    frmTraveler tr = new frmTraveler(id);
                    tr.Show();
                    this.Hide();
                }
               
            }
            else
            {
                reader.Close();
                con.Close();
                MessageBox.Show("Invalid ID or Password!", "Login Failed",MessageBoxButtons.OK,MessageBoxIcon.Error);
                textBox1.Clear();
                textBox2.Clear();

                textBox1.Focus();
                return;
            }
            con.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new frmTravelerRegister(2,null).Show();
            this.Hide();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            new frmUser().Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new frmForgetPassword().Show(); 
            this.Hide();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            new frmAboutUs().Show();
        }
    }
}
