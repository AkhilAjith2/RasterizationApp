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
using Color = System.Drawing.Color;


namespace RasterizationApp
{
    public class Edge
    {
        public int YMax { get;}
        public double X { get; set; }
        public double InverseSlope { get;}

        public Edge(int yMax, double x, double inverseSlope)
        {
            YMax = yMax;
            X = x;
            InverseSlope = inverseSlope;
        }
    }

    public class ScanLineFiller
    {
        public void FillPolygon(Polygon polygon)
        {
            FillPolygonCommon(polygon, FillMode.Color);
        }

        public void FillPolygonImage(Polygon polygon)
        {
            if (string.IsNullOrEmpty(polygon.FillImagePath)) return;
            LoadImage(polygon);
            FillPolygonCommon(polygon, FillMode.Image);
        }

        private void FillPolygonCommon(Polygon polygon, FillMode fillMode)
        {
            List<Edge> AET = new List<Edge>();
            List<Point> P = polygon.Vertices;
            int N = P.Count;
            List<int> indices = P
                .Select((point, index) => new { Point = point, Index = index })
                .OrderBy(pair => pair.Point.Y)
                .Select(pair => pair.Index)
                .ToList();
            int k = 0;
            int i = indices[k];
            int y = (int)P[indices[0]].Y;
            int ymax = (int)P[indices[N - 1]].Y;

            int y_min = y;
            int x_min = (int)P.Min(point => point.X);

            while (y < ymax)
            {
                // Add edges to AET when the bottom of the edge is reached
                while (k < N && (int)P[i].Y == y)
                {
                    int prevIndex = (i == 0) ? N - 1 : i - 1;
                    int nextIndex = (i == N - 1) ? 0 : i + 1;

                    if (P[prevIndex].Y > P[i].Y)
                    {
                        AET.Add(CreateEdge(P[i], P[prevIndex], y));
                    }

                    if (P[nextIndex].Y > P[i].Y)
                    {
                        AET.Add(CreateEdge(P[i], P[nextIndex], y));
                    }

                    ++k;
                    if (k < N) i = indices[k];
                }

                // Remove edges from AET when the top of the edge is reached
                AET.RemoveAll(edge => edge.YMax == y);

                // Sort AET by x value and then by inverse slope
                AET.Sort((n1, n2) =>
                {
                    int xComparison = n1.X.CompareTo(n2.X);
                    return xComparison != 0 ? xComparison : n1.InverseSlope.CompareTo(n2.InverseSlope);
                });

                // Fill the scanline based on sorted AET
                for (int p = 0; p < AET.Count; p += 2)
                {
                    if (p + 1 < AET.Count)
                    {
                        int x1 = (int)Math.Round(AET[p].X);
                        int x2 = (int)Math.Round(AET[p + 1].X);

                        if (fillMode == FillMode.Color)
                        {
                            DrawScanLine(x1, x2, y, polygon.Color);
                        }
                        else if (fillMode == FillMode.Image)
                        {
                            FillScanLineWithImage(x1, x2, y, polygon, x_min, y_min);
                        }
                    }
                }

                y++;
                foreach (var edge in AET)
                {
                    edge.X += edge.InverseSlope;
                }

            }
        }


        private void FillScanLineWithImage(int xStart, int xEnd, int y, Polygon polygon, int x_min, int y_min)
        {
            for (int x = xStart; x < xEnd; x++)
            {
                int img_x = (x - x_min) % polygon.FillImage.PixelWidth;
                int img_y = (y - y_min) % polygon.FillImage.PixelHeight;
                Color pixelFromImage = MainWindow.GetPixelColorFromBitmap(polygon.FillImage, img_y, img_x);
                MainWindow.SetPixelColor(y, x, pixelFromImage);
            }
        }

        private Edge CreateEdge(Point p1, Point p2, double y)
        {
            int yMax = (int)Math.Max(p1.Y, p2.Y);
            double m = ((double)p2.Y - p1.Y) / (p2.X - p1.X);
            double x = (yMax==p1.Y) ? p2.X : p1.X;
            double inverseSlope = (m == 0) ? 0 : 1 / m;

            return new Edge(yMax, x, inverseSlope);
        }



        private void DrawScanLine(int xStart, int xEnd, int y, SolidColorBrush color)
        {
            System.Windows.Media.Color mediaColor = color.Color;
            Color convertedColor = Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
            for (int x = xStart; x <= xEnd; x++)
            {
                MainWindow.SetPixelColor(y, x, convertedColor);
            }
        }

        public void LoadImage(Polygon polygon)
        {
            BitmapImage bitmapImg = new BitmapImage();
            bitmapImg.BeginInit();
            bitmapImg.UriSource = new Uri(polygon.FillImagePath);
            bitmapImg.EndInit();

            polygon.FillImage = new WriteableBitmap(bitmapImg);
        }

        private enum FillMode
        {
            Color,
            Image
        }
    }


    public partial class MainWindow : Window
    {
        public static Color GetPixelColorFromBitmap(WriteableBitmap bitmap, int row, int column)
        {
            if (row < 0 || row >= bitmap.PixelHeight || column < 0 || column >= bitmap.PixelWidth)
            {
                return Color.Transparent;
            }
            /*bitmap.Lock();
            try
            {*/
                unsafe
                {
                    byte* pPixel =
                        (byte*)(bitmap.BackBuffer + row * bitmap.BackBufferStride + column * 4);
                    return Color.FromArgb(pPixel[2], pPixel[1], pPixel[0]);
                }
            /*}
            finally
            {
                bitmap.Unlock();
            }*/
        }

        public static void SetPixelColor(int row, int column, Color color)
        {
            if (row >= bitmapHeight || row < 0 || column >= bitmapWidth || column < 0)
                return;
            /*writableBitmap.Lock();
            try
            {*/
               
                unsafe
                {
                    byte* pPixel = (byte*)GetPixelPtr(row, column);
                    pPixel[0] = color.B;
                    pPixel[1] = color.G;
                    pPixel[2] = color.R;
                }
                /*writableBitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));
            }
            finally
            {
                writableBitmap.Unlock();
            }*/
            
        }

        public static IntPtr GetPixelPtr(int row, int column)
        {
            return writableBitmap.BackBuffer + row * writableBitmap.BackBufferStride + column * 4;
        }

        private string SelectedImagePath { get; set; }
    }
}
