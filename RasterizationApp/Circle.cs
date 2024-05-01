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

namespace RasterizationApp
{
    public class CustomCircle
    {
        public Point Center { get; set; }
        public double Radius { get; set; }
        public SolidColorBrush Color { get; set; }

        public CustomCircle(Point center, double radius, SolidColorBrush color)
        {
            Center = center;
            Radius = radius;
            Color = color;
        }
        public CustomCircle()
        {
            Center = new Point();
            Radius = new int();
            Color = new SolidColorBrush();
        }
    }
    public partial class MainWindow : Window
    {
        private void DrawCircle(Point center, double radius, SolidColorBrush color = null)
        {
            int dE = 3;
            int dSE = 5 - 2 * (int)radius;
            int d = 1 - (int)radius;
            int x = 0;
            int y = (int)radius;

            PutCirclePixels(center, x, y, color);

            while (y > x)
            {
                if (d < 0) // move to E
                {
                    d += dE;
                    dE += 2;
                    dSE += 2;
                }
                else // move to SE
                {
                    d += dSE;
                    dE += 2;
                    dSE += 4;
                    --y;
                }
                ++x;
                PutCirclePixels(center, x, y, color);
            }
            if (selectedCircle != null && center == selectedCircle.Center)
            {
                DrawCircleBorder(center, radius, Brushes.Yellow); 
            }
            /*FillCircle(center, (int)radius, color);*/
        }
        private void DrawCircleBorder(Point center, double radius, SolidColorBrush color)
        {
            Ellipse borderCircle = new Ellipse
            {
                Width = 2 * radius + 5, 
                Height = 2 * radius + 5,
                Stroke = color,
                StrokeThickness = 4, 
                Margin = new Thickness(center.X - radius - 2.5, center.Y - radius - 2.5, 0, 0) 
            };
            DrawingCanvas.Children.Add(borderCircle);
        }

        private void FillCircle(Point center, int radius, SolidColorBrush color)
        {
            Ellipse circle = new Ellipse
            {
                Width = 2 * radius,
                Height = 2 * radius,
                Fill = color,
                Margin = new Thickness(center.X - radius, center.Y - radius, 0, 0)
            };
            DrawingCanvas.Children.Add(circle);
        }

        /*private void FillCircle(Point center, int radius, SolidColorBrush fillColor)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        PutPixel(new Point(center.X + x, center.Y + y), fillColor);
                    }
                }
            }
        }*/

        private void PutCirclePixels(Point center, int x, int y, SolidColorBrush color = null)
        {
            PutPixel(new Point((int)(center.X + x), (int)(center.Y + y)), color);
            PutPixel(new Point((int)(center.X - x), (int)(center.Y + y)), color);
            PutPixel(new Point((int)(center.X + x), (int)(center.Y - y)), color);
            PutPixel(new Point((int)(center.X - x), (int)(center.Y - y)), color);
            PutPixel(new Point((int)(center.X + y), (int)(center.Y + x)), color);
            PutPixel(new Point((int)(center.X - y), (int)(center.Y + x)), color);
            PutPixel(new Point((int)(center.X + y), (int)(center.Y - x)), color);
            PutPixel(new Point((int)(center.X - y), (int)(center.Y - x)), color);
        }

        private void PutPixel(Point point, SolidColorBrush color)
        {
            Rectangle pixel = new Rectangle
            {
                Width = 1,
                Height = 1,
                Fill = color,
                Margin = new Thickness(point.X, point.Y, 0, 0) 
            };
            DrawingCanvas.Children.Add(pixel);
        }

        private bool IsPointInsideCircle(Point point, Point center, double radius)
        {
            CustomCircle smallestCircle = GetSmallestCircleContainingPoint(point);
            return smallestCircle != null && smallestCircle.Center == center && Math.Abs(smallestCircle.Radius - radius) < 0.01; 
        }

        private CustomCircle GetSmallestCircleContainingPoint(Point point)
        {
            List<CustomCircle> circlesContainingPoint = new List<CustomCircle>();

            foreach (var circle in circles)
            {
                double distanceSquared = Math.Pow(point.X - circle.Center.X, 2) + Math.Pow(point.Y - circle.Center.Y, 2);
                double circleRadius = circle.Radius;
                if (distanceSquared <= Math.Pow(circleRadius, 2))
                {
                    circlesContainingPoint.Add(circle);
                }
            }

            if (circlesContainingPoint.Count > 0)
            {
                CustomCircle smallestCircle = circlesContainingPoint[0];
                foreach (var circle in circlesContainingPoint)
                {
                    if (circle.Radius < smallestCircle.Radius)
                    {
                        smallestCircle = circle;
                    }
                }
                return smallestCircle;
            }

            return null;
        }



        private void DrawCircle_Click(object sender, RoutedEventArgs e)
        {
            isDrawingLine = false;
            isDrawingCircle = true;
            isDrawingPolygon = false;
            isDrawingCapsule = false;

            LineButton.IsChecked = false;
            CircleButton.IsChecked = true;
            PolygonButton.IsChecked = false;
            CapsuleButton.IsChecked = false;
        }
    }
}
