using System;
using System.Windows.Forms;
// Создаем шаблон приложения, где подключаем модули
namespace Asteroids
{
    class Program
    {
        static void Main(string[] args)
        {
            Form form = new Form
            {
                //Width = Screen.PrimaryScreen.Bounds.Width,
                //Height = Screen.PrimaryScreen.Bounds.Height
                Width = 800,
                Height = 600
            };
            Game.Init(form);
            form.Show();
            Game.Load();
            Game.Draw();
            Application.Run(form);
        }
    }
}