using System;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    // Класс, описывающий аргументы событий
    public class CControllerEventArgs : EventArgs
    {
        // Ссылка на визуальное представление объекта
        public Ellipse Sprite { get; }

        // Сообщение, связанное с действием
        public string Message { get; set; } = "";

        // Дополнительные данные для игры
        public object Data { get; set; }

        public CControllerEventArgs(Ellipse sprite)
        {
            Sprite = sprite;
        }

        public CControllerEventArgs(Ellipse sprite, string message)
        {
            Sprite = sprite;
            Message = message;
        }
    }
}
