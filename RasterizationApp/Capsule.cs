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
using System.Net;

namespace RasterizationApp
{
    public class Capsule
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public double Radius { get; set; }

        public Capsule(Point startPoint, Point endPoint, double radius)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Radius = radius;
        }
    }
    public partial class MainWindow : Window
    {
        public void DrawCapsule(Capsule capsule)
        {
            double angle = Math.Atan2(capsule.EndPoint.Y - capsule.StartPoint.Y, capsule.EndPoint.X - capsule.StartPoint.X);

            double offsetX = capsule.Radius * Math.Cos(angle + Math.PI / 2);
            double offsetY = capsule.Radius * Math.Sin(angle + Math.PI / 2);

            Point startLine1 = new Point(capsule.StartPoint.X + offsetX, capsule.StartPoint.Y + offsetY);
            Point endLine1 = new Point(capsule.EndPoint.X + offsetX, capsule.EndPoint.Y + offsetY);

            Point startLine2 = new Point(capsule.StartPoint.X - offsetX, capsule.StartPoint.Y - offsetY);
            Point endLine2 = new Point(capsule.EndPoint.X - offsetX, capsule.EndPoint.Y - offsetY);

            DrawLine(startLine1, endLine1, 1, Brushes.Black);
            DrawLine(startLine2, endLine2, 1, Brushes.Black);

            DrawArc(capsule.StartPoint, startLine1, endLine1, capsule.Radius, Brushes.Black);
            DrawArc(capsule.EndPoint,startLine2, endLine2, capsule.Radius, Brushes.Black); 
        }

        public void DrawArc(Point center, Point start, Point end, double radius, SolidColorBrush color = null)
        {
            DrawArc(center.X, center.Y, radius, start, end, color);
        }

        public void DrawArc(double centerX, double centerY, double radius, Point start, Point end, SolidColorBrush color = null)
        {
            double angle = Math.Atan2(end.Y - start.Y, end.X - start.X);

            double startAngle = angle - Math.PI / 2; 
            double endAngle = angle + Math.PI / 2;   

            double step = Math.PI / 180; 

            for (double theta = startAngle; theta <= endAngle; theta += step)
            {
                int x = (int)(centerX + radius * Math.Cos(theta));
                int y = (int)(centerY + radius * Math.Sin(theta));

                PutPixel(x, y, color);
            }
        }

        /*private void DrawCapsule_Click(object sender, RoutedEventArgs e)
        {
            isDrawingLine = false;
            isDrawingCircle = false;
            isDrawingPolygon = false;
            isDrawingCapsule = true;

            LineButton.IsChecked = false;
            CircleButton.IsChecked = false;
            PolygonButton.IsChecked = false;
            CapsuleButton.IsChecked = true;
        }*/
    }

}
