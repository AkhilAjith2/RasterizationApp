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
    public class Polygon
    {
        public List<Point> Vertices { get; set; }
        public int Thickness { get; set; }
        public SolidColorBrush Color { get; set; }
        public bool isFilled { get; set; }
        public bool IsImageFilled { get; set; }
        public string FillImagePath { get; set; }
        public WriteableBitmap FillImage { get; set; }

        public Polygon(List<Point> vertices, int thickness, SolidColorBrush color, bool isFilled = false, bool IsImageFilled = false, string FillImagePath = null, WriteableBitmap FillImage = null)
        {
            Vertices = vertices;
            Thickness = thickness;
            Color = color;
            isFilled = false;
        }
        public Polygon()
        {
            Vertices = new List<Point>();
            Thickness = new int();
            Color = new SolidColorBrush();
            isFilled = false;
        }
    }
    public partial class MainWindow : Window
    {
        private bool IsCloseToFirstVertex(Point clickedPoint)
        {
            if (currentPolygonVertices.Count < 2)
            {
                return false;
            }
            double distance = CalculateDistance(clickedPoint, currentPolygonVertices[0]);
            return distance < 10; 
        }

        private double CalculateDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        private void DrawPolygon(Polygon polygon, SolidColorBrush color)
        {
            if (polygon.Vertices.Count < 2)
            {
                return;
            }

            for (int i = 0; i < polygon.Vertices.Count - 1; i++)
            {
                if (antialiasingEnabled)
                {
                    ThickAntialiasedLine((int)polygon.Vertices[i].X, (int)polygon.Vertices[i].Y, (int)polygon.Vertices[i + 1].X, (int)polygon.Vertices[i + 1].Y, polygon.Thickness, color);
                }
                else
                {
                    DrawLine(polygon.Vertices[i], polygon.Vertices[i + 1], polygon.Thickness, color);
                }
            }
            if (antialiasingEnabled)
            {
                ThickAntialiasedLine((int)polygon.Vertices[polygon.Vertices.Count - 1].X, (int)polygon.Vertices[polygon.Vertices.Count - 1].Y, (int)polygon.Vertices[0].X, (int)polygon.Vertices[0].Y, polygon.Thickness, color);
            }
            else
            {
                DrawLine(polygon.Vertices[polygon.Vertices.Count - 1], polygon.Vertices[0], polygon.Thickness, color);
            }

            if(clippingRectangle!=null)
            {
                DrawClippingLine(polygon, clippingRectangle, 3, Brushes.Blue);
            }
        }

        private void DrawClippingLine(Polygon polygon, MyRectangle clip, int thickness, SolidColorBrush highlightColor)
        {
            List<Point> clippedPolygon = ClipPolygon(polygon);

            // Ensure we draw each segment properly
            for (int i = 0; i < clippedPolygon.Count; i += 2)
            {
                Point p1 = clippedPolygon[i];
                Point p2 = clippedPolygon[i + 1];
                DrawLine(p1, p2, thickness, highlightColor);
            }
        }

        private void DrawPolygon_Click(object sender, RoutedEventArgs e)
        {
            isDrawingLine = false;
            isDrawingCircle = false;
            isDrawingPolygon = true;
            isDrawingCapsule = false;
            isDrawingRectangle = false;

            LineButton.IsChecked = false;
            CircleButton.IsChecked = false;
            PolygonButton.IsChecked = true;
            RectangleButton.IsChecked = false;
        }

        private bool IsPointInsidePolygon(Point point, List<Point> polygon)
        {
            int polygonSides = polygon.Count;
            bool isInside = false;

            for (int i = 0, j = polygonSides - 1; i < polygonSides; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
            }

            return isInside;
        }
    }
}
