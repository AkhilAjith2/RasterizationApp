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
    public class CustomLine
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public int Thickness { get; set; }
        public SolidColorBrush Color { get; set; }

        public CustomLine(Point startPoint, Point endPoint, int thickness, SolidColorBrush color)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Thickness = thickness;
            Color = color;
        }

        public CustomLine()
        {
            StartPoint = new Point();
            EndPoint = new Point();
            Thickness = new int();
            Color = new SolidColorBrush();
        }
    }

    public partial class MainWindow : Window
    {
        private double CalculateDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        /*private void DrawLine(Point p1, Point p2, int thickness = 1, SolidColorBrush color = null)
        {
            int x0 = (int)p1.X;
            int y0 = (int)p1.Y;
            int x1 = (int)p2.X;
            int y1 = (int)p2.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx - dy;

            bool isHorizontal = dx > dy;

            while (true)
            {
                DrawLineWithThickness(x0, y0, color, thickness, isHorizontal);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }a
        }*/

        private void DrawLine(Point p1, Point p2, int thickness = 1, SolidColorBrush color = null)
        {
            int x1 = (int)p1.X;
            int y1 = (int)p1.Y;
            int x2 = (int)p2.X;
            int y2 = (int)p2.Y;

            bool isHorizontal = Math.Abs(x2 - x1) > Math.Abs(y2 - y1);

            if (!isHorizontal)
            {
                int temp = x1;
                x1 = y1;
                y1 = temp;
                temp = x2;
                x2 = y2;
                y2 = temp;
            }

            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int d = 2 * dy - dx;
            int dE = 2 * dy;
            int dNE = 2 * (dy - dx);
            int xf = x1, yf = y1;
            int xb = x2, yb = y2;

            if (!isHorizontal)
            {
                DrawLineWithThickness(yf, xf, color, thickness, isHorizontal); 
                DrawLineWithThickness(yb, xb, color, thickness, isHorizontal);
            }
            else
            {
                DrawLineWithThickness(xf, yf, color, thickness, isHorizontal);
                DrawLineWithThickness(xb, yb, color, thickness, isHorizontal);
            }

            if (x1 < x2)
            {
                if (y1 <= y2) // Slope is positive or zero (slanting upwards or horizontal)
                {
                    while (xf < xb)
                    {
                        ++xf;
                        --xb;
                        if (d < 0)
                            d += dE;
                        else
                        {
                            d += dNE;
                            ++yf;
                            --yb;
                        }
                        if (!isHorizontal)
                        {
                            DrawLineWithThickness(yf, xf, color, thickness, isHorizontal); 
                            DrawLineWithThickness(yb, xb, color, thickness, isHorizontal);
                        }
                        else
                        {
                            DrawLineWithThickness(xf, yf, color, thickness, isHorizontal);
                            DrawLineWithThickness(xb, yb, color, thickness, isHorizontal);
                        }
                    }
                }
                else // Slope is negative (slanting downwards)
                {
                    while (xf < xb)
                    {
                        ++xf;
                        --xb;
                        if (d < 0)
                            d += dE;
                        else
                        {
                            d += dNE;
                            --yf;
                            ++yb;
                        }
                        if (!isHorizontal)
                        {
                            DrawLineWithThickness(yf, xf, color, thickness, isHorizontal); 
                            DrawLineWithThickness(yb, xb, color, thickness, isHorizontal);
                        }
                        else
                        {
                            DrawLineWithThickness(xf, yf, color, thickness, isHorizontal);
                            DrawLineWithThickness(xb, yb, color, thickness, isHorizontal);
                        }
                    }
                }
            }
            else // x1 >= x2
            {
                if (y1 <= y2) // Slope is negative (slanting downwards)
                {
                    while (xf > xb)
                    {
                        --xf;
                        ++xb;
                        if (d < 0)
                            d += dE;
                        else
                        {
                            d += dNE;
                            ++yf;
                            --yb;
                        }
                        if (!isHorizontal)
                        {
                            DrawLineWithThickness(yf, xf, color, thickness, isHorizontal); 
                            DrawLineWithThickness(yb, xb, color, thickness, isHorizontal);
                        }
                        else
                        {
                            DrawLineWithThickness(xf, yf, color, thickness, isHorizontal);
                            DrawLineWithThickness(xb, yb, color, thickness, isHorizontal);
                        }
                    }
                }
                else // Slope is positive or zero (slanting upwards or horizontal)
                {
                    while (xf > xb)
                    {
                        --xf;
                        ++xb;
                        if (d < 0)
                            d += dE;
                        else
                        {
                            d += dNE;
                            --yf;
                            ++yb;
                        }
                        if (!isHorizontal)
                        {
                            DrawLineWithThickness(yf, xf, color, thickness, isHorizontal); 
                            DrawLineWithThickness(yb, xb, color, thickness, isHorizontal);
                        }
                        else
                        {
                            DrawLineWithThickness(xf, yf, color, thickness, isHorizontal);
                            DrawLineWithThickness(xb, yb, color, thickness, isHorizontal);
                        }
                    }
                }
            }
        }

        private void DrawLineWithThickness(int x, int y, SolidColorBrush color, int thickness, bool isHorizontal)
        {
            PutPixel(x, y, color);

            if (thickness > 1)
            {
                if (isHorizontal == true)
                {
                    for (int i = 1; i <= thickness / 2; i++)
                    {
                        PutPixel(x, y + i, color);
                        PutPixel(x, y - i, color);
                    }
                }
                else
                {
                    for (int j = 1; j <= thickness / 2; j++)
                    {
                        PutPixel(x + j, y, color);
                        PutPixel(x - j, y, color);
                    }
                }
            }
        }

        public void PutPixel(int x, int y, SolidColorBrush color)
        {
            Rectangle pixel = new Rectangle
            {
                Width = 1,
                Height = 1,
                Fill = color,
                Margin = new Thickness(x, y, 0, 0)
            };
            DrawingCanvas.Children.Add(pixel);
        }

        /*private void PutPixel(int x, int y, SolidColorBrush color)
        {
            int stride = writableBitmap.PixelWidth * (writableBitmap.Format.BitsPerPixel / 8);
            int bytesPerPixel = writableBitmap.Format.BitsPerPixel / 8;
            byte[] colorData = { ((System.Windows.Media.Color)color.Color).B, ((System.Windows.Media.Color)color.Color).G, ((System.Windows.Media.Color)color.Color).R, ((System.Windows.Media.Color)color.Color).A };

            writableBitmap.WritePixels(new Int32Rect(x, y, 1, 1), colorData, stride, 0);
        }*/

        /*private void PutPixel(int x, int y, SolidColorBrush color)
        {
            // Lock the writable bitmap to write pixel data
            writableBitmap.Lock();

            // Calculate the stride of the bitmap (number of bytes per pixel row)
            int stride = writableBitmap.PixelWidth * 4;

            // Get a pointer to the back buffer of the writable bitmap
            IntPtr backBuffer = writableBitmap.BackBuffer;

            // Calculate the address of the pixel to draw
            backBuffer += y * stride + x * 4;

            // Convert the WPF color to a System.Drawing.Color
            System.Drawing.Color drawingColor = System.Drawing.Color.FromArgb(color.Color.A, color.Color.R,
                                                                             color.Color.G, color.Color.B);

            // Set the color of the pixel in the back buffer
            System.Runtime.InteropServices.Marshal.Copy(new byte[] { drawingColor.B, drawingColor.G,
                                                               drawingColor.R, drawingColor.A }, 0,
                                                        backBuffer, 4);

            // Unlock the writable bitmap to update the changes
            writableBitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            writableBitmap.Unlock();
        }*/

        private bool ClosestLineToSelect(Point clickedPoint, Point start, Point end)
        {
            return Math.Abs((end.Y - start.Y) * clickedPoint.X - (end.X - start.X) * clickedPoint.Y + end.X * start.Y - end.Y * start.X) / Math.Sqrt(Math.Pow(end.Y - start.Y, 2) + Math.Pow(end.X - start.X, 2)) < 5;
        }

        private void DrawLine_Click(object sender, RoutedEventArgs e)
        {
            isDrawingLine = true;
            isDrawingCircle = false;
            isDrawingPolygon = false;
            isDrawingCapsule = false;
            
            LineButton.IsChecked = true;
            CircleButton.IsChecked = false;
            PolygonButton.IsChecked = false;
            CapsuleButton.IsChecked = false;
        }

        private int PromptForThickness()
        {
            ThicknessWindow dialog = new ThicknessWindow();
            if (dialog.ShowDialog() == true)
            {
                int thickness;
                if (int.TryParse(dialog.ThicknessValue, out thickness))
                {
                    return thickness;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Invalid thickness value. Please enter a valid integer value.");
                    return -1;
                }
            }
            else
            {
                return -1;
            }
        }
    }
}
