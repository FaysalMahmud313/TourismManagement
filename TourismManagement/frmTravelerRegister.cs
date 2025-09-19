using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace TourismManagement
{
    public partial class frmTravelerRegister : Form
    {
        private int value;
        private string ID;
        public frmTravelerRegister(int val,string id)
        {
            InitializeComponent();
            this.value = val;
            this.ID = id;
        }

        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
       
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

            string userType = "";
            if (radioButton4.Checked) userType = "Traveler";
            else if (radioButton3.Checked) userType = "Airline";
            else if (radioButton5.Checked) userType = "Hotel Owner";
            else if (radioButton6.Checked) userType = "Vehicle Owner";

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(nid) ||string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(gender) || string.IsNullOrEmpty(userType))
            {
                MessageBox.Show("Please fill all fields!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!long.TryParse(phone, out _))
            {
                MessageBox.Show("Phone number must be numeric!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            Random rnd = new Random();
            string userID = "";
           
            bool exists;

            do
            {
                int number = rnd.Next(1000, 9999);

                if (userType == "Traveler")
                    userID = "T" + number;
                else if (userType == "Airline")
                    userID = "VA" + number;
                else if (userType == "Hotel Owner")
                    userID = "VH" + number;
                else if (userType == "Vehicle Owner")
                    userID = "VV" + number;

                string query = @"
        SELECT COUNT(*) FROM Admin_Info WHERE AdminID = @UserID
        UNION ALL
        SELECT COUNT(*) FROM Traveler_Info WHERE TravelerID = @UserID
        UNION ALL
        SELECT COUNT(*) FROM Vendor_Info WHERE VendorID = @UserID
        UNION ALL
        SELECT COUNT(*) FROM Login WHERE UserID = @UserID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserID", userID);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                exists = false;
                while (reader.Read())
                {
                    if (reader.GetInt32(0) > 0)
                    {
                        exists = true;
                        break;
                    }
                }
                reader.Close();
                con.Close();

            } while (exists);

            SendEmail se = new SendEmail();

            byte[] imgData = null;
            if (pictureBox2.Image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    pictureBox2.Image.Save(ms, pictureBox2.Image.RawFormat);
                    imgData = ms.ToArray();
                }
            }
            if (imgData == null)
            {
                MessageBox.Show("Image Required! Please Provide an Image.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            con.Open();
            SqlTransaction tran = con.BeginTransaction();

            try
            {
                if (userType == "Traveler")
                {
                  
                    
                    SqlCommand cmd = new SqlCommand("INSERT INTO Traveler_Info values ('"+ userID + "','"+ name + "','"+ email + "','"+nid+"','"+phone+"','"+gender+"','"+userType+"','"+txtSetPassword.Text+"',@Picture)", con, tran);
                    cmd.Parameters.AddWithValue("@Picture", (object)imgData ?? DBNull.Value);
                    cmd.ExecuteNonQuery();

                    SqlCommand cmd1 = new SqlCommand("insert into Login values('" + userID + "','" + txtSetPassword.Text + "', 3)", con, tran);
                    cmd1.ExecuteNonQuery();

                    se.Email(userID, name, userType, email, txtSetPassword.Text);
                }
                else
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO Vendor_Info values ('" + userID + "','" + name + "','" + email + "','" + nid + "','" + phone + "','" + gender + "','" + userType + "','" + txtSetPassword.Text + "',@Picture,'Invalid')", con, tran);
                    cmd.Parameters.AddWithValue("@Picture", (object)imgData ?? DBNull.Value);
                    cmd.ExecuteNonQuery();

                    SqlCommand cmd2 = new SqlCommand("insert into Login values('" + userID + "','" + txtSetPassword.Text + "', 2)", con, tran);
                    cmd2.ExecuteNonQuery();

                    string walletId = GenerateUniqueWalletID(con,tran);

                    // 4️⃣ Insert vendor wallet
                    SqlCommand cmdWallet = new SqlCommand(@"
                    INSERT INTO Vendor_Wallet (WalletID, VendorID, TotalEarning) 
                    VALUES (@WalletID, @VendorID, 0)", con, tran);

                    cmdWallet.Parameters.AddWithValue("@WalletID", walletId);
                    cmdWallet.Parameters.AddWithValue("@VendorID", userID);
                    cmdWallet.ExecuteNonQuery();

                    se.EmailVendorPending(name, email);
                }

                tran.Commit();
                MessageBox.Show("User registered successfully!\nGenerated ID: " + userID, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
               
                
                

                txtName.Clear();
                txtEmail.Clear();
                txtNid.Clear();
                txtPhone.Clear();
                pictureBox2.Image = null;
                radioButton1.Checked = false;
                radioButton2.Checked = false;
                radioButton3.Checked = false;
                radioButton4.Checked = false;
                radioButton5.Checked = false;
                radioButton6.Checked = false;
                button2.Show();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }


        }

        public string GenerateUniqueWalletID(SqlConnection con, SqlTransaction tran)
        {
            string walletId;
            Random rnd = new Random();

            while (true)
            {
                walletId = "WV" + rnd.Next(1000, 9999);

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Vendor_Wallet WHERE WalletID=@WalletID", con,tran))
                {
                    cmd.Parameters.AddWithValue("@WalletID", walletId);
                    
                    int count = (int)cmd.ExecuteScalar();
                    if (count == 0)
                        return walletId; // Found unique ID
                }
            }
        }
        private void button11_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            new frmUser().Show();
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (value == 1)
            {
                new frmAdmin(ID).Show();
                this.Close();
            }
            else
            {
                new frmLogin().Show();
                this.Close();
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

        private void txtName_Enter(object sender, EventArgs e)
        {
            if (txtName.Text == "Name")
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
            if (txtEmail.Text == "Email")
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
            if (txtNid.Text == "Nid/Passport")
            {
                txtNid.Text = "";
                txtNid.ForeColor = Color.Black;
            }
        }

        private void txtNid_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNid.Text))
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
    }
}
