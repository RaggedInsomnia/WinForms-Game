using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Kursach
{
    class Enemy
    {
        public int x;
        public int y;
        static public int speed = 3;
        public bool Dead = false;

        static public int size;
        public Image img;
        private int difficulty;

        public Enemy(int x, int y, int diff)
        {
            Random rnd = new Random();
            int i = rnd.Next(1, 4);
            if (i == 1)
                img = new Bitmap(Properties.Resources.enemy1);
            else if (i == 2)
                img = new Bitmap(Properties.Resources.enemy2);
            else
                img = new Bitmap(Properties.Resources.enemy3);
            img.RotateFlip(RotateFlipType.Rotate180FlipX);
            this.x = x;
            this.y = y;
            size = 50;
            difficulty = diff;
        }

        public void Move(Player player)
        {
            y += speed;
            if (difficulty == 3 && player.x >= x - 150 && x >= player.x)
                x -= 1;
            if (difficulty == 3 && player.x <= x + 150 && x <= player.x)
                x += 1;
        }
        public void Shoot(Form1 form, Player player)
        {
            if (player.x >= x - 100 && player.x <= x + 100)
            {
                PictureBox bullet = new PictureBox
                {
                    Image = Properties.Resources.bullet,
                    Size = new Size(5, 20),
                    Tag = "ebullet",
                    Left = x + size / 2,
                    Top = y + size
                };
                bullet.Image.RotateFlip(RotateFlipType.Rotate180FlipX);
                form.Controls.Add(bullet);
            }
        }
    }
}
