using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Kursach
{
    public partial class Form1 : Form
    {
        Player player;
        List<Enemy> enemies = new List<Enemy>();
        Stack<PictureBox> Health = new Stack<PictureBox>();
        Label Score;
        int score = 0;
        int[] Records = new int[5];
        public int difficulty = 1;
        bool IsRunning = false;
        public Form1()
        {
            InitializeComponent();
            MainMenu();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public void Init()
        {
            score = 0;
            Score = new Label
            {
                Tag = "Score",
                ForeColor = Color.Red,
                BackColor = Color.Black,
                Font = new Font("Stencil", 20),  //выводим надпись 
                Size = new Size(200, 30),
                Location = new Point(10, 10)
            };
            Controls.Add(Score);
            player = new Player(Size.Width / 2 - 25, Size.Height - 100, difficulty);
            for (int i = 0; i < player.HP; i++)
            {
                PictureBox heart = new PictureBox
                {
                    Image = Properties.Resources.Heart,  //создаем 3 картинки сердца - здоровье игрока и помещаем их в стек чтобы последовательно удалять с конца
                    Size = new Size(35, 30),
                    Top = 50,
                    Left = 15 + 35 * i
                };
                Health.Push(heart);
                Controls.Add(heart);
            }
            MainTimer.Start();
            EnemyTimer.Start();
            SpawnTimer.Start();
            SpawnTimer.Interval = 2000;
            if (difficulty == 2)
                EnemyTimer.Interval = 3000;
            else if (difficulty == 3)
                EnemyTimer.Interval = 2000;
        }

        private void update(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;   //отрисовываем игрока, противников и щит игрока, если он активен
            if (IsRunning)
            {
                g.DrawImage(player.img, player.x, player.y, Player.size, Player.size);
                if (player.IsInvincible)
                {
                    Pen pen = new Pen(Color.White, 3);
                    g.DrawEllipse(pen, player.x - 5, player.y, Player.size + 10, Player.size + 10);
                }
                foreach (Enemy enemy in enemies)
                    if (!enemy.Dead)
                        g.DrawImage(enemy.img, enemy.x, enemy.y, Enemy.size, Enemy.size);
            }
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (IsRunning)
            {
                if (player.HP == 0)  //при смерти игрока
                {
                    IsRunning = false;
                    MainTimer.Stop();
                    foreach (Control bul in Controls.OfType<PictureBox>().ToList())  //удаляем все снаряды
                        Controls.Remove(bul);
                    Controls.Remove(Score);
                    Invalidate();
                    Label GO = new Label()
                    {
                        Text = "ИГРА ОКОНЧЕНА",
                        ForeColor = Color.Red,
                        BackColor = Color.Black,
                        Font = new Font("Stencil", 30),  //выводим надпись 
                        Size = new Size(400, 50),
                        Location = new Point(Width / 2 - 185, Height / 2 - 50),
                        Tag = "GameOver"
                    };
                    Button ret = new Button();
                    ret.Text = "Вернуться в главное меню";  //выводим кнопку для возвращения в ГМ
                    ret.Width = 400;
                    ret.Height = 50;
                    ret.Location = new Point(Width / 2 - 210, Height / 2 + 20);
                    ret.Font = new Font("Stencil", 18);
                    Controls.Add(GO);
                    Controls.Add(ret);
                    for (int i = 0; i < 5; i++)   //записываем в таблицу рекордов кол-во очков
                        if (Records[i] < score)
                        {
                            for (int j = 4; j >= i && j >= 1; j--)
                                Records[j] = Records[j - 1];
                            Records[i] = score;
                            break;
                        }
                    if (File.Exists("Records.txt"))
                    {
                        int i = 0;
                        using (StreamWriter writer = new StreamWriter(File.OpenWrite("Records.txt")))  //при выходе из игры записываем таблицу рекордов в файл
                            for (i = 0; i < 5; i++)
                                writer.WriteLine(Records[i]);
                    }
                    ret.Click += Ret_Click;
                }
                foreach (Control bul in Controls)
                    if (bul is PictureBox && bul.Tag == "bullet")
                    {
                        bul.Top -= 15;
                        if (bul.Top < Height - 990)  //движение снарядов игрока
                            Controls.Remove(bul);
                    }
                foreach (Control bul in Controls)
                    if (bul is PictureBox && bul.Tag == "ebullet")
                    {
                        bul.Top += 10;
                        if (bul.Top > Height - 10)  //движение снарядов врагов
                            Controls.Remove(bul);
                    }
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i].y < Height)
                        enemies[i].Move(player);  //если враг находится в пределах экрана, он двигается
                    else
                    {
                        enemies[i].img.Dispose();  //при выходе за пределы экрана враг удаляется
                        enemies.Remove(enemies[i]);
                    }
                    foreach (Control bul in Controls)
                        if (bul is PictureBox && bul.Tag == "bullet")
                            if (bul.Top <= enemies[i].y + Enemy.size && bul.Top >= enemies[i].y - bul.Size.Height && bul.Left >= enemies[i].x - 10 && bul.Left <= enemies[i].x + Enemy.size)
                            {
                                enemies[i].img.Dispose();  //коллизия противников со снарядами игрока
                                enemies[i].Dead = true;
                                score += 100;
                                bul.Dispose();
                            }
                }
                for (int i = enemies.Count - 1; i >= 0; i--)
                    if (enemies[i].Dead == true)   //удаление уничтоженных врагов
                        enemies.Remove(enemies[i]);
                foreach (Control bul in Controls)
                    if (bul is PictureBox && bul.Tag == "ebullet")
                        if (bul.Bottom >= player.y && bul.Bottom <= player.y + Player.size + bul.Size.Height && bul.Left <= player.x + Player.size && bul.Right >= player.x)
                        {
                            if (!player.IsInvincible)
                            {
                                Controls.Remove(Health.Pop());  //коллизия игрока со снарядами врагов
                                player.TakeDamage();
                            }
                            bul.Dispose();
                        }
                foreach (Enemy enemy in enemies)
                {
                    if (enemy.y + Enemy.size >= player.y && enemy.y + Enemy.size <= player.y + Player.size && enemy.x <= player.x + Player.size && enemy.x + Enemy.size >= player.x)
                        if (!player.IsInvincible)
                        {
                            Controls.Remove(Health.Pop());  //коллизия игрока с врагами
                            player.TakeDamage();
                        }
                }
                Score.Text = $"ОЧКИ: {score}";
                Invalidate();
            }
        }

        private void Ret_Click(object sender, EventArgs e)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
                enemies.Remove(enemies[i]);
            foreach (Control item in Controls)
                if (item is Label && item.Tag == "GameOver")
                    item.Dispose();
            foreach (Control but in Controls.OfType<Button>().ToList())
                Controls.Remove(but);
            foreach (Control bul in Controls.OfType<PictureBox>().ToList())
                Controls.Remove(bul);

            MainMenu();
        }

        private void EnemyTimer_Tick(object sender, EventArgs e)
        {
            if (IsRunning && MainTimer.Enabled && difficulty != 1)
                foreach (Enemy enemy in enemies)  //стрельба врагов 
                    enemy.Shoot(this, player);
        }

        private void SpawnTimer_Tick(object sender, EventArgs e)
        {
            if (MainTimer.Enabled)
            {
                if (SpawnTimer.Interval > 500)  //появление врагов в случайной позиции на экране (со временем враги появляются быстрее)
                    SpawnTimer.Interval -= 50;
                Random rnd = new Random();
                Enemy enemy = new Enemy(rnd.Next(100, 500), -50, difficulty);
                enemies.Add(enemy);
            }
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (IsRunning)
            {
                if (player.x >= 20)
                    if (e.KeyCode == Keys.A)
                        player.x -= Player.speed;
                if (player.x <= Size.Width - Player.size - 40)  //управление движением игрока
                    if (e.KeyCode == Keys.D)
                        player.x += Player.speed;

                if (e.KeyCode == Keys.Escape)  //пауза
                {
                    MainTimer.Stop();
                    EnemyTimer.Stop();
                    PauseMenu();
                }
            }
        }

        private void OnClick(object sender, EventArgs e)
        {
            if (IsRunning && MainTimer.Enabled)  //управление (стрельба) с помощью ЛКМ
                player.Shoot(this);
        }

        private void MainMenu()
        {
            if (File.Exists("Records.txt"))
            {
                int i = 0;
                using (StreamReader reader = new StreamReader(File.OpenRead("Records.txt")))  //считываем из файла сохраненные рекорды
                    while (reader.Peek() != -1)
                    {
                        Records[i] = int.Parse(reader.ReadLine());
                        i++;
                    }
            }
            else
                File.Create("Records.txt");
            Button start = new Button();
            Button settings = new Button();  //Главное меню при запуске игры (создаем 3 кнопки)
            Button exit = new Button();
            start.Text = "Начать игру";
            settings.Text = "Выбор сложности";
            exit.Text = "Выйти из игры";
            start.BackColor = Color.White;
            start.Width = 200;
            start.Height = 50;
            start.Top = Height / 2 - start.Height - 100;
            start.Left = Width / 2 - start.Width / 2;
            start.Font = new Font("Stencil", 18);
            Controls.Add(start);

            settings.BackColor = Color.White;
            settings.Width = 200;
            settings.Height = 70;
            settings.Top = Height / 2 - settings.Height + 10;
            settings.Left = Width / 2 - settings.Width / 2;
            settings.Font = new Font("Stencil", 18);
            Controls.Add(settings);

            exit.BackColor = Color.White;
            exit.Width = 200;
            exit.Height = 50;
            exit.Top = Height / 2 - exit.Height + 100;
            exit.Left = Width / 2 - exit.Width / 2;
            exit.Font = new Font("Stencil", 18);
            Controls.Add(exit);
            start.Click += Start_Click;
            exit.Click += Exit_Click;
            settings.Click += Settings_Click;

            Label Rec = new Label()
            {
                Text = "РЕКОРДЫ",
                Tag = "Records",
                Font = new Font("Stencil", 18),   //вывод таблицы рекордов
                ForeColor = Color.Red,
                BackColor = Color.Black,
                Size = new Size(140, 50),
                Location = new Point(50, start.Top)
            };
            Controls.Add(Rec);
            Label[] Recs = new Label[5];
            for (int i = 0; i < 5; i++)
            {
                if (Records[i] != 0)
                {
                    Recs[i] = new Label();
                    Recs[i].Text = Records[i].ToString();
                    Recs[i].Tag = "Records";
                    Recs[i].Font = new Font("Stencil", 18);
                    Recs[i].ForeColor = Color.Red;
                    Recs[i].BackColor = Color.Black;
                    Recs[i].Size = new Size(160, 50);
                    Recs[i].Location = new Point(Rec.Location.X, Rec.Location.Y + Rec.Size.Height + Recs[i].Size.Height * i);
                    Controls.Add(Recs[i]);
                }
            }

            Label diff = new Label()
            {
                Text = "СЛОЖНОСТЬ",
                Tag = "difficulty",
                Font = new Font("Stencil", 18),
                ForeColor = Color.Red,
                BackColor = Color.Black,
                Size = new Size(170, 50),
                Location = new Point(Width - 220, start.Top)
            };
            Controls.Add(diff);
            Label difflvl = new Label();
            if (difficulty == 1)
                difflvl.Text = "Легкая";
            else if (difficulty == 2)
                difflvl.Text = "Средняя";
            else
                difflvl.Text = "Высокая";
            difflvl.Tag = "difficulty";
            difflvl.Font = new Font("Stencil", 18);
            difflvl.ForeColor = Color.Red;
            difflvl.BackColor = Color.Black;
            difflvl.Size = new Size(170, 50);
            difflvl.Location = new Point(diff.Location.X, diff.Location.Y + 60);
            Controls.Add(difflvl);

        }

        private void Settings_Click(object sender, EventArgs e)
        {
            foreach (Control item in Controls.OfType<Button>().ToList())  //очищаем экран
                Controls.Remove(item);
            foreach (Control item in Controls.OfType<Label>().ToList())
                Controls.Remove(item);
            Button easy = new Button();
            Button normal = new Button();   //создаем три кнопки
            Button hard = new Button();

            easy.Text = "Легкая";
            easy.BackColor = Color.White;
            easy.Width = 200;
            easy.Height = 50;
            easy.Top = Height / 2 - easy.Height - 100;
            easy.Left = Width / 2 - easy.Width / 2;
            easy.Font = new Font("Stencil", 18);
            Controls.Add(easy);

            normal.Text = "Средняя";
            normal.BackColor = Color.White;
            normal.Width = 200;
            normal.Height = 50;
            normal.Top = Height / 2 - normal.Height;
            normal.Left = Width / 2 - normal.Width / 2;   
            normal.Font = new Font("Stencil", 18);
            Controls.Add(normal);

            hard.Text = "Высокая";
            hard.BackColor = Color.White;
            hard.Width = 200;
            hard.Height = 50;
            hard.Top = Height / 2 - hard.Height + 100;
            hard.Left = Width / 2 - hard.Width / 2;
            hard.Font = new Font("Stencil", 18);
            Controls.Add(hard);
            easy.Click += Easy_Click;
            normal.Click += Normal_Click;
            hard.Click += Hard_Click;
        }

        private void Easy_Click(object sender, EventArgs e)
        {
            difficulty = 1;
            foreach (Control item in Controls.OfType<Button>().ToList())  
                Controls.Remove(item);
            MainMenu();
        }

        private void Normal_Click(object sender, EventArgs e)
        {
            difficulty = 2;
            foreach (Control item in Controls.OfType<Button>().ToList())  
                Controls.Remove(item);
            MainMenu();
        }
        private void Hard_Click(object sender, EventArgs e)
        {
            difficulty = 3;
            foreach (Control item in Controls.OfType<Button>().ToList())  
                Controls.Remove(item);
            MainMenu();
        }

        private void Start_Click(object sender, EventArgs e)
        {
            IsRunning = true;
            foreach (Control but in Controls.OfType<Button>().ToList())  //при нажатии на кнопку "Начать игру" в главном меню удаляем все кнопки и запускаем игру
                Controls.Remove(but);
            foreach (Control lab in Controls.OfType<Label>().ToList())
                Controls.Remove(lab);
            Init();
        }
        private void Exit_Click(object sender, EventArgs e)
        {

            Application.Exit();  //при нажатии "Выйти из игры" в ГМ закрываем приложение
        }

        private void PauseMenu() //меню паузы по нажатию на escape
        {
            Label pause = new Label()
            {
                Text = "МЕНЮ ПАУЗЫ",
                Tag = "Pause",
                Font = new Font("Stencil", 18),  //надпись "Меню паузы"
                ForeColor = Color.Red,
                BackColor = Color.Black,
                Size = new Size(250, 50),
                Location = new Point(Width / 2 - 85, Height / 2 - 90)
            };
            Controls.Add(pause);
            Button cont = new Button();  //кнопки "Продолжить" и "Выйти в меню"
            Button exit = new Button();
            cont.Text = "Продолжить";
            exit.Text = "Выйти в меню";
            cont.BackColor = Color.White;
            cont.Width = 200;
            cont.Height = 50;
            cont.Top = Height / 2 - 30;
            cont.Left = Width / 2 - 100;
            cont.BringToFront();
            cont.Font = new Font("Stencil", 18);
            Controls.Add(cont);
            exit.BackColor = Color.White;
            exit.Width = 200;
            exit.Height = 50;
            exit.Top = Height / 2 + 40;
            exit.Left = Width / 2 - 100;
            exit.BringToFront();
            exit.Font = new Font("Stencil", 18);
            Controls.Add(exit);
            cont.Click += Cont_Click;
            exit.Click += Exit_Click1;
        }

        private void Exit_Click1(object sender, EventArgs e)
        {
            IsRunning = false;
            foreach (Control bul in Controls.OfType<PictureBox>().ToList())  //при нажатии "Выйти в меню" в меню паузы
                Controls.Remove(bul);                                        //удаляем все кнопки, врагов, снаряды и игрока и запускаем главное меню
            foreach (Control but in Controls.OfType<Button>().ToList())
                Controls.Remove(but);
            foreach (Control lab in Controls.OfType<Label>().ToList())
                if (lab.Tag == "Pause" || lab.Tag == "Score")
                    Controls.Remove(lab);
            Invalidate();
            enemies.Clear();
            for (int i = 0; i < 5; i++)   //записываем в таблицу рекордов кол-во очков
                if (Records[i] < score)
                {
                    for (int j = 4; j >= i && j >= 1; j--)
                        Records[j] = Records[j - 1];
                    Records[i] = score;
                    break;
                }
            if (File.Exists("Records.txt"))
            {
                int i = 0;
                using (StreamWriter writer = new StreamWriter(File.OpenWrite("Records.txt")))  //при выходе из игры записываем таблицу рекордов в файл
                    for (i = 0; i < 5; i++)
                        writer.WriteLine(Records[i]);
            }
            MainMenu();
        }

        private void Cont_Click(object sender, EventArgs e)
        {
            foreach (Control but in Controls.OfType<Button>().ToList())
                Controls.Remove(but);
            foreach (Control lab in Controls.OfType<Label>().ToList())  //при нажатии "Продолжить" в МП удаляем кнопки и надпись "Меню паузы" и продолжаем игру
                if (lab.Tag == "Pause")
                    Controls.Remove(lab);
            MainTimer.Start();
            EnemyTimer.Start();
        }
    }
}
