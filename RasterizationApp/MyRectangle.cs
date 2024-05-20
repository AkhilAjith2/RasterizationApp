using System;
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
using static System.Windows.Forms.LinkLabel;

namespace RasterizationApp
{
    public class MyRectangle
    {
        public List<Point> Vertices { get; set; }
        public int Thickness { get; set; }
        public SolidColorBrush Color { get; set; }

        public MyRectangle(List<Point> vertices, int thickness, SolidColorBrush color)
        {
            Vertices = vertices;
            Thickness = thickness;
            Color = color;
        }

        public MyRectangle()
        {
            Vertices = new List<Point>();
            Thickness = new int();
            Color = new SolidColorBrush();
        }
    }
    public partial class MainWindow : Window
    {
        private void DrawRectangle(MyRectangle rectangle, SolidColorBrush color = null)
        {
            // Draw lines between the vertices to form the rectangle
            for (int i = 0; i < rectangle.Vertices.Count; i++)
            {
                int nextIndex = (i + 1) % rectangle.Vertices.Count; // Index of the next vertex (wraps around)
                DrawLine(rectangle.Vertices[i], rectangle.Vertices[nextIndex], rectangle.Thickness, color ?? rectangle.Color);
            }
        }

        private void DrawRectangle_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("DrawRectangle clicked and set to true");
            isDrawingLine = false;
            isDrawingCircle = false;
            isDrawingPolygon = false;
            isDrawingCapsule = false;
            isDrawingRectangle = true;

            LineButton.IsChecked = false;
            CircleButton.IsChecked = false;
            PolygonButton.IsChecked = false;
            RectangleButton.IsChecked = true;
        }

        private bool IsPointInsideRectangle(Point point, MyRectangle rectangle)
        {
            double minX = rectangle.Vertices[0].X;
            double maxX = rectangle.Vertices[0].X;
            double minY = rectangle.Vertices[0].Y;
            double maxY = rectangle.Vertices[0].Y;

            // Find the minimum and maximum coordinates of the rectangle
            for (int i = 1; i < rectangle.Vertices.Count; i++)
            {
                minX = Math.Min(minX, rectangle.Vertices[i].X);
                maxX = Math.Max(maxX, rectangle.Vertices[i].X);
                minY = Math.Min(minY, rectangle.Vertices[i].Y);
                maxY = Math.Max(maxY, rectangle.Vertices[i].Y);
            }

            // Check if the point lies within the bounding box of the rectangle
            return (point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY);
        }
    }
}
