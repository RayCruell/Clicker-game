using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface ISaveList<T>
    {
        // Сигнатура загрузки
        T Load(string path);
        // Сигнатура сохранения
        void Save(T data, string path);
    }
}
