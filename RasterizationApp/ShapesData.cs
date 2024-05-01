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
using Color = System.Windows.Media.Color;
using System.IO;
using System.Xml.Serialization;
namespace RasterizationApp
{
    [XmlInclude(typeof(System.Windows.Media.MatrixTransform))]
    public class ShapeData
    {
        public List<LineData> Lines { get; set; }
        public List<CircleData> Circles { get; set; }
        public List<PolygonData> Polygons { get; set; }

        public ShapeData()
        {
            Lines = new List<LineData>();
            Circles = new List<CircleData>();
            Polygons = new List<PolygonData>();
        }
    }

    public class LineData
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public int Thickness { get; set; }
        public SolidColorBrush Color { get; set; }
    }

    public class CircleData
    {
        public Point Center { get; set; }
        public double Radius { get; set; }
        public SolidColorBrush Color { get; set; }
    }

    public class PolygonData
    {
        public List<Point> Vertices { get; set; }
        public int Thickness { get; set; }
        public SolidColorBrush Color { get; set; }
    }

    public class ShapeSerializer
    {
        public static void SaveShapes(string filePath, ShapeData data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ShapeData));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, data);
            }
        }

        public static ShapeData LoadShapes(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ShapeData));
            using (StreamReader reader = new StreamReader(filePath))
            {
                return (ShapeData)serializer.Deserialize(reader);
            }
        }
    }

    public partial class MainWindow : Window
    {
        ShapeData dataToSave = new ShapeData();

        private void SaveShapesToFile()
        {
            // Create a SaveFileDialog instance
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            // Set initial directory and file name
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.FileName = ""; // Set FileName to empty string

            // Set the file filter and default extension
            saveFileDialog.Filter = "XML files (*.xml)|*.xml";
            saveFileDialog.DefaultExt = ".xml";

            // Show the SaveFileDialog
            bool? result = saveFileDialog.ShowDialog();

            // If the user clicks the Save button
            if (result == true)
            {
                // Get the selected file path
                string filePath = saveFileDialog.FileName;

                // Save the shapes to the selected file
                ShapeSerializer.SaveShapes(filePath, dataToSave);
            }
        }
        private void LoadShapesFromFile()
        {
            // Create an OpenFileDialog instance
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            // Set initial directory and file filter
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "XML files (*.xml)|*.xml";

            // Show the OpenFileDialog
            bool? result = openFileDialog.ShowDialog();

            // If the user clicks the Open button
            if (result == true)
            {
                // Get the selected file path
                string filePath = openFileDialog.FileName;

                // Load the shapes from the selected file
                ShapeData loadedData = ShapeSerializer.LoadShapes(filePath);

                // Clear existing shapes on the canvas
                Clear_Canvas();

                // Clear existing lists
                lines.Clear();
                circles.Clear();
                polygons.Clear();

                // Add loaded shapes to the canvas
                foreach (var lineData in loadedData.Lines)
                {
                    // Draw the line with the converted brush
                    lines.Add(new CustomLine(lineData.StartPoint, lineData.EndPoint, lineData.Thickness, lineData.Color));
                }

                foreach (var circleData in loadedData.Circles)
                {
                    // Draw the circle with the converted brush
                    circles.Add(new CustomCircle(circleData.Center, circleData.Radius, circleData.Color));
                }

                foreach (var polygonData in loadedData.Polygons)
                {
                    // Draw the polygon with the converted brush
                    polygons.Add(new Polygon(polygonData.Vertices,polygonData.Thickness, polygonData.Color));
                }
            }
            RedrawCanvas();
        }
    }
}
