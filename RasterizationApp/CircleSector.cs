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
namespace RasterizationApp
{
    public class CircleSector
    {
        public Point Center { get; set; }
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public CircleSector(Point center, Point startPoint, Point endPoint)
        {
            Center = center;
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
    }

    public partial class MainWindow : Window
    {
        private void DrawCircleSector(CircleSector circleSector)
        {
            // Calculate the radius of the circle using the distance between points StartPoint and Center
            double radius = Math.Sqrt(Math.Pow(circleSector.StartPoint.X - circleSector.Center.X, 2) +
                                       Math.Pow(circleSector.StartPoint.Y - circleSector.Center.Y, 2));

            // Calculate the angle of the sector using the dot product or trigonometric functions
            double angle = CalculateAngle(circleSector.Center, circleSector.StartPoint, circleSector.EndPoint);

            // Convert the angle to radians
            double startAngle = Math.Atan2(circleSector.StartPoint.Y - circleSector.Center.Y, circleSector.StartPoint.X - circleSector.Center.X);
            double endAngle = Math.Atan2(circleSector.EndPoint.Y - circleSector.Center.Y, circleSector.EndPoint.X - circleSector.Center.X);

            // Ensure start and end angles are within [0, 2π)
            startAngle = startAngle < 0 ? startAngle + 2 * Math.PI : startAngle;
            endAngle = endAngle < 0 ? endAngle + 2 * Math.PI : endAngle;

*//*            Swap start and end angles if startAngle is greater than endAngle
*//*            if (startAngle > endAngle)
            {
                double temp = startAngle;
                startAngle = endAngle;
                endAngle = temp;
            }
 
            // Iterate over the bounding rectangle of the circle
            int startX = (int)(circleSector.Center.X - radius);
            int startY = (int)(circleSector.Center.Y - radius);
            int endX = (int)(circleSector.Center.X + radius);
            int endY = (int)(circleSector.Center.Y + radius);

            // Iterate over the pixels within the bounding rectangle of the circle
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if (IsWithinCircle(x, y, circleSector.Center, radius))
                    {
                        // Calculate the angle from the center of the circle to the current pixel
                        double currentAngle = Math.Atan2(y - circleSector.Center.Y, x - circleSector.Center.X);
                        currentAngle = currentAngle < 0 ? currentAngle + 2 * Math.PI : currentAngle;

                        // Adjust angles based on quadrant
                        if (circleSector.StartPoint.X >= circleSector.Center.X && circleSector.StartPoint.Y >= circleSector.Center.Y)
                        {
                            // First quadrant
                            if (currentAngle >= startAngle && currentAngle <= endAngle)
                            {
                                DrawPixel(x, y);
                            }
                        }
                        else if (circleSector.StartPoint.X < circleSector.Center.X && circleSector.StartPoint.Y >= circleSector.Center.Y)
                        {
                            // Second quadrant
                            if (currentAngle >= startAngle && currentAngle <= endAngle)
                            {
                                DrawPixel(x, y);
                            }
                        }
                        else if (circleSector.StartPoint.X < circleSector.Center.X && circleSector.StartPoint.Y < circleSector.Center.Y)
                        {
                            // Third quadrant
                            if (currentAngle <= startAngle || currentAngle >= endAngle)
                            {
                                DrawPixel(x, y);
                            }
                        }
                        else if (circleSector.StartPoint.X >= circleSector.Center.X && circleSector.StartPoint.Y < circleSector.Center.Y)
                        {
                            // Fourth quadrant
                            if (currentAngle <= startAngle || currentAngle >= endAngle)
                            {
                                DrawPixel(x, y);
                            }
                        }
                    }
                }
            }
        }

        *//*private void DrawCircleSector(CircleSector circleSector)
        {
            // Calculate the radius of the circle using the distance between points StartPoint and Center
            double radius = Math.Sqrt(Math.Pow(circleSector.StartPoint.X - circleSector.Center.X, 2) +
                                      Math.Pow(circleSector.StartPoint.Y - circleSector.Center.Y, 2));

            // Determine the direction of the arc based on the positions of the start and end points
            bool clockwise = !(circleSector.StartPoint.X < circleSector.Center.X ^ circleSector.EndPoint.Y < circleSector.Center.Y);

            // Calculate the angle between the start and end points
            double startAngle = Math.Atan2(circleSector.StartPoint.Y - circleSector.Center.Y,
                                           circleSector.StartPoint.X - circleSector.Center.X);
            double endAngle = Math.Atan2(circleSector.EndPoint.Y - circleSector.Center.Y,
                                         circleSector.EndPoint.X - circleSector.Center.X);

            // Ensure start angle is smaller than end angle
            if (clockwise ? startAngle > endAngle : startAngle < endAngle)
            {
                double temp = startAngle;
                startAngle = endAngle;
                endAngle = temp;
            }

            // Draw the arc of the circle sector
            double angleStep = 0.01;
            for (double angle = startAngle; clockwise ? angle <= endAngle : angle >= endAngle; angle += (clockwise ? angleStep : -angleStep))
            {
                int x = (int)(circleSector.Center.X + radius * Math.Cos(angle));
                int y = (int)(circleSector.Center.Y + radius * Math.Sin(angle));
                DrawPixel(x, y);
            }

            // Calculate the endpoints of the lines where they intersect with the circle
            int startX = (int)(circleSector.Center.X + radius * Math.Cos(startAngle));
            int startY = (int)(circleSector.Center.Y + radius * Math.Sin(startAngle));
            int endX = (int)(circleSector.Center.X + radius * Math.Cos(endAngle));
            int endY = (int)(circleSector.Center.Y + radius * Math.Sin(endAngle));

            // Draw lines connecting the center to the start and end points
            DrawLine(circleSector.Center, new Point(startX, startY), 1, Brushes.Black);
            DrawLine(circleSector.Center, new Point(endX, endY), 1, Brushes.Black);
        }*//*

        private double CalculateAngle(Point center, Point startPoint, Point endPoint)
        {
            // Calculate vectors AB and AC
            double ABx = startPoint.X - center.X;
            double ABy = startPoint.Y - center.Y;
            double ACx = endPoint.X - center.X;
            double ACy = endPoint.Y - center.Y;

            // Calculate the dot product of AB and AC
            double dotProduct = ABx * ACx + ABy * ACy;

            // Calculate the magnitudes of vectors AB and AC
            double magnitudeAB = Math.Sqrt(ABx * ABx + ABy * ABy);
            double magnitudeAC = Math.Sqrt(ACx * ACx + ACy * ACy);

            // Calculate the angle between vectors AB and AC using the dot product formula
            double angle = Math.Acos(dotProduct / (magnitudeAB * magnitudeAC));

            // Convert the angle to degrees
            angle = angle * (180 / Math.PI);

            return angle;
        }

        // Function to check if a pixel is within the sector angle
        private bool IsWithinSector(int x, int y, Point center, double angle)
        {
            double dx = x - center.X;
            double dy = y - center.Y;

            double pixelAngle = Math.Atan2(dy, dx) * (180 / Math.PI);

            // Ensure the angle is within the range of the sector angle
            return pixelAngle >= 0 && pixelAngle <= angle;
        }

        private void DrawPixel(int x, int y)
        {
            Rectangle pixel = new Rectangle
            {
                Width = 1,
                Height = 1,
                Fill = Brushes.Black,
                Margin = new Thickness(x, y, 0, 0)
            };
            DrawingCanvas.Children.Add(pixel);
        }

        private bool IsWithinCircle(int x, int y, Point center, double radius)
        {
            return Math.Pow(x - center.X, 2) + Math.Pow(y - center.Y, 2) <= Math.Pow(radius, 2);
        }

        private void DrawCircleSector_Click(object sender, RoutedEventArgs e)
        {
            isDrawingLine = false;
            isDrawingCircle = false;
            isDrawingPolygon = false;
            isDrawingCircleSector = true;
            // Toggle the LineButton
            LineButton.IsChecked = false;
            CircleButton.IsChecked = false;
            PolygonButton.IsChecked = false;
            CircleSectorButton.IsChecked = true;
        }

    }
}
*/