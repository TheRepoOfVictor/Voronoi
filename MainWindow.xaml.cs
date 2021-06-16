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

        public MainWindow()
        {
            InitializeComponent();

            Vd = new VDiagram(0, 800, 450);
            focus = new Node(400, 225);
            canvas = TheCanvas;
            //AddAndDrawNode(540,192);
            //AddAndDrawNode(160,198);
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
            DrawDot(e.GetPosition(this).X, e.GetPosition(this).Y, Brushes.Gray);
            //Vd.PrintNodes();
        }

        private void Sweep(object sender, RoutedEventArgs e)
        {
            Vd.Sweep();
            foreach(Edge edge in Vd.Edges)
            {
                DrawLine(edge.Origin.X, edge.Origin.Y, edge.End.X, edge.End.Y);
            }

            /*
            foreach(Face f in Vd.TheDiagram.Faces)
            {
                Debug.WriteLine("Face at (" + f.Focus.X + "," + f.Focus.Y + ")");
                HalfEdge cur = f.Edges[0];

                
                Vertex origin = cur.Origin;
                if (origin != null) 
                { 
                    Debug.Write("[" + origin.X + "," + origin.Y + "]");
                    while (true)
                    {
                        Vertex dest = cur.Destination;
                        Debug.Write("----->[" + dest.X + "," + dest.Y + "]");

                        if(dest == null)
                        {
                            Debug.WriteLine("Something Went Wrong Here!");
                            break;
                        }

                        if (dest.X == origin.X && dest.Y == origin.Y)
                        {
                            Debug.WriteLine("");
                            break;
                        }
                        cur = cur.Next;
                    }
                }
                else Debug.WriteLine("Something Went Wrong Here!");
                


                Vertex dest = cur.Destination;
                if (dest != null)
                {
                    Debug.Write("[" + dest.X + "," + dest.Y + "]");
                    while (true)
                    {
                        Vertex ori = cur.Origin;
                        Debug.Write("----->[" + ori.X + "," + ori.Y + "]");

                        if (dest == null)
                        {
                            Debug.WriteLine("Something Went Wrong Here!");
                            break;
                        }

                        if (dest.X == ori.X && dest.Y == ori.Y)
                        {
                            Debug.WriteLine("");
                            break;
                        }
                        cur = cur.Prev;
                    }
                }
                else Debug.WriteLine("Something Went Wrong Here!");

                Debug.WriteLine("_____________________________________________");
            }
            */
        }

        private void Relax(object sender, RoutedEventArgs e)
        {
            List<Node> relaxedNodes = Vd.Relax();
            TheCanvas.Children.Clear();
            Vd = new VDiagram(0, 800, 450);
            foreach (Node n in relaxedNodes)
            {
                DrawDot(n.X, n.Y, Brushes.Gray);
                Vd.AddNode(n.X, n.Y);
            }
            Vd.Sweep();
            foreach (Edge edge in Vd.Edges)
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
