using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Kursach
{
    class Player
    {
        public int x;
        public int y;
        static public int speed = 10;

        static public int size;
        public Image img;
        public int HP;
        public bool IsInvincible = false;
        private Timer PlayerTimer = new Timer();
        public Player(int x, int y, int diff)
        {
            img = new Bitmap(Properties.Resources.Ship);
            this.x = x;
            this.y = y;
            size = 50;
            PlayerTimer.Interval = 3000;
            if (diff == 1)
                HP = 5;
            else
                HP = 3;
        }
      

        public void Shoot(Form1 form)
        {
            PictureBox bullet = new PictureBox
            {
                Image = Properties.Resources.bullet,
                Size = new Size(5, 20),
                Tag = "bullet",
                Left = x + size / 2,
                Top = y - 20
            };
            form.Controls.Add(bullet);
        }


        public void TakeDamage()
        {
            HP--;
            PlayerTimer.Start();
            PlayerTimer.Tick += PlayerTimer_Tick;
            IsInvincible = true;
        }

        private void PlayerTimer_Tick(object sender, EventArgs e)
        {
            PlayerTimer.Stop();
            IsInvincible = false;
        }
    }
}
