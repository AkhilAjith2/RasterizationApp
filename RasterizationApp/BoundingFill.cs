/*using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Rectangle = System.Windows.Shapes.Rectangle;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using System.Net;
using Color = System.Drawing.Color;

namespace RasterizationApp
{
    public partial class MainWindow : Window
    {
        public SolidColorBrush fillColor;
        public SolidColorBrush boundaryColor;
        public bool boundaryMode = false;
        public void BoundaryFill(int x, int y, SolidColorBrush fillColor, SolidColorBrush boundaryColor)
        {
            int width = writableBitmap.PixelWidth;
            int height = writableBitmap.PixelHeight;
            bool[,] visited = new bool[width, height];

            Stack<Point> stack = new Stack<Point>();
            stack.Push(new Point(x, y));

            while (stack.Count > 0)
            {
                Point p = stack.Pop();
                int px = (int)p.X;
                int py = (int)p.Y;

                if (px < 0 || px >= width || py < 0 || py >= height)
                    continue;

                Color color = GetPixelColorFromBitmap(writableBitmap, px, py);
                SolidColorBrush currentColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                System.Windows.Media.Color currentBrushColor = currentColor.Color;
                System.Windows.Media.Color boundaryBrushColor = boundaryColor.Color;
                System.Windows.Media.Color fillBrushColor = fillColor.Color;

                if ((currentBrushColor.R != boundaryBrushColor.R || currentBrushColor.G != boundaryBrushColor.G || currentBrushColor.B != boundaryBrushColor.B) &&
    (currentBrushColor.R != fillBrushColor.R || currentBrushColor.G != fillBrushColor.G || currentBrushColor.B != fillBrushColor.B) &&
    !visited[px, py])
                {
                    System.Windows.Media.Color mediaColor = fillColor.Color;
                    Color drawingColor = Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
                    SetPixelColor(py, px, drawingColor);
                    visited[px, py] = true;

                    stack.Push(new Point(px + 1, py));
                    stack.Push(new Point(px - 1, py));
                    stack.Push(new Point(px, py + 1));
                    stack.Push(new Point(px, py - 1));
                }
            }
        }
    }
}
*/