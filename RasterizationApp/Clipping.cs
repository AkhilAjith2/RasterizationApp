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
    public partial class MainWindow : Window
    {
        private MyRectangle clippingRectangle;

        private List<Point> LiangBarsky(PointF p1, PointF p2, List<Point> clippedPolygon)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float tE = 0, tL = 1;

            // Define the clipping rectangle
            float minX = (float)clippingRectangle.Vertices.Min(v => v.X);
            float maxX = (float)clippingRectangle.Vertices.Max(v => v.X);
            float minY = (float)clippingRectangle.Vertices.Min(v => v.Y);
            float maxY = (float)clippingRectangle.Vertices.Max(v => v.Y);

            // Perform clipping
            if (Clip(dx, minX - p1.X, ref tE, ref tL) &&
                Clip(-dx, p1.X - maxX, ref tE, ref tL) &&
                Clip(dy, minY - p1.Y, ref tE, ref tL) &&
                Clip(-dy, p1.Y - maxY, ref tE, ref tL))
            {
                if (tL < 1)
                {
                    p2 = new PointF(p1.X + dx * tL, p1.Y + dy * tL);
                }

                if (tE > 0)
                {
                    p1 = new PointF(p1.X + dx * tE, p1.Y + dy * tE);
                }

                // Add clipped points as pairs (start and end)
                clippedPolygon.Add(new Point((int)p1.X, (int)p1.Y));
                clippedPolygon.Add(new Point((int)p2.X, (int)p2.Y));
            }

            return clippedPolygon;
        }

        private List<Point> ClipPolygon(Polygon polygon)
        {
            List<Point> clippedPolygon = new List<Point>();

            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                int nextIndex = (i + 1) % polygon.Vertices.Count;

                PointF p1 = new PointF((float)polygon.Vertices[i].X, (float)polygon.Vertices[i].Y);
                PointF p2 = new PointF((float)polygon.Vertices[nextIndex].X, (float)polygon.Vertices[nextIndex].Y);

                clippedPolygon = LiangBarsky(p1, p2, clippedPolygon);
            }

            return clippedPolygon;
        }

        private new bool Clip(float denom, float numer, ref float tE, ref float tL)
        {
            if (denom == 0)
            {
                if (numer > 0)
                    return false;
                return true;
            }

            float t = numer / denom;
            if (denom > 0)
            {
                if (t > tL)
                    return false;
                if (t > tE)
                    tE = t;
            }
            else
            {
                if (t < tE)
                    return false;
                if (t < tL)
                    tL = t;
            }
            return true;
        }
    }
}
