using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Voronoi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VDiagram Vd;
        private Node focus; 
        private static Canvas canvas;
        //private RBTree tree;

        public MainWindow()
        {
            InitializeComponent();

            Vd = new VDiagram(0, 800, 450);
            focus = new Node(400, 225);
            canvas = TheCanvas;
        }

        private void AddAndDrawNode(double x, double y)
        {
            Vd.AddNode(x, y);
            DrawDot(x, y, Brushes.Gray);
        }

        private void DrawDot(double x, double y, SolidColorBrush colour)
        {
            double dotSize = 10;
            Ellipse dot = new Ellipse()
            {
                Width = dotSize,
                Height = dotSize,
                Stroke = Brushes.Black,
                Fill = colour
            };

            dot.SetValue(Canvas.LeftProperty, x - dotSize / 2);
            dot.SetValue(Canvas.TopProperty, y - dotSize / 2);

            TheCanvas.Children.Add(dot);
        }

        private static void DrawLine(double x1, double y1, double x2, double y2)
        {
            Line l = new Line();
            l.Stroke = Brushes.LightSteelBlue;
            l.X1 = x1;
            l.X2 = x2;
            l.Y1 = y1;
            l.Y2 = y2;
            l.HorizontalAlignment = HorizontalAlignment.Left;
            l.VerticalAlignment = VerticalAlignment.Center;
            l.StrokeThickness = 3;
            canvas.Children.Add(l);
        }

        private void WhenMouseDown(Object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("MouseClicked at (" + e.GetPosition(this).X + "," + e.GetPosition(this).Y +")");

            
            Vd.AddNode(e.GetPosition(this).X, e.GetPosition(this).Y);
            Vd.PrintNodes();
            DrawDot(e.GetPosition(this).X, e.GetPosition(this).Y, Brushes.Gray);
        }
        
        private void Sweep(object sender, RoutedEventArgs e)
        {
            Vd.Sweep();
            foreach(Edge edge in Vd.Edges)
            {
                DrawLine(edge.Origin.X, edge.Origin.Y, edge.End.X, edge.End.Y);
            }
        }
        private void Clear(object sender, RoutedEventArgs e)
        {
            TheCanvas.Children.Clear();
            Vd = new VDiagram(0, 800, 450);
        }
    }
}
