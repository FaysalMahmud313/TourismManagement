using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TourismManagement
{
    public partial class frmAddAdmin : Form
    {
        private string id;
        SqlConnection con=new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        public frmAddAdmin(string adminID)
        {
            InitializeComponent();
            this.id = adminID;
        }
        private string GenerateAdminID()
        {
            Random rnd = new Random();
            string newID;
            bool exists;

            
                do
                {
                    int number = rnd.Next(1000, 9999); // 4-digit random number
                    newID = "A" + number;

                    string query = "SELECT COUNT(*) FROM Admin_Info WHERE AdminID = @AdminID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@AdminID", newID);

                    con.Open();
                    int count = (int)cmd.ExecuteScalar();
                    con.Close();

                    if (count > 0)
                    {
                        exists = true; 
                    }
                    else
                    {
                        exists = false; 
                    }
                }
                while (exists); 
            

            return newID;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string nid = txtNid.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string gender = "";
            if (radioButton1.Checked)
                gender = "Male";
            else if (radioButton2.Checked)
                gender = "Female";

            if (txtSetPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Password doesn't match! Please re-enter.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                
                txtSetPassword.Clear();
                txtConfirmPassword.Clear();

                
                txtSetPassword.Focus();

                return; 
            }
            if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(nid) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(gender))
            {
                MessageBox.Show("Please fill all fields!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!long.TryParse(phone, out _))
            {
                MessageBox.Show("Phone number must be numeric!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string adminID= GenerateAdminID();
            int role = 1;

            byte[] imgData = null;
            if (pictureBox2.Image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    pictureBox2.Image.Save(ms, pictureBox2.Image.RawFormat); // save as bytes
                    imgData = ms.ToArray();
                }
            }
            if(imgData==null)
            {
                MessageBox.Show("Image Required! Please Provide an Image.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            con.Open();
            SqlCommand cmd = new SqlCommand("insert into Admin_Info values('"+adminID+"','" + name + "','" + email + "', '" + nid + "','" + phone + "','"+gender+"','" + DateTime.Parse(dateTimePicker1.Text) + "','"+txtSetPassword.Text+"',@Picture)", con);
            cmd.Parameters.Add("@Picture", SqlDbType.VarBinary).Value = imgData;
            cmd.ExecuteNonQuery();

            SqlCommand cmd1 = new SqlCommand("insert into Login values('" + adminID + "','" + txtSetPassword.Text + "','"+role+"')", con);
            cmd1.ExecuteNonQuery();
            con.Close();

            DialogResult result = MessageBox.Show("Admin Added Successfully!\nAdminID: " + adminID, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SendEmail se = new SendEmail();
            se.Email(adminID, name, "Admin", email, txtSetPassword.Text);

            if (result == DialogResult.OK)
            {
                new frmAdmin(id).Show();
                this.Hide();
            }

        }
      
        

        private void txtName_Enter(object sender, EventArgs e)
        {
            if(txtName.Text =="Name")
            {
                txtName.Text = "";
                txtName.ForeColor = Color.Black;
            }
        }

        private void txtName_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                txtName.Text = "Name";
                txtName.ForeColor = Color.Gray;

            }
        }

        private void txtEmail_Enter(object sender, EventArgs e)
        {
            if( txtEmail.Text =="Email")
            {
                txtEmail.Text = "";
                txtEmail.ForeColor = Color.Black;
            }
        }

        private void txtEmail_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                txtEmail.Text = "Email";
                txtEmail.ForeColor = Color.Gray;

            }
        }

        private void txtNid_Enter(object sender, EventArgs e)
        {
            if(txtNid.Text=="Nid/Passport")
            {
                txtNid.Text = "";
                txtNid.ForeColor = Color.Black;
            }
        }

        private void txtNid_Leave(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtNid.Text))
            {
                txtNid.Text = "Nid/Passport";
                txtNid.ForeColor = Color.Gray;
            }
        }

        private void txtPhone_Enter(object sender, EventArgs e)
        {
            if (txtPhone.Text == "Phone Number")
            {
                txtPhone.Text = "";
                txtPhone.ForeColor = Color.Black;
            }
        }

        private void txtPhone_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                txtPhone.Text = "Phone Number";
                txtPhone.ForeColor = Color.Gray;
            }
        }
        private void txtSetPassword_Enter(object sender, EventArgs e)
        {
            if (txtSetPassword.Text == "Set Password")
            {
                txtSetPassword.Text = "";
                txtSetPassword.ForeColor = Color.Black;
            }
        }
        private void txtSetPassword_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSetPassword.Text))
            {
                txtSetPassword.Text = "Set Password";
                txtSetPassword.ForeColor = Color.Gray;
            }
        }

        private void txtConfirmPassword_Enter(object sender, EventArgs e)
        {
            if (txtConfirmPassword.Text == "Confirm Password")
            {
                txtConfirmPassword.Text = "";
                txtConfirmPassword.ForeColor = Color.Black;
            }
        }

        private void txtConfirmPassword_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtConfirmPassword.Text))
            {
                txtConfirmPassword.Text = "Confirm Password";
                txtConfirmPassword.ForeColor = Color.Gray;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK) // user selected a file
            {
                pictureBox2.Image = Image.FromFile(ofd.FileName);
                button2.Hide(); // ✅ hide only when a file is chosen
            }
            else
            {
                button2.Show(); // ✅ keep visible if canceled
            }
        }

    }
}
