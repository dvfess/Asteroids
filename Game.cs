using System.Windows.Forms;
using System.Drawing;
using System;
using System.Collections.Generic;

namespace Asteroids
{
    // 1. Добавить в программу коллекцию астероидов. Как только она заканчивается(все астероиды сбиты), формируется новая коллекция, в которой на один астероид больше.
    // 2. Дана коллекция List<T>. Требуется подсчитать, сколько раз каждый элемент встречается в данной коллекции:
    //    для целых чисел;
    //    a. * для обобщенной коллекции;
    //    b. ** используя Linq.
    //Дмитрий Волков

    internal class AsteroidsEventArgs : EventArgs
    {
        private readonly int f_oldCount;
        public AsteroidsEventArgs(int count)
        {
            f_oldCount = count;
        }
        public int oldCount => f_oldCount;
    }

    internal static class Game
    {
        private static Timer timer = new Timer() { Interval = 30 };
        private static Timer rndTimer = new Timer() { Interval = 3000 };
        public static Random rnd = new Random();
        /// <summary>
        /// Количество сбитых астероидов
        /// </summary>
        private static int XP = 0;
        /// <summary>
        /// Счётчик для генерации новой волны астероидов
        /// </summary>
        private static int asteroidsCount = 3;
        /// <summary>
        /// Количество пуль на полотне
        /// </summary>
        private static int amountOfBullets = 4;

        private static BufferedGraphicsContext _context;
        private static event EventHandler<AsteroidsEventArgs> onAsteroidsEmpty;

        private static void RespawnAsteroids(object sender, AsteroidsEventArgs e)
        {
            for (var i = 0; i < e.oldCount + 1; i++)
            {
                int r = rnd.Next(5, 50);
                _asteroids.Add(new Asteroid(new Point(1000, rnd.Next(0, Game.Height - r)), new Point(-r / 5, r), new Size(r, r), rnd.Next(4, 10)));
            }
            Logger.LogMessage($"New wave of asteroids. {e.oldCount + 1}");
        }

        /// <summary>
        /// Хранилище фоновых объектов
        /// </summary>
        public static BaseObject[] _bg_objects;
        public static List<AidKit> _aidKits = new List<AidKit>();
        private static List<Bullet> _bullets = new List<Bullet>();
        private static List<Asteroid> _asteroids = new List<Asteroid>();
        public static BufferedGraphics Buffer;
        private static Ship _ship = new Ship(new Point(10, 400), new Point(5, 5), new Size(10, 10));

        /// https://docs.microsoft.com/ru-ru/dotnet/csharp/delegates-patterns
        public static void LogToConsole(string message)
        {
            Console.WriteLine(message);
        }

        // Свойства
        /// <summary>
        /// Ширина игрового поля
        /// </summary>
        public static int Width { get; set; }
        /// <summary>
        /// Высота игрового поля
        /// </summary>
        public static int Height { get; set; }

        private static void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
                if (_bullets.Count <= amountOfBullets)
                {
                    _bullets.Add(new Bullet(new Point(_ship.Rect.X + 10, _ship.Rect.Y + 4), new Point(4, 0), new Size(4, 1)));
                }
            if (e.KeyCode == Keys.Up) _ship.Up();
            if (e.KeyCode == Keys.Down) _ship.Down();
        }

        public static void Init(Form form)
        {
            FileLogger fileOutput = new FileLogger("log.txt");
            Logger.WriteMessage += LogToConsole;
            if (form.Width < 0 || form.Width > 1000 || form.Height < 0 || form.Height > 1000)
            {
                throw new ArgumentOutOfRangeException();
            }
            // Графическое устройство для вывода графики            
            Graphics g;

            // Предоставляет доступ к главному буферу графического контекста для текущего приложения
            _context = BufferedGraphicsManager.Current;
            g = form.CreateGraphics();

            // Создаем объект (поверхность рисования) и связываем его с формой
            // Запоминаем размеры формы
            Width = form.ClientSize.Width;
            Height = form.ClientSize.Height;

            // Связываем буфер в памяти с графическим объектом, чтобы рисовать в буфере
            Buffer = _context.Allocate(g, new Rectangle(0, 0, Width, Height));
            onAsteroidsEmpty += RespawnAsteroids;
            Load();
            timer.Start();
            timer.Tick += Timer_Tick;
            rndTimer.Start();
            rndTimer.Tick += RndTimer_Tik;
            form.KeyDown += Form_KeyDown;
            Ship.MessageDie += Finish;
        }

        private static void RndTimer_Tik(object sender, System.EventArgs e)
        {
            if (_ship.Energy < 5 && _sip.Energy > 0)
            {
                rndTimer.Interval = rnd.Next(5, 15) * 1000;
                int r = rnd.Next(7, 10);
                _aidKits.Add(new AidKit(new Point(Width, rnd.Next(r, Game.Height - r)),
                                        new Point(-r, r),
                                        new Size(r, r)));
                Logger.LogMessage("Here a Med Kit comes! =)");
            }
        }

        private static void Timer_Tick(object sender, System.EventArgs e)
        {
            Draw();
            Update();
        }

        /// <summary>
        /// Отрисовка объектов на полотне
        /// </summary>
        public static void Draw()
        {
            Buffer.Graphics.Clear(Color.Black);

            foreach (BaseObject obj in _bg_objects) obj?.Draw();

            foreach (AidKit kit in _aidKits) kit.Draw();

            foreach (Asteroid a in _asteroids) a?.Draw();

            foreach (Bullet _bullet in _bullets) _bullet.Draw();

            _ship?.Draw();

            if (_ship != null)
                Buffer.Graphics.DrawString("Energy: " + _ship.Energy, SystemFonts.DefaultFont, Brushes.White, 0, 0);

            Buffer.Graphics.DrawString("XP: " + XP, SystemFonts.DefaultFont, Brushes.White, 0, 14);
            Buffer.Graphics.DrawString("Amount of bullets: " + (amountOfBullets - _bullets.Count), SystemFonts.DefaultFont, Brushes.White, 0, 28);

            Buffer.Render();
        }

        /// <summary>
        /// Обновление состояний объектов
        /// </summary>
        public static void Update()
        {
            foreach (BaseObject obj in _bg_objects) obj.Update();

            foreach (BaseObject obj in _aidKits) obj.Update();

            for (int b_idx = _bullets.Count - 1; b_idx >= 0; b_idx--)
            {
                _bullets[b_idx].Update();

                // Пуля ушла за край игрового поля
                if (_bullets[b_idx].Rect.X >= Game.Width)
                {
                    _bullets.Remove(_bullets[b_idx]);
                    b_idx--;
                }
            }

            for (int i = _aidKits.Count-1; i >= 0; i--)
            {
                if (_aidKits.Count > 0 && _ship.Collision(_aidKits[i])) _ship.EnergyLow(_aidKits[i].Power * -1);
            }

            for (var i = _asteroids.Count - 1; i >= 0; i--)
            {
                _asteroids[i].Update();

                for (int j = _bullets.Count - 1; j >= 0; j--)
                {
                    // Коллизия пули и астероида
                    if (_bullets[j].Collision(_asteroids[i]))
                    {
                        System.Media.SystemSounds.Hand.Play();
                        _asteroids[i].Power -= _bullets[j].Power;
                        if (_asteroids[i].Power <= 0)
                        {
                            _asteroids[i].DrawStatus = false;
                            XP++;
                        }
                        _bullets[j].DrawStatus = false;
                        continue;
                    }
                }

                // Коллизия корабля и астероида
                if (_asteroids.Count > 0 && !_ship.Collision(_asteroids[i])) continue;

                _ship?.EnergyLow(_asteroids[i].Power);
                _asteroids[i].Respawn(Width);

                System.Media.SystemSounds.Asterisk.Play();

                Logger.WriteMessage("Asteroid hit your ship!");
                if (_ship.Energy <= 0)
                {
                    Logger.WriteMessage("You'r dead!");
                    _ship?.Die();
                }


            }
            //Убираемся за собой
            for (int i = _asteroids.Count-1; i >= 0; i--)
                if (!_asteroids[i].DrawStatus)
                    _asteroids.Remove(_asteroids[i]);

            for (int i = _bullets.Count-1; i >= 0; i--)
                if (!_bullets[i].DrawStatus)
                    _bullets.Remove(_bullets[i]);

            for (int i = _aidKits.Count-1; i >= 0; i--)
                if (!_aidKits[i].DrawStatus)
                    _aidKits.Remove(_aidKits[i]);

            // Волна астероидов
            if (_asteroids.Count < 1)
                onAsteroidsEmpty(null, new AsteroidsEventArgs(asteroidsCount++));
        }

        public static void Load()
        {
            // Ёмкость хранилища под объекты
            int objectsAmount = 30;
            _bg_objects = new BaseObject[objectsAmount];
            var rnd = new Random();

            // Изображение планеты
            Image image = new Bitmap("planet.png");

            for (var i = 0; i < _bg_objects.Length; i++)
            {
                int r = rnd.Next(5, 50);
                _bg_objects[i] = new Star(new Point(1000, rnd.Next(0, Game.Height)), new Point(-r, r), new Size(3, 3));
            }

            onAsteroidsEmpty(null, new AsteroidsEventArgs(asteroidsCount));
        }

        public static void Finish()
        {
            timer.Stop();
            Buffer.Graphics.DrawString("The End", new Font(FontFamily.GenericSansSerif, 60, FontStyle.Underline), Brushes.White, 200, 100);
            Buffer.Render();
            Logger.WriteMessage("Game over.");
        }
    }
}