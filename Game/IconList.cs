using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Game
{
    public class IconList
    {
        private List<Icon> icons;              
        private int border;                   
        private int x;                         
        private int y;                         
        private int x_sh;                      
        private int y_sh;                      
        private int imageWidth;                
        private int imageHeight;             
        private int canvasW;                  
        private int canvasH;                   

        public IconList(int iconWidth, int iconHeight, int canvasWidth, int canvasHeight)
        {
            icons = new List<Icon>();
            border = 8;
            x = 0;
            y = 0;
            x_sh = iconWidth + border;
            y_sh = iconHeight + border;
            imageWidth = iconWidth;
            imageHeight = iconHeight;
            canvasW = canvasWidth;
            canvasH = canvasHeight;
        }

        public void Load(string relativePath)
        {
            string baseDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string fullPath = Path.Combine(baseDir, relativePath);

            if (!Directory.Exists(fullPath))
                throw new DirectoryNotFoundException($"Папка не найдена: {fullPath}");

            string[] files = Directory.GetFiles(fullPath, "*.png");
            icons.Clear();

            int col = 0;
            int row = 0;

            foreach (string file in files)
            {
                Icon icon = new Icon(imageWidth, imageHeight, file);

                double posX = x + (col * x_sh);
                double posY = y + (row * y_sh);
                icon.SetPosition(new Point(posX, posY));
                icons.Add(icon);

                col++;
                if (posX + imageWidth * 2 > canvasW)  
                {
                    col = 0;
                    row++;
                }
            }
        }

        public int GetDeltaY() => y;

        public void Scroll(double delta)
        {
            y += (int)delta;
            foreach (var icon in icons)
            {
                Point newPos = new Point(icon.X(), icon.Y() + delta);
                icon.SetPosition(newPos);
            }
        }

        public List<Icon> GetIcons() => icons;

        public Icon FindByName(string name)
        {
            foreach (var icon in icons)
                if (icon.GetName() == name)
                    return icon;
            return null;
        }

        public Icon IsMouseOver(Point mousePosition)
        {
            foreach (var icon in icons)
                if (icon.IsMouseOver(mousePosition))
                    return icon;
            return null;
        }
    }
}