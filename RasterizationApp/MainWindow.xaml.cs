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
using static System.Windows.Forms.LinkLabel;
using System.Windows.Ink;


namespace RasterizationApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    

    public partial class MainWindow : Window
    {
        private static WriteableBitmap writableBitmap;

        private List<CustomLine> lines = new List<CustomLine>();
        private List<CustomCircle> circles = new List<CustomCircle>();
        private List<Polygon> polygons = new List<Polygon>();
        private List<Capsule> capsules = new List<Capsule>();
        private List<MyRectangle> rectangles = new List<MyRectangle>();

        private List<Point> currentPolygonVertices = new List<Point>();

        private Point startPoint;
        private Point endPoint;
        private Point selectedVertex;

        private CustomLine selectedLine = null;
        private CustomCircle selectedCircle = null;
        private Polygon selectedPolygon = null;
        private MyRectangle selectedRectangle = null;

        private bool isDrawing = false;
        private bool isDrawingLine = true;
        private bool isDrawingCircle = false;
        private bool isDrawingPolygon = false;
        private bool isDrawingCapsule = false;
        private bool isDrawingRectangle = false;

        public SolidColorBrush SelectedColor { get; set; } = Brushes.Black; 
        private bool antialiasingEnabled = false;

        private bool isMovingPolygonEdge = false;
        private bool isMovingRectangleEdge = false;

        Capsule capsule = null;
        private Point firstVertex;
        private bool isFirstClick = true;

        public static int bitmapWidth = 1400;
        public static int bitmapHeight = 800;

        public MainWindow()
        {
            InitializeComponent();
            MouseDown += Canvas_MouseDown;
            KeyDown += MainWindow_KeyDown;

            InitializeWritableBitmap();
        }
        private void InitializeWritableBitmap()
        {
            // Create a WritableBitmap with the desired width, height, and DPI settings
            writableBitmap = new WriteableBitmap(bitmapWidth, bitmapHeight, 96, 96, PixelFormats.Bgra32, null);

            // Display the WritableBitmap in an Image control
            CanvasBitmap.Source = writableBitmap;
            SetBitmapColor(writableBitmap, Colors.White);
        }

        public static void SetBitmapColor(WriteableBitmap bitmap, System.Windows.Media.Color color)
        {
            int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
            int stride = bitmap.PixelWidth * bytesPerPixel;
            int size = stride * bitmap.PixelHeight;
            byte[] pixels = new byte[size];

            for (int i = 0; i < size; i += bytesPerPixel)
            {
                pixels[i] = color.B;
                pixels[i + 1] = color.G;
                pixels[i + 2] = color.R;
                pixels[i + 3] = color.A;
            }

            bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixels, stride, 0);
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.G)
            {
                if (isDrawingPolygon)
                {
                    isMovingPolygonEdge = !isMovingPolygonEdge;
                    Point cursorPosition = Mouse.GetPosition(CanvasBitmap);

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
                else if (isDrawingRectangle)
                {
                    isMovingRectangleEdge = !isMovingRectangleEdge;
                    Point cursorPosition = Mouse.GetPosition(CanvasBitmap);

                    foreach (var rectangle in rectangles)
                    {
                        if (IsPointInsideRectangle(cursorPosition, rectangle))
                        {
                            if (selectedRectangle == rectangle)
                            {
                                selectedRectangle = null;
                            }
                            else
                            {
                                selectedRectangle = rectangle;
                            }
                            RedrawCanvas();
                            return;
                        }
                    }
                }
            }
            else if (e.Key == Key.E)
            {
                if (isDrawingLine)
                {
                    Point cursorPosition = Mouse.GetPosition(CanvasBitmap);

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
                    Point cursorPosition = Mouse.GetPosition(CanvasBitmap);

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
                    Point cursorPosition = Mouse.GetPosition(CanvasBitmap);

                    foreach (var polygon in polygons)
                    {
                        if (IsPointInsidePolygon(cursorPosition, polygon.Vertices))
                        {
                            if (selectedPolygon == polygon)
                            {
                                selectedPolygon = null;
                                isMovingPolygonEdge = false;
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
                else if (isDrawingRectangle)
                {
                    Point cursorPosition = Mouse.GetPosition(CanvasBitmap);

                    foreach (var rectangle in rectangles)
                    {
                        if (IsPointInsideRectangle(cursorPosition, rectangle))
                        {
                            if (selectedRectangle == rectangle)
                            {
                                selectedRectangle = null;
                                isMovingRectangleEdge = false;
                            }
                            else
                            {
                                selectedRectangle = rectangle;
                            }
                            RedrawCanvas();
                            return;
                        }
                    }
                }
            }
            else if (e.Key == Key.C)
            {
                if (isDrawingRectangle)
                {
                    Point cursorPosition = Mouse.GetPosition(CanvasBitmap);

                    foreach (var rectangle in rectangles)
                    {
                        if (IsPointInsideRectangle(cursorPosition, rectangle))
                        {
                            if (clippingRectangle == rectangle)
                            {
                                clippingRectangle = null;
                            }
                            else
                            {
                                clippingRectangle = rectangle;
                            }
                            RedrawCanvas();
                            return;
                        }
                    }
                }
                /*else if (isDrawingPolygon) 
                {
                    Point cursorPosition = Mouse.GetPosition(CanvasBitmap);

                    foreach (var polygon in polygons)
                    {
                        if (IsPointInsidePolygon(cursorPosition, polygon.Vertices))
                        {
                            if (clippingRectangle != null)
                            {
                                polygon.ClippingRectangle = clippingRectangle;
                            }
                            else
                            {
                                polygon.ClippingRectangle = null;
                            }
                            RedrawCanvas();
                            return;
                        }
                    }
                }*/
            }
            else if (e.Key == Key.D)
            {
                Point cursorPosition = Mouse.GetPosition(CanvasBitmap);

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
                else if (isDrawingRectangle)
                {
                    foreach (var rectangle in rectangles)
                    {
                        if (IsPointInsideRectangle(cursorPosition, rectangle))
                        {
                            if (selectedRectangle == null)
                            {
                                selectedRectangle = rectangle;
                                if (clippingRectangle == rectangle)
                                {
                                    clippingRectangle = null;
                                }
                                rectangles.Remove(selectedRectangle);
                                selectedRectangle = null;
                                RedrawCanvas();
                                return;
                            }
                        }
                    }
                }
            }
            else if (e.Key == Key.F)
            {
                if (selectedPolygon != null)
                {
                    if (selectedPolygon.isFilled == true)
                    {
                        selectedPolygon.isFilled = false;
                        selectedPolygon.Color = Brushes.Black;
                    }
                    else if (selectedPolygon.isFilled == false)
                    {
                        ColorDialog colorDialog = new ColorDialog();
                        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            SolidColorBrush newColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                            selectedPolygon.Color = newColor;
                        }
                        selectedPolygon.isFilled = true;
                    }
                }
                RedrawCanvas();
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
            else if (e.Key == Key.T && selectedRectangle != null)
            {
                int newThickness = PromptForThickness();

                if (newThickness > 0)
                {
                    foreach (var rectangle in rectangles)
                    {
                        if (rectangle == selectedRectangle)
                        {
                            rectangle.Thickness = newThickness;

                            RedrawCanvas();
                            return;
                        }
                    }
                }
            }
            else if (e.Key == Key.I && selectedPolygon != null)
            {
                Point cursorPosition = Mouse.GetPosition(CanvasBitmap);

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp;*.tiff)|*.jpg;*.jpeg;*.png;*.bmp;*.tiff";
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string imagePath = openFileDialog.FileName;
                    SelectedImagePath = imagePath;
                }

                foreach (var polygon in polygons)
                {
                    if (IsPointInsidePolygon(cursorPosition, polygon.Vertices))
                    {
                        selectedPolygon = polygon;
                        selectedPolygon.Color = Brushes.Black;
                        selectedPolygon.isFilled = false;
                        selectedPolygon.IsImageFilled = true;
                        selectedPolygon.FillImagePath = SelectedImagePath;
                        RedrawCanvas();
                        return;
                    }
                }
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickedPoint = e.GetPosition(CanvasBitmap);

            /*if (boundaryMode == true)
            {
                BoundaryFill((int)clickedPoint.X, (int)clickedPoint.Y, fillColor, boundaryColor);
            }*/

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
                if (selectedPolygon != null && isMovingPolygonEdge == false)
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
                            Point clickedPos = e.GetPosition(CanvasBitmap);

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
                else if (selectedPolygon != null && isMovingPolygonEdge == true)
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
                Point clickedPos = e.GetPosition(CanvasBitmap);

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
            else if (isDrawingRectangle)
            {
                if (selectedRectangle != null && isMovingRectangleEdge == false)
                {
                    if (e.RightButton == MouseButtonState.Pressed)
                    {
                        {
                            double deltaX = clickedPoint.X - selectedRectangle.Vertices[0].X;
                            double deltaY = clickedPoint.Y - selectedRectangle.Vertices[0].Y;

                            for (int i = 0; i < selectedRectangle.Vertices.Count; i++)
                            {
                                selectedRectangle.Vertices[i] = new Point(selectedRectangle.Vertices[i].X + deltaX, selectedRectangle.Vertices[i].Y + deltaY);
                            }

                            RedrawCanvas();
                        }
                    }
                    else if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        double minDistance = double.MaxValue;
                        foreach (var vertex in selectedRectangle.Vertices)
                        {
                            Console.WriteLine(vertex);
                            double distance = CalculateDistance(clickedPoint, vertex);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                selectedVertex = vertex;
                            }
                        }
                        
                        if (selectedVertex != null)
                        {
                            int vertexIndex = selectedRectangle.Vertices.IndexOf(selectedVertex);
                            selectedRectangle.Vertices[vertexIndex] = clickedPoint;

                            if (vertexIndex == 0)
                            {
                                selectedRectangle.Vertices[1] = new Point(selectedRectangle.Vertices[1].X, clickedPoint.Y);
                                selectedRectangle.Vertices[3] = new Point(clickedPoint.X, selectedRectangle.Vertices[3].Y);
                            }
                            else if (vertexIndex == 1)
                            {
                                selectedRectangle.Vertices[0] = new Point(selectedRectangle.Vertices[0].X, clickedPoint.Y);
                                selectedRectangle.Vertices[2] = new Point(clickedPoint.X, selectedRectangle.Vertices[2].Y);
                            }
                            else if (vertexIndex == 2)
                            {
                                selectedRectangle.Vertices[1] = new Point(clickedPoint.X, selectedRectangle.Vertices[1].Y);
                                selectedRectangle.Vertices[3] = new Point(selectedRectangle.Vertices[3].X, clickedPoint.Y);
                            }
                            else if (vertexIndex == 3)
                            {
                                selectedRectangle.Vertices[0] = new Point(clickedPoint.X, selectedRectangle.Vertices[0].Y);
                                selectedRectangle.Vertices[2] = new Point(selectedRectangle.Vertices[2].X, clickedPoint.Y);
                            }
                            RedrawCanvas();
                            return;
                        }
                    }
                }
                else if (selectedRectangle != null && isMovingRectangleEdge == true)
                {
                    List<CustomLine> edges = new List<CustomLine>();
                    for (int i = 0; i < selectedRectangle.Vertices.Count; i++)
                    {
                        Point startPoint = selectedRectangle.Vertices[i];
                        Point endPoint = selectedRectangle.Vertices[(i + 1) % selectedRectangle.Vertices.Count];
                        edges.Add(new CustomLine(startPoint, endPoint, selectedRectangle.Thickness, selectedRectangle.Color));
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

                    // Move the edge based on the position of the clicked point

                    if (closestEdge.StartPoint.Y == closestEdge.EndPoint.Y) // Horizontal edge
                    {
                        if (closestEdge.StartPoint == selectedRectangle.Vertices[0] || closestEdge.EndPoint == selectedRectangle.Vertices[0])
                        {
                            selectedRectangle.Vertices[0] = new Point(selectedRectangle.Vertices[0].X, clickedPoint.Y);
                            selectedRectangle.Vertices[1] = new Point(selectedRectangle.Vertices[0].X, selectedRectangle.Vertices[2].Y);
                            selectedRectangle.Vertices[3] = new Point(selectedRectangle.Vertices[2].X, selectedRectangle.Vertices[0].Y);
                            RedrawCanvas();
                        }
                        else if (closestEdge.StartPoint == selectedRectangle.Vertices[2] || closestEdge.EndPoint == selectedRectangle.Vertices[2])
                        {
                            selectedRectangle.Vertices[2] = new Point(selectedRectangle.Vertices[2].X, clickedPoint.Y);
                            selectedRectangle.Vertices[1] = new Point(selectedRectangle.Vertices[0].X, selectedRectangle.Vertices[2].Y);
                            selectedRectangle.Vertices[3] = new Point(selectedRectangle.Vertices[2].X, selectedRectangle.Vertices[0].Y);
                            RedrawCanvas();
                        }
                    }
                    else //Vertical edge
                    {
                        if (closestEdge.StartPoint == selectedRectangle.Vertices[0] || closestEdge.EndPoint == selectedRectangle.Vertices[0])
                        {
                            selectedRectangle.Vertices[0] = new Point(clickedPoint.X, selectedRectangle.Vertices[0].Y);
                            selectedRectangle.Vertices[1] = new Point(selectedRectangle.Vertices[0].X, selectedRectangle.Vertices[2].Y);
                            selectedRectangle.Vertices[3] = new Point(selectedRectangle.Vertices[2].X, selectedRectangle.Vertices[0].Y);
                            RedrawCanvas();
                        }
                        else if (closestEdge.StartPoint == selectedRectangle.Vertices[2] || closestEdge.EndPoint == selectedRectangle.Vertices[2])
                        {
                            selectedRectangle.Vertices[2] = new Point(clickedPoint.X, selectedRectangle.Vertices[2].Y);
                            selectedRectangle.Vertices[1] = new Point(selectedRectangle.Vertices[0].X, selectedRectangle.Vertices[2].Y);
                            selectedRectangle.Vertices[3] = new Point(selectedRectangle.Vertices[2].X, selectedRectangle.Vertices[0].Y);
                            RedrawCanvas();

                        }
                    }
                }
                else
                {
                    if (isFirstClick)
                    {
                        firstVertex = clickedPoint;
                        isFirstClick = false;
                    }
                    else
                    {
                        Point thirdVertex = clickedPoint;
                        Point secondVertex = new Point(thirdVertex.X, firstVertex.Y);
                        Point forthVertex = new Point(firstVertex.X, thirdVertex.Y);

                        List<Point> vertices = new List<Point> { firstVertex, secondVertex, thirdVertex, forthVertex };

                        rectangles.Add(new MyRectangle(vertices, 1, Brushes.Black));
                        Console.WriteLine(rectangles.Count);
                        isFirstClick = true;
                    }
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
            InitializeWritableBitmap();

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
                    DrawLine(line.StartPoint, line.EndPoint, line.Thickness, Brushes.Red);
                }
            }
            
            foreach (var circle in circles)
            {
                if (circle != selectedCircle)
                {
                    DrawCircle(circle.Center, circle.Radius, circle.Color);
                }
                else
                {
                    DrawCircle(circle.Center, circle.Radius, Brushes.Red);
                }
            }

            foreach (var polygon in polygons)
            {
                if (polygon.isFilled)
                {
                    ScanLineFiller filler = new ScanLineFiller();
                    filler.FillPolygon(polygon);
                }
                if (polygon.IsImageFilled)
                {
                    ScanLineFiller filler = new ScanLineFiller();
                    filler.FillPolygonImage(polygon);
                }
                DrawPolygon(polygon, polygon.Color);
                if (selectedPolygon != null && polygon == selectedPolygon && isMovingPolygonEdge == false)
                {
                    DrawPolygon(polygon, Brushes.Red);
                }
                else if (selectedPolygon != null && polygon == selectedPolygon && isMovingPolygonEdge == true)
                {
                    DrawPolygon(polygon, Brushes.Green);
                }
                /*else if (polygon.ClippingRectangle != null)
                {
                    DrawPolygon(polygon, Brushes.BlueViolet);
                }*/
                
            }
            if (currentPolygonVertices.Count > 1)
            {
                Polygon polygon = new Polygon(currentPolygonVertices.ToList(),1, Brushes.Black);
                DrawPolygon(polygon, Brushes.Blue);
            }

            foreach (var capsule in capsules)
            {
                DrawCapsule(capsule);
            }

            foreach (var rectangle in rectangles)
            {
                
                if (selectedRectangle != null && rectangle == selectedRectangle && isMovingRectangleEdge == false)
                {
                    DrawRectangle(rectangle, Brushes.Red);
                }
                else if (selectedRectangle != null && rectangle == selectedRectangle && isMovingRectangleEdge == true)
                {
                    DrawRectangle(rectangle, Brushes.Green);
                }
                else if (clippingRectangle == rectangle)
                {
                    DrawRectangle(rectangle, Brushes.BlueViolet);
                }
                else
                {
                    DrawRectangle(rectangle);
                }
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
            rectangles.Clear();
            currentPolygonVertices.Clear();

            selectedLine = null;
            selectedCircle = null;
            selectedPolygon = null;
            capsule = null;
            selectedRectangle = null;

            isDrawing = false;
            isDrawingLine = true;
            isDrawingCircle = false;
            isDrawingPolygon = false;
            isDrawingCapsule = false;
            isDrawingRectangle = false;

            LineButton.IsChecked = true;
            CircleButton.IsChecked = false;
            PolygonButton.IsChecked = false;
            RectangleButton.IsChecked = false;

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
                    selectedCircle = null;
                    RedrawCanvas();
                }
                else if (selectedPolygon != null)
                {
                    selectedPolygon.Color = newColor;
                    selectedPolygon = null;
                    RedrawCanvas();
                }
                else if (selectedRectangle != null)
                {
                    selectedRectangle.Color = newColor;
                    selectedRectangle = null;
                    RedrawCanvas();
                }
            }
        }
        /*private void SelectFillColor_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SolidColorBrush newColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                fillColor = newColor;
            }
        }

        private void SelectBoundaryColor_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SolidColorBrush newColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                boundaryColor = newColor;
            }
        }

        private void BoundaryFill_Click(object sender, RoutedEventArgs e)
        {
            boundaryMode = true;
            isDrawingLine = false;
            isDrawingCircle = false;
            isDrawingPolygon = false;
            isDrawingRectangle = false;
        }*/
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
            dataToSave.Rectangles.Clear();

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
                    Color = polygon.Color,
                    IsFilled = polygon.isFilled,
                    IsImageFilled = polygon.IsImageFilled,
                    FillImagePath = polygon.FillImagePath,
                    FillImage = polygon.FillImage
                });
            }

            foreach (var rectangle in rectangles)
            {
                dataToSave.Rectangles.Add(new RectangleData
                {
                    Vertices = rectangle.Vertices,
                    Thickness = rectangle.Thickness,
                    Color = rectangle.Color
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