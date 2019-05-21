using System;
using System.Drawing;

namespace Asteroids
{
    interface ICollision
    {
        bool Collision(ICollision obj);
        Rectangle Rect { get; }
    }

    abstract class BaseObject : ICollision
    {
        public delegate void Message();
        /// <summary>
        /// Положение объекта
        /// </summary>
        protected Point Pos;
        /// <summary>
        /// Направление движения
        /// </summary>
        protected Point Dir;
        /// <summary>
        /// Размер объекта Height x Wight
        /// </summary>
        protected Size Size;

        /// <param name="pos">Позиция объекта. Экземпляр класса Point.</param>
        /// <param name="dir">Направление движения. Экземпляр класса Point.</param>
        /// <param name="size">Размер объекта. Экземпляр класса Size.</param>
        protected BaseObject(Point pos, Point dir, Size size)
        {
            Pos = pos;
            Dir = dir;
            Size = size;
        }
        public abstract void Draw();
        public abstract void Update();
        internal bool DrawStatus = true;
        public void Respawn(int x)
        {
            this.Pos.X = x;
        }
        public bool Collision(ICollision o) => o.Rect.IntersectsWith(this.Rect);
        public Rectangle Rect => new Rectangle(Pos, Size);
    }


    class Asteroid : BaseObject, ICloneable, IComparable<Asteroid>
    {
        /// <summary>
        /// Метод клонирования данных объекта.
        /// </summary>
        /// <returns>Возвращает новый экземпляр класса, с копией внутренних данных данного объекта.</returns>
        public object Clone()
        {
            //Копипаста из методички =) работаем с механизированными астероидами.
            // Создаем копию нашего робота
            Asteroid asteroid = new Asteroid(new Point(Pos.X, Pos.Y), new Point(Dir.X, Dir.Y), new Size(Size.Width, Size.Height));
            // Не забываем скопировать новому астероиду Power нашего астероида
            asteroid.Power = Power;
            return asteroid;
        }

        public int Power { get; set; }

        /// <summary>
        /// Объект игрового поля - астероид.
        /// </summary>
        /// <param name="pos">Позиция объекта. Экземпляр класса Point.</param>
        /// <param name="dir">Направление движения. Экземпляр класса Point.</param>
        /// <param name="size">Размер объекта. Экземпляр класса Size.</param>
        public Asteroid(Point pos, Point dir, Size size, int power = 3) : base(pos, dir, size)
        {
            Power = power;
        }

        /// <summary>
        /// Отрисовка объекта на полотне.
        /// </summary>
        public override void Draw()
        {
            if (DrawStatus)
            {
                Game.Buffer.Graphics.DrawString("x " + this.Power, SystemFonts.DefaultFont, Brushes.White, this.Pos.X + Size.Width, this.Pos.Y);
                Game.Buffer.Graphics.FillEllipse(Brushes.White, Pos.X, Pos.Y, Size.Width, Size.Height);
            }
        }

        /// <summary>
        /// Обновление координат объекта.
        /// </summary>
        public override void Update()
        {
            Pos.X += Dir.X;
            if (Pos.X < 0) Pos.X = Game.Width + Size.Width;
        }

        int IComparable<Asteroid>.CompareTo(Asteroid obj)
        {
            if (Power > obj.Power)
                return 1;
            if (Power < obj.Power)
                return -1;
            return 0;
        }
    }

    class Bullet : BaseObject
    {
        public readonly int Power;
        public Bullet(Point pos, Point dir, Size size, int power = 1) : base(pos, dir, size)
        {
            Power = power;
        }
        public override void Draw()
        {
            if (DrawStatus)
                Game.Buffer.Graphics.DrawRectangle(Pens.OrangeRed, Pos.X, Pos.Y, Size.Width, Size.Height);
        }

        public override void Update()
        {
            Pos.X += 7;
        }
    }

    class Planet : BaseObject
    {
        private Image img;
        public Planet(Point pos, Point dir, Image image) : base(pos, dir, image.Size)
        {
            img = image;
        }

        public override void Draw()
        {
            if (DrawStatus)
                Game.Buffer.Graphics.DrawImage(img, Pos.X, Pos.Y);
        }

        public override void Update()
        {
            Pos.X += Dir.X;
            if (Pos.X < 0) Pos.X = Game.Width + Size.Width;
        }
    }

    class Star : BaseObject
    {
        public Star(Point pos, Point dir, Size size) : base(pos, dir, size) { }
        public override void Draw()
        {
            if (DrawStatus)
            {
                Game.Buffer.Graphics.DrawLine(Pens.White, Pos.X, Pos.Y, Pos.X + Size.Width, Pos.Y + Size.Height);
                Game.Buffer.Graphics.DrawLine(Pens.White, Pos.X + Size.Width, Pos.Y, Pos.X, Pos.Y + Size.Height);
            }
        }

        public override void Update()
        {
            Pos.X += Dir.X;
            if (Pos.X < 0) Pos.X = Game.Width - Size.Width;
        }
    }

    class Ship : BaseObject
    {
        public static event Message MessageDie;
        private int _energy = 10;
        public int Energy => _energy;

        public void EnergyLow(int n)
        {
            _energy -= n;
        }
        public Ship(Point pos, Point dir, Size size) : base(pos, dir, size)
        {
        }
        public override void Draw()
        {
            if (DrawStatus)
                Game.Buffer.Graphics.FillEllipse(Brushes.Wheat, Pos.X, Pos.Y, Size.Width, Size.Height);
        }
        public override void Update()
        {
        }
        public void Up()
        {
            if (Pos.Y > 0) Pos.Y = Pos.Y - Dir.Y;
        }
        public void Down()
        {
            if (Pos.Y < Game.Height) Pos.Y = Pos.Y + Dir.Y;
        }
        public void Die()
        {
            MessageDie?.Invoke();
            Logger.WriteMessage("Depressurization! You lost your ship.");
        }
    }

    class AidKit: BaseObject
    {
        internal int Power;
        public AidKit(Point pos, Point dir, Size size, int power = 1): base(pos, dir, size)
        {
            Power = power;
        }

        public override void Draw()
        {
            if (DrawStatus)
            {
                Game.Buffer.Graphics.DrawString("x " + this.Power, SystemFonts.DefaultFont, Brushes.White, Pos.X + Size.Width, Pos.Y);
                Game.Buffer.Graphics.FillEllipse(Brushes.Red, Pos.X, Pos.Y, Size.Width, Size.Height);
            }
        }

        public override void Update()
        {
            Pos.X += Dir.X;
            if (Pos.X < 0)
                DrawStatus = false;
        }
    }
}
