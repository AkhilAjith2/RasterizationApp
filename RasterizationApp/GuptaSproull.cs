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

namespace RasterizationApp
{
    public partial class MainWindow : Window
    {
        private void ThickAntialiasedLine(int x1, int y1, int x2, int y2, float thickness, SolidColorBrush color)
        {
            int dx = Math.Abs(x2 - x1), dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int dE = 2 * dy, dNE = 2 * (dy - dx);
            int d = 2 * dy - dx;
            int two_v_dx = 0;
            float invDenom = 1 / (2 * (float)Math.Sqrt(dx * dx + dy * dy));
            float two_dx_invDenom = 2 * dx * invDenom;
            int x = x1, y = y1;
            int i;
            IntensifyPixel(x, y, thickness, 0, color);
            bool isSteep = dy > dx;
            if (!isSteep) // Line is more horizontal than vertical
            {
                for (i = 1; (x + i * sx) != x2 && Math.Abs(y - y2) > 1 && IntensifyPixel(x + i * sx, y, thickness, i * two_dx_invDenom, color) != 0; ++i) ;
                for (i = 1; (x - i * sx) != x2 && Math.Abs(y - y2) > 1 && IntensifyPixel(x - i * sx, y, thickness, i * two_dx_invDenom, color) != 0; ++i) ;
            }
            else // Line is more vertical than horizontal (steep line)
            {
                for (i = 1; (y + i * sy) != y2 && IntensifyPixel(x, y + i * sy, thickness, i * two_dx_invDenom, color) != 0; ++i) ;
                for (i = 1; (y - i * sy) != y2 && IntensifyPixel(x, y - i * sy, thickness, i * two_dx_invDenom, color) != 0; ++i) ;
            }
            while ((x != x2 || Math.Abs(y - y2) > 1) && (x >= 0 && x < CanvasBitmap.ActualWidth && y >= 0 && y < CanvasBitmap.ActualHeight))
            {
                if (d < 0)
                {
                    two_v_dx = d + dx;
                    d += dE;
                }
                else
                {
                    two_v_dx = d - dx;
                    d += dNE;
                    y += sy;
                }
                x += sx;
                IntensifyPixel(x, y, thickness, two_v_dx * invDenom, color);
                for (i = 1; (y + i * sy) != y2 && IntensifyPixel(x, y + i * sy, thickness, i * two_dx_invDenom - two_v_dx * invDenom, color) != 0; ++i) ;
                for (i = 1; (y - i * sy) != y2 && IntensifyPixel(x, y - i * sy, thickness, i * two_dx_invDenom + two_v_dx * invDenom, color) != 0; ++i) ;
            }
        }

        private int IntensifyPixel(int x, int y, float thickness, float distance, SolidColorBrush color)
        {
            float r = 0.5f;
            float cov = coverage(thickness, distance, r);
            if (cov > 0)
                PutPixel(x, y, lerp(new SolidColorBrush(Colors.White), color, cov));
            return (int)cov;
        }

        private static float coverage(float thickness, float distance, float r)
        {
            float distToEdge = Math.Abs(distance) - thickness / 2.0f;
            float smoothstepValue = Clamp((r - distToEdge) / r, 0.0f, 1.0f);
            return smoothstepValue;
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        private static SolidColorBrush lerp(SolidColorBrush color1, SolidColorBrush color2, float t)
        {
            byte r = (byte)(color1.Color.R + (color2.Color.R - color1.Color.R) * t);
            byte g = (byte)(color1.Color.G + (color2.Color.G - color1.Color.G) * t);
            byte b = (byte)(color1.Color.B + (color2.Color.B - color1.Color.B) * t);
            return new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
        }
    }
}
