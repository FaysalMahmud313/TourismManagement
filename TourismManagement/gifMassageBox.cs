using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TourismManagement
{
    public static class gifMassageBox
    {
        public static void Show(string title = "Message")
        {
            using (Form f = new Form())
            {
                f.StartPosition = FormStartPosition.CenterScreen;
                f.Size = new Size(320, 400);
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MaximizeBox = false;
                f.MinimizeBox = false;
                f.Text = title;

                // PictureBox for GIF
                PictureBox pb = new PictureBox();
                pb.Image = Properties.Resources.Update2; // fixed GIF from resources
                pb.SizeMode = PictureBoxSizeMode.Zoom;
                pb.Size = new Size(300, 300);
                pb.Location = new Point(10, 10);
                f.Controls.Add(pb);

                // Stop GIF after playing once
                ImageAnimator.StopAnimate(pb.Image, null); // ensures it doesn’t loop automatically
                ImageAnimator.UpdateFrames(pb.Image);      // update first frame

                // Label for message
                Label lbl = new Label();
                lbl.Text = title;
                lbl.AutoSize = false;
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.Dock = DockStyle.Bottom;
                lbl.Height = 40;
                f.Controls.Add(lbl);

                // OK button
                Button ok = new Button();
                ok.Text = "OK";
                ok.Height = 40;
                ok.Dock = DockStyle.Bottom;
                ok.Click += (s, e) => f.Close();
                f.Controls.Add(ok);

                f.ShowDialog();
            }
        }
    }
}
