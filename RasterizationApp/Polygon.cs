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
    public class Polygon
    {
        public List<Point> Vertices { get; set; }
        public int Thickness { get; set; }
        public SolidColorBrush Color { get; set; } 

        public Polygon(List<Point> vertices, int thickness, SolidColorBrush color)
        {
            Vertices = vertices;
            Thickness = thickness;
            Color = color; 
        }
        public Polygon()
        {
            Vertices = new List<Point>();
            Thickness = new int();
            Color = new SolidColorBrush(); 
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
                DrawLine(polygon.Vertices[i], polygon.Vertices[i + 1], polygon.Thickness, color);
            }

            DrawLine(polygon.Vertices[polygon.Vertices.Count - 1], polygon.Vertices[0], polygon.Thickness, color);

            /*FillPolygon(polygon.Vertices, polygon.Color);*/

            if (selectedPolygon != null && polygon == selectedPolygon && isMovingEdge==false)
            {
                DrawPolygonOutline(polygon.Vertices, Brushes.Yellow, 2); 
            }
            else if (selectedPolygon != null && polygon == selectedPolygon && isMovingEdge == true)
            {
                DrawPolygonOutline(polygon.Vertices, Brushes.Green, 2);
            }
        }
        private void DrawPolygonOutline(List<Point> vertices, SolidColorBrush color, int thickness)
        {
            System.Windows.Shapes.Polygon polygonOutline = new System.Windows.Shapes.Polygon();

            PointCollection points = new PointCollection();
            foreach (var vertex in vertices)
            {
                points.Add(vertex);
            }
            polygonOutline.Points = points;

            polygonOutline.StrokeThickness = thickness + 2;
            polygonOutline.Stroke = color;

            polygonOutline.Points.Add(vertices[0]);

            DrawingCanvas.Children.Add(polygonOutline);
        }
        private void FillPolygon(List<Point> vertices, SolidColorBrush color)
        {
            if (vertices.Count < 3)
            {
                return;
            }

            Path filledPolygon = new Path();
            filledPolygon.Fill = color;

            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext context = geometry.Open())
            {
                context.BeginFigure(vertices[0], true, true);

                for (int i = 1; i < vertices.Count; i++)
                {
                    context.LineTo(vertices[i], true, false);
                }
            }

            geometry.Freeze();

            filledPolygon.Data = geometry;

            DrawingCanvas.Children.Add(filledPolygon);
        }

        private void DrawPolygon_Click(object sender, RoutedEventArgs e)
        {
            isDrawingLine = false;
            isDrawingCircle = false;
            isDrawingPolygon = true;
            isDrawingCapsule = false;

            LineButton.IsChecked = false;
            CircleButton.IsChecked = false;
            PolygonButton.IsChecked = true;
            CapsuleButton.IsChecked = false;
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
