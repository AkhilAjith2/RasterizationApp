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
using System.Security.AccessControl;
using System.Net;
using System.Diagnostics;


namespace RasterizationApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    

    public partial class MainWindow : Window
    {
        private WriteableBitmap writableBitmap;

        private List<CustomLine> lines = new List<CustomLine>();
        private List<CustomCircle> circles = new List<CustomCircle>();
        private List<Polygon> polygons = new List<Polygon>();
        private List<Capsule> capsules = new List<Capsule>();

        private List<Point> currentPolygonVertices = new List<Point>();

        private Point startPoint;
        private Point endPoint;
        private Point selectedVertex;

        private CustomLine selectedLine = null;
        private CustomCircle selectedCircle = null;
        private Polygon selectedPolygon = null;

        private bool isDrawing = false;
        private bool isDrawingLine = true;
        private bool isDrawingCircle = false;
        private bool isDrawingPolygon = false;
        private bool isDrawingCapsule = false;

        public SolidColorBrush SelectedColor { get; set; } = Brushes.Black; 
        private bool antialiasingEnabled = false;

        private bool isMovingEdge = false;

        Capsule capsule = null;

        public MainWindow()
        {
            InitializeComponent();
            MouseDown += Canvas_MouseDown;
            KeyDown += MainWindow_KeyDown;

            /*InitializeWritableBitmap();*/
        }
        /*private void InitializeWritableBitmap()
        {
            // Create a WritableBitmap with the desired width, height, and DPI settings
            writableBitmap = new WriteableBitmap(800, 600, 96, 96, PixelFormats.Bgra32, null);

            // Display the WritableBitmap in an Image control
            ImageControl.Source = writableBitmap;
        }*/

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.G)
            {
                isMovingEdge = !isMovingEdge;
                Point cursorPosition = Mouse.GetPosition(DrawingCanvas);

                foreach (var polygon in polygons)
                {
                    if (IsPointInsidePolygon(cursorPosition, polygon.Vertices))
                    {
                        if (selectedPolygon == polygon)
                        {
                            selectedPolygon = null;
                        }
                        else
                        {
                            selectedPolygon = polygon;
                        }
                        RedrawCanvas();
                        return;
                    }
                }
            }
            else if (e.Key == Key.E)
            {
                if (isDrawingLine)
                {
                    Point cursorPosition = Mouse.GetPosition(DrawingCanvas);

                    foreach (var line in lines)
                    {
                        if (ClosestLineToSelect(cursorPosition, line.StartPoint, line.EndPoint))
                        {
                            if (selectedLine == line)
                            {
                                selectedLine = null;
                            }
                            else
                            {
                                selectedLine = line;
                            }

                            RedrawCanvas();
                            break;
                        }
                    }
                }
                else if (isDrawingCircle)
                {
                    Point cursorPosition = Mouse.GetPosition(DrawingCanvas);

                    foreach (var circle in circles)
                    {
                        if (IsPointInsideCircle(cursorPosition, circle.Center, circle.Radius))
                        {
                            if (selectedCircle == circle)
                            {
                                selectedCircle = null;
                            }
                            else
                            {
                                selectedCircle = circle;

                            }

                            RedrawCanvas();
                            break;
                        }
                    }
                    
                }
                else if (isDrawingPolygon)
                {
                    Point cursorPosition = Mouse.GetPosition(DrawingCanvas);

                    foreach (var polygon in polygons)
                    {
                        if (IsPointInsidePolygon(cursorPosition, polygon.Vertices))
                        {
                            if (selectedPolygon == polygon)
                            {
                                selectedPolygon = null;
                                isMovingEdge = false;
                            }
                            else
                            {
                                selectedPolygon = polygon;
                            }
                            RedrawCanvas();
                            return;
                        }
                    }
                }

            }
            else if (e.Key == Key.D)
            {
                Point cursorPosition = Mouse.GetPosition(DrawingCanvas);

                if (isDrawingLine)
                {
                    foreach (var line in lines)
                    {
                        if (ClosestLineToSelect(cursorPosition, line.StartPoint, line.EndPoint))
                        {
                            if (selectedLine == null)
                            {
                                selectedLine = line;
                                lines.Remove(selectedLine);
                                selectedLine = null;
                                RedrawCanvas();
                                return;
                            }
                        }
                    }
                }
                else if (isDrawingCircle)
                {
                    foreach (var circle in circles)
                    {
                        if (IsPointInsideCircle(cursorPosition, circle.Center, circle.Radius))
                        {
                            if (selectedCircle == null)
                            {
                                selectedCircle = circle;
                                circles.Remove(selectedCircle);
                                selectedCircle = null;
                                RedrawCanvas();
                                return;
                            }
                        }
                    }
                }
                else if (isDrawingPolygon)
                {
                    foreach (var polygon in polygons)
                    {
                        if (IsPointInsidePolygon(cursorPosition, polygon.Vertices))
                        {
                            if (selectedPolygon == null)
                            {
                                selectedPolygon = polygon;
                                polygons.Remove(selectedPolygon);
                                selectedPolygon = null;
                                RedrawCanvas();
                                return;
                            }
                        }
                    }
                }
            }
            else if (e.Key == Key.T && selectedLine != null)
            {
                int newThickness = PromptForThickness();
                Console.WriteLine(newThickness);
                if (newThickness > 0)
                {
                    int index = lines.IndexOf(selectedLine);
                    if (index != -1)
                    {
                        lines[index] = new CustomLine(selectedLine.StartPoint, selectedLine.EndPoint, newThickness, selectedLine.Color);
                        selectedLine = lines[index];
                        RedrawCanvas();
                        return;
                    }
                }
            }
            else if (e.Key == Key.T && selectedPolygon != null)
            {
                int newThickness = PromptForThickness();

                if (newThickness > 0)
                {
                    foreach (var polygon in polygons)
                    {
                        if (polygon == selectedPolygon)
                        {
                            polygon.Thickness = newThickness;

                            RedrawCanvas();
                            return;
                        }
                    }
                }
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickedPoint = e.GetPosition(DrawingCanvas);

            if (isDrawingLine)
            {
                if (selectedLine != null)
                {
                    double distanceToStart = CalculateDistance(clickedPoint, selectedLine.StartPoint);
                    double distanceToEnd = CalculateDistance(clickedPoint, selectedLine.EndPoint);
                    int index = lines.IndexOf(selectedLine);

                    if (index != -1)
                    {
                        if (distanceToStart <= distanceToEnd)
                        {
                            lines[index] = new CustomLine(clickedPoint, selectedLine.EndPoint, selectedLine.Thickness, selectedLine.Color);
                            selectedLine = lines[index];
                            RedrawCanvas();
                        }
                        else
                        {
                            lines[index] = new CustomLine(selectedLine.StartPoint, clickedPoint, selectedLine.Thickness, selectedLine.Color);
                            selectedLine = lines[index];
                            RedrawCanvas();
                        }

                    }
                    return;
                }

                if (!isDrawing)
                {
                    startPoint = clickedPoint;
                    isDrawing = true;
                }
                else
                {
                    endPoint = clickedPoint;
                    lines.Add(new CustomLine(startPoint, endPoint, 1, Brushes.Black));
                    isDrawing = false;
                }
            }
            else if (isDrawingCircle)
            {
                if (selectedCircle != null)
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        double newRadius = CalculateDistance(selectedCircle.Center.X, selectedCircle.Center.Y, clickedPoint.X, clickedPoint.Y);

                        int index = circles.IndexOf(selectedCircle);
                        if (index != -1)
                        {
                            circles[index] = new CustomCircle(selectedCircle.Center, newRadius, selectedCircle.Color);
                            selectedCircle = circles[index];
                            RedrawCanvas();
                        }
                    }
                    else if (e.RightButton == MouseButtonState.Pressed)
                    {
                        int index = circles.IndexOf(selectedCircle);
                        if (index != -1)
                        {
                            circles[index] = new CustomCircle(clickedPoint, selectedCircle.Radius, selectedCircle.Color);
                            selectedCircle = circles[index];
                            RedrawCanvas();
                        }
                    }
                    return;
                }
                if (!isDrawing)
                {
                    startPoint = clickedPoint;
                    isDrawing = true;
                }
                else
                {
                    endPoint = clickedPoint;
                    double radius = CalculateDistance(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
                    circles.Add(new CustomCircle(startPoint, radius, Brushes.Black));
                    isDrawing = false;
                }
            }
            else if (isDrawingPolygon)
            {
                if (selectedPolygon != null && isMovingEdge == false)
                {
                    if (e.RightButton == MouseButtonState.Pressed)
                    {
                        {
                            double deltaX = clickedPoint.X - selectedPolygon.Vertices[0].X;
                            double deltaY = clickedPoint.Y - selectedPolygon.Vertices[0].Y;

                            for (int i = 0; i < selectedPolygon.Vertices.Count; i++)
                            {
                                selectedPolygon.Vertices[i] = new Point(selectedPolygon.Vertices[i].X + deltaX, selectedPolygon.Vertices[i].Y + deltaY);
                            }

                            RedrawCanvas();
                        }
                    }
                    else if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        double minDistance = double.MaxValue;
                        foreach (var vertex in selectedPolygon.Vertices)
                        {
                            double distance = CalculateDistance(clickedPoint, vertex);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                selectedVertex = vertex;
                            }
                        }
                        RedrawCanvas();

                        if (selectedVertex != null)
                        {
                            Point clickedPos = e.GetPosition(DrawingCanvas);

                            int vertexIndex = selectedPolygon.Vertices.IndexOf(selectedVertex);
                            selectedPolygon.Vertices[vertexIndex] = clickedPos;

                            if (vertexIndex > 0)
                            {
                                var prevEdge = lines.FindIndex(t => t.StartPoint == selectedPolygon.Vertices[vertexIndex - 1] && t.EndPoint == selectedVertex);
                                if (prevEdge != -1)
                                {
                                    lines[prevEdge] = new CustomLine(selectedPolygon.Vertices[vertexIndex - 1], clickedPos, selectedPolygon.Thickness, selectedPolygon.Color);
                                }
                            }
                            if (vertexIndex < selectedPolygon.Vertices.Count - 1)
                            {
                                var nextEdge = lines.FindIndex(t => t.StartPoint == selectedVertex && t.EndPoint == selectedPolygon.Vertices[vertexIndex + 1]);
                                if (nextEdge != -1)
                                {
                                    lines[nextEdge] = new CustomLine(clickedPos, selectedPolygon.Vertices[vertexIndex + 1], selectedPolygon.Thickness, selectedPolygon.Color);
                                }
                            }
                            RedrawCanvas();
                            return;
                        }
                    }
                }
                else if (selectedPolygon != null && isMovingEdge == true)
                {
                    List<CustomLine> edges = new List<CustomLine>();
                    for (int i = 0; i < selectedPolygon.Vertices.Count; i++)
                    {
                        Point startPoint = selectedPolygon.Vertices[i];
                        Point endPoint = selectedPolygon.Vertices[(i + 1) % selectedPolygon.Vertices.Count];
                        edges.Add(new CustomLine(startPoint, endPoint, selectedPolygon.Thickness, selectedPolygon.Color));
                    }

                    double closestDistance = double.MaxValue;
                    CustomLine closestEdge = null;

                    foreach (var edge in edges)
                    {
                        double distance = DistanceToEdge(clickedPoint, edge.StartPoint, edge.EndPoint);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestEdge = edge;
                        }
                    }

                    if (closestEdge != null)
                    {
                        Vector edgeVector = new Vector(closestEdge.EndPoint.X - closestEdge.StartPoint.X, closestEdge.EndPoint.Y - closestEdge.StartPoint.Y);
                        Vector normalVector = new Vector(-edgeVector.Y, edgeVector.X);
                        double x = (closestEdge.StartPoint.X + closestEdge.EndPoint.X) / 2;
                        double y = (closestEdge.StartPoint.Y + closestEdge.EndPoint.Y) / 2;
                        Point midpoint = new Point(x, y);

                        double deltaX = clickedPoint.X - midpoint.X;
                        double deltaY = clickedPoint.Y - midpoint.Y;
                        Point newMidpoint = new Point(clickedPoint.X - deltaX, clickedPoint.Y - deltaY);
                        Point newStartPoint = new Point(closestEdge.StartPoint.X + deltaX, closestEdge.StartPoint.Y + deltaY);
                        Point newEndPoint = new Point(closestEdge.EndPoint.X + deltaX, closestEdge.EndPoint.Y + deltaY);

                        int vertex1Index = selectedPolygon.Vertices.IndexOf(closestEdge.StartPoint);
                        int vertex2Index = selectedPolygon.Vertices.IndexOf(closestEdge.EndPoint);
                        selectedPolygon.Vertices[vertex1Index] = newStartPoint;
                        selectedPolygon.Vertices[vertex2Index] = newEndPoint;

                        closestEdge.StartPoint = newStartPoint;
                        closestEdge.EndPoint = newEndPoint;

                        RedrawCanvas();
                    }
                }
                else
                {
                    if (!IsCloseToFirstVertex(clickedPoint))
                    {
                        currentPolygonVertices.Add(clickedPoint);
                    }
                    if (currentPolygonVertices.Count > 2 && IsCloseToFirstVertex(clickedPoint))
                    {
                        polygons.Add(new Polygon(currentPolygonVertices.ToList(), 1, Brushes.Black));
                        currentPolygonVertices.Clear();
                    }
                }
            }

            else if (isDrawingCapsule)
            {
                Point clickedPos = e.GetPosition(DrawingCanvas);

                if (capsule == null)
                {
                    capsule = new Capsule(clickedPoint, new Point(), 0);
                }
                else if (capsule.EndPoint == new Point())
                {
                    capsule.EndPoint = clickedPoint;
                }
                else if (capsule.Radius == 0)
                {
                    double distance = Math.Sqrt(Math.Pow(clickedPoint.X - capsule.EndPoint.X, 2) + Math.Pow(clickedPoint.Y - capsule.EndPoint.Y, 2));
                    capsule.Radius = distance;
                    capsules.Add(capsule);
                    capsule = null;
                }

            }
            RedrawCanvas();
        }

        private double DistanceToEdge(Point point, Point startPoint, Point endPoint)
        {
            double edgeLength = Math.Sqrt(Math.Pow(endPoint.X - startPoint.X, 2) + Math.Pow(endPoint.Y - startPoint.Y, 2));
            if (edgeLength == 0) return double.MaxValue;
            double t = ((point.X - startPoint.X) * (endPoint.X - startPoint.X) + (point.Y - startPoint.Y) * (endPoint.Y - startPoint.Y)) / (edgeLength * edgeLength);
            if (t < 0) return Math.Sqrt(Math.Pow(point.X - startPoint.X, 2) + Math.Pow(point.Y - startPoint.Y, 2));
            if (t > 1) return Math.Sqrt(Math.Pow(point.X - endPoint.X, 2) + Math.Pow(point.Y - endPoint.Y, 2));
            Point projection = new Point(startPoint.X + t * (endPoint.X - startPoint.X), startPoint.Y + t * (endPoint.Y - startPoint.Y));
            return Math.Sqrt(Math.Pow(point.X - projection.X, 2) + Math.Pow(point.Y - projection.Y, 2));
        }

        private void RedrawCanvas()
        {
            DrawingCanvas.Children.Clear();

            foreach (var line in lines)
            {
                if (line != selectedLine)
                {
                    if(antialiasingEnabled)
                    {
                        ThickAntialiasedLine((int)line.StartPoint.X, (int)line.StartPoint.Y, (int)line.EndPoint.X, (int)line.EndPoint.Y, line.Thickness, line.Color);
                    }
                    else
                    {
                        DrawLine(line.StartPoint, line.EndPoint, line.Thickness, line.Color);
                    }
                }
                else
                {
                    DrawLine(line.StartPoint, line.EndPoint, line.Thickness, Brushes.Yellow);
                }
            }
            
            foreach (var circle in circles)
            {
                DrawCircle(circle.Center, circle.Radius, circle.Color);
            }

            foreach (var polygon in polygons)
            {
                DrawPolygon(polygon, polygon.Color);
            }
            if (currentPolygonVertices.Count > 1)
            {
                Polygon polygon = new Polygon(currentPolygonVertices.ToList(),3, Brushes.Black);
                DrawPolygon(polygon, Brushes.Blue);
            }

            foreach (var capsule in capsules)
            {
                DrawCapsule(capsule);
            }

        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            Clear_Canvas();
        }

        private void Clear_Canvas()
        {
            lines.Clear();
            circles.Clear();
            polygons.Clear();
            capsules.Clear();
            currentPolygonVertices.Clear();

            selectedLine = null;
            selectedCircle = null;
            selectedPolygon = null;
            capsule = null;

            isDrawing = false;
            isDrawingLine = true;
            isDrawingCircle = false;
            isDrawingPolygon = false;
            isDrawingCapsule = false;

            LineButton.IsChecked = true;
            CircleButton.IsChecked = false;
            PolygonButton.IsChecked = false;
            CapsuleButton.IsChecked = false;

            RedrawCanvas();
        }


        private void ColorPicker_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SolidColorBrush newColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                if (selectedLine != null)
                {
                    selectedLine.Color = newColor;
                    selectedLine = null;
                    RedrawCanvas();
                }
                else if (selectedCircle != null)
                {
                    selectedCircle.Color = newColor;
                    RedrawCanvas();
                }
                else if (selectedPolygon != null)
                {
                    selectedPolygon.Color = newColor;
                    RedrawCanvas();
                }
            }
        }

        

        private void ToggleAntialiasing_Click(object sender, RoutedEventArgs e)
        {
            antialiasingEnabled = !antialiasingEnabled;
            if (antialiasingEnabled)
            {
                AntialiasingButton.IsChecked = true;
            }
            else
            {
                AntialiasingButton.IsChecked = false;
            }
            RedrawCanvas();
            
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            dataToSave.Lines.Clear();
            dataToSave.Circles.Clear();
            dataToSave.Polygons.Clear();

            foreach (var line in lines)
            {
                dataToSave.Lines.Add(new LineData
                {
                    StartPoint = line.StartPoint,
                    EndPoint = line.EndPoint,
                    Thickness = line.Thickness,
                    Color = line.Color
                });
            }

            foreach (var circle in circles)
            {
                dataToSave.Circles.Add(new CircleData
                {
                    Center = circle.Center,
                    Radius = circle.Radius,
                    Color = circle.Color
                });
            }

            foreach (var polygon in polygons)
            {
                dataToSave.Polygons.Add(new PolygonData
                {
                    Vertices = polygon.Vertices,
                    Thickness = polygon.Thickness,
                    Color = polygon.Color
                });
            }
            SaveShapesToFile();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadShapesFromFile();
        }

    }
}   