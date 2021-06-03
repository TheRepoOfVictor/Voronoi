using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Voronoi
{
    public class Arc
    {
        public Arc(Node f, Edge le, Edge re, Arc la, Arc ra)
        {
            Focus = f;
            LeftEdge = le;
            RightEdge = re;
            LeftArc = la;
            RightArc = ra;
            Event = null;
        }
        
        public Node Focus { get; set; }
        public Edge LeftEdge { get; set; }
        public Edge RightEdge { get; set; }
        public Arc LeftArc { get; set; }
        public Arc RightArc { get; set; }
        public Event Event { get; set; }

        public static bool operator <(Arc a1, Arc a2)
        {
            double middle1 = (VDiagram.GetCurrentLeftIntersectionOfArc(a1) + VDiagram.GetCurrentRightIntersectionOfArc(a1)) / 2;
            double middle2 = (VDiagram.GetCurrentLeftIntersectionOfArc(a2) + VDiagram.GetCurrentRightIntersectionOfArc(a2)) / 2;
            return middle1 < middle2;
        }

        public static bool operator >(Arc a1, Arc a2)
        {
            double middle1 = (VDiagram.GetCurrentLeftIntersectionOfArc(a1) + VDiagram.GetCurrentRightIntersectionOfArc(a1)) / 2;
            double middle2 = (VDiagram.GetCurrentLeftIntersectionOfArc(a2) + VDiagram.GetCurrentRightIntersectionOfArc(a2)) / 2;
            return middle1 > middle2;
        }
    }

    public class Edge
    {
        public Edge(Node o, Node d, Arc la, Arc ra, Node end = null)
        {
            Origin = o;
            Direction = d;
            LeftArc = la;
            RightArc = ra;
            End = end;
        }

        public Node Origin { get; set; }
        public Node Direction { get; set; }
        public Arc LeftArc { get; set; }
        public Arc RightArc { get; set; }
        public Node End { get; set; }
    }

    public class Node
    {
        public Node(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Node operator +(Node n1, Node n2)
        {
            return new Node(n1.X + n2.X, n2.Y + n1.Y);
        }

        public double X { get; }
        public double Y { get; }
    }

    class VDiagram
    {
        private static RBTree SweepLine;
        private static PriorityQueue Queue;
        private static double LeftBound;
        private static double RightBound;
        private static double BottomBound;

        public VDiagram(double left, double right, double bottom)
        {
            LeftBound = left;
            RightBound = right;
            BottomBound = bottom;
            Nodes = new List<Node>();
            SweepLine = new RBTree();
            Queue = new PriorityQueue();

            Edge leftmost = new Edge(new Node(LeftBound, -BottomBound), new Node(LeftBound, BottomBound), null, null);
            Edge rightmost = new Edge(new Node(RightBound , -BottomBound), new Node(RightBound, BottomBound), null, null);
            Arc first = new Arc(new Node(RightBound/2, -BottomBound), leftmost, rightmost, null, null);
            leftmost.RightArc = first;
            rightmost.LeftArc = first;
            Edges = new List<Edge>();
            SweepLine.Insert(first);
        }


        public List<Edge> Edges { get; set; }
        public List<Node> Nodes { get; }
        public static double Directrix { get; set; }

        public void Sweep()
        {
            foreach(Node n in Nodes)
            {
                Event e = new Event(n, true, Event.Type.Site);
                Queue.Insert(e);
            }

            while (!Queue.Empty())
            {
                Event next = Queue.Pop();
                Directrix = next.Priority;
                if(next.Valid && next.T == Event.Type.Site)
                {
                    HandleSiteEvent(next);
                }
                else if(next.Valid)
                {
                    HandleCircleEvent(next);
                }
            }
            Cleanup(SweepLine.Find(0));
        }

        private void Cleanup(Arc arc)
        {
            if (arc == null) return;
            if (arc.RightArc == null) return;
            Node bottomLeft = new Node(LeftBound, BottomBound);
            Node bottomRight = new Node(RightBound, BottomBound);
            Node intersection = GetIntersectionFromPoints(arc.RightEdge.Origin, arc.RightEdge.Direction, bottomLeft, bottomRight);
            if (intersection != null)
            {
                Edges.Add(new Edge(arc.RightEdge.Origin, null, null, null, intersection));
            }
            Cleanup(arc.RightArc);
        }

        private static void CheckForCircleEvents(Arc arc)
        {
            if (arc.LeftArc == null && arc.RightArc == null) return;
            if(arc.LeftArc != null && arc.RightArc != null && arc.LeftArc.Focus.Equals(arc.RightArc.Focus)) return;

            Edge le = arc.LeftEdge;
            Edge re = arc.RightEdge;
            Node intersection = GetIntersectionFromPoints(le.Origin, le.Direction, re.Origin, re.Direction);
            if (intersection == null) return;
            double dy;
            if(arc.LeftArc != null)
            {
                dy = Math.Sqrt(Math.Pow(arc.LeftArc.Focus.X - intersection.X, 2.0) + Math.Pow(arc.LeftArc.Focus.Y - intersection.Y, 2.0));
            }
            else
            {
                dy = Math.Sqrt(Math.Pow(arc.RightArc.Focus.X - intersection.X, 2.0) + Math.Pow(arc.RightArc.Focus.Y - intersection.Y, 2.0));
            }
            if (intersection.Y + dy < Directrix) return;
            Node node = new Node(intersection.X, intersection.Y + dy);
            Event newEvent = new Event(node, true, Event.Type.Circle, intersection, arc);
            arc.Event = newEvent;
            Queue.Insert(newEvent);
        }

        private void HandleSiteEvent(Event e)
        {
            Node next = e.Node;
            Arc above = SweepLine.Find(next.X);
            SweepLine.Delete(above);
            if(above.Event != null)
            {
                above.Event.Valid = false;
            }
            
            Node intersection = GetNodeFromCurve(above.Focus, next);
            Node[] directions = GetNormalDirections(above.Focus, next);
            Edge leftedge = new Edge(intersection, directions[0] + intersection, null, null);
            Edge rightedge = new Edge(intersection, directions[1] + intersection, null, null);

            Arc newSite = new Arc(next, leftedge, rightedge, null, null);
            Arc newleft = new Arc(above.Focus, above.LeftEdge, leftedge, above.LeftArc, newSite);
            if(newleft.LeftArc != null) newleft.LeftArc.RightArc = newleft;
            newleft.LeftEdge.RightArc = newleft;
            Arc newRight = new Arc(above.Focus, rightedge, above.RightEdge, newSite, above.RightArc);
            if (newRight.RightArc != null) newRight.RightArc.LeftArc = newRight;
            newRight.RightEdge.LeftArc = newRight;

            leftedge.LeftArc = newleft;
            leftedge.RightArc = newSite;

            rightedge.LeftArc = newSite;
            rightedge.RightArc = newRight;

            newSite.LeftArc = newleft;
            newSite.RightArc = newRight;

            SweepLine.Insert(newSite);
            SweepLine.Insert(newleft);
            SweepLine.Insert(newRight);

            CheckForCircleEvents(newleft);
            CheckForCircleEvents(newRight);
        }

        private void HandleCircleEvent(Event e)
        {
            Arc toRemove = e.Arc;
            Arc leftArc = toRemove.LeftArc;
            Arc rightArc = toRemove.RightArc;
            if (leftArc == null && rightArc == null) return;
            if(leftArc != null && leftArc.Event != null)
            {
                leftArc.Event.Valid = false;
            }
            if (rightArc != null && rightArc.Event != null)
            {
                rightArc.Event.Valid = false;
            }
            SweepLine.Delete(toRemove);

            if (leftArc == null)
            {
                rightArc.LeftEdge = toRemove.LeftEdge;
                rightArc.LeftArc = null;
                Edges.Add(new Edge(toRemove.RightEdge.Origin, null, null, null, e.Intersection));
                CheckForCircleEvents(rightArc);
            }
            else if (rightArc == null)
            {
                leftArc.RightEdge = toRemove.RightEdge;
                leftArc.RightArc = null;
                Edges.Add(new Edge(toRemove.LeftEdge.Origin, null, null, null, e.Intersection));
                CheckForCircleEvents(leftArc);
            }
            else
            {
                Node newDirection = GetNormalDirections(leftArc.Focus, rightArc.Focus)[0];
                Edge newEdge = new Edge(e.Intersection, newDirection + e.Intersection, leftArc, rightArc);
                leftArc.RightEdge = newEdge;
                leftArc.RightArc = rightArc;
                rightArc.LeftEdge = newEdge;
                rightArc.LeftArc = leftArc;
                Edges.Add(new Edge(toRemove.RightEdge.Origin, null, null, null, e.Intersection));
                Edges.Add(new Edge(toRemove.LeftEdge.Origin, null, null, null, e.Intersection));
                CheckForCircleEvents(rightArc);
                CheckForCircleEvents(leftArc);
            }
        }

        public void AddNode(double x, double y)
        {
            Node node = new Node(x, y);
            Nodes.Add(node);
        }

        public void PrintNodes()
        {
            foreach(Node n in Nodes)
            {
                Debug.Write("(" + n.X + "," + n.Y + ") ");
            }
            Debug.WriteLine("");
        }

        static private Node[] GetNormalDirections(Node Upper, Node Lower)
        {
            double dx = Upper.X - Lower.X;
            double dy = Upper.Y - Lower.Y;

            Node leftDirection = new Node(dy, dx * -1);
            Node rightDirection = new Node(dy * -1, dx);

            return new Node[] { leftDirection, rightDirection };
        }

        static public double CurveFunc(Node focus, double dir, double x)
        {
            double x1 = focus.X;
            double y1 = focus.Y;
            double y = Math.Pow(x - x1, 2.0) / (2.0 * (y1 - dir)) + (y1 + dir) / 2.0;
            return y;
        }

        static public Node GetNodeFromCurve(Node focus, Node p)
        {
            double y2 = p.Y;
            double x = p.X;
            double y = CurveFunc(focus, y2, x);
            return new Node(x, y);
        }

        static public double[] GetIntersectionsFromCurves(Node f1, Node f2, double dir)
        {
            double x1 = f1.X;
            double y1 = f1.Y;
            double x2 = f2.X;
            double y2 = f2.Y;

            double d1 = y1 - dir;
            if(d1 == 0)
            {
                Node intersection = GetNodeFromCurve(f2, f1);
                return new double[] { intersection.X - 0.01, intersection.X + 0.01 };
            }

            double d2 = y2 - dir;
            if(d2 == 0)
            {
                Node intersection = GetNodeFromCurve(f1, f2);
                return new double[] { intersection.X - 0.01, intersection.X + 0.01 };
            }

            if(y1 == y2)
            {
                double average = (x1 + x2) / 2;
                return new double[] { average, average };
            }

            double a1 = 1.0 / (2.0 * d1);
            double b1 = -2.0 * x1 / (2.0 * d1);
            double c1 = x1 * x1 / (2.0 * d1) + (y1 + dir) / 2.0;

            double a2 = 1.0 / (2.0 * d2);
            double b2 = -2.0 * x2 / (2.0 * d2);
            double c2 = x2 * x2 / (2.0 * d2) + (y2 + dir) / 2.0;

            double det = Math.Sqrt(Math.Pow(b1 - b2, 2.0) - 4.0 * (a1 - a2) * (c1 - c2));

            double posX = (-1.0 * (b1 - b2) + det) / (2 * (a1 - a2));
            double negX = (-1.0 * (b1 - b2) - det) / (2 * (a1 - a2));
            return new double[] { posX, negX };
        }

        static private bool CheckIntersection(Node origin, Node direction, Node intersection)
        {
            double x1 = origin.X;
            double y1 = origin.Y;
            double x2 = direction.X;
            double y2 = direction.Y;
            double x3 = intersection.X;
            double y3 = intersection.Y;

            double dx1 = x3 - x2;
            double dx2 = x3 - x1;
            double dx3 = x2 - x1;

            double dy1 = y3 - y2;
            double dy2 = y3 - y1;
            double dy3 = y2 - y1;

            return (Math.Abs(dx1) <= Math.Abs(dx2 + dx3)) && (Math.Abs(dy1) <= Math.Abs(dy2 + dy3));
        }

        static public Node GetIntersectionFromPoints(Node n1, Node n2, Node n3, Node n4)
        {
            double x1 = n1.X;
            double y1 = n1.Y;
            double x2 = n2.X;
            double y2 = n2.Y;
            double x3 = n3.X;
            double y3 = n3.Y;
            double x4 = n4.X;
            double y4 = n4.Y;

            double a1 = (y1 - y2) / (x1 - x2);
            double b1 = y1 - a1 * x1;

            if(x3 == x4)
            {
                double y_ = a1 * x3 + b1;
                Node i = new Node(x3, y_);
                return CheckIntersection(n1, n2, i) && CheckIntersection(n3, n4, i) ? i : null;
            }

            double a2 = (y3 - y4) / (x3 - x4);
            double b2 = y3 - a2 * x3;

            if (x1 == x2)
            {
                double y_ = a2 * x1 + b2;
                Node i = new Node(x1, y_);
                return CheckIntersection(n1, n2, i) && CheckIntersection(n3, n4, i) ? i : null;
            }

            if (a1 - a2 == 0) return null;

            double x = (b2 - b1) / (a1 - a2);
            double y = a1 * x + b1;

            Node intersection = new Node(x, y);

            return CheckIntersection(n1, n2, intersection) && CheckIntersection(n3, n4, intersection) ? intersection : null;
        }

        public static double GetCurrentLeftIntersectionOfArc(Arc arc)
        {
            if (arc.LeftArc == null) return LeftBound;
            double[] intersections = GetIntersectionsFromCurves(arc.Focus, arc.LeftArc.Focus, Directrix);
            double y1 = CurveFunc(arc.Focus, Directrix, intersections[0] + 0.5);
            double y2 = CurveFunc(arc.LeftArc.Focus, Directrix, intersections[0] + 0.5);
            return y1 > y2 ? intersections[0] : intersections[1];
        }

        public static double GetCurrentRightIntersectionOfArc(Arc arc)
        {
            if (arc.RightArc == null) return RightBound;
            double[] intersections = GetIntersectionsFromCurves(arc.Focus, arc.RightArc.Focus, Directrix);
            double y1 = CurveFunc(arc.Focus, Directrix, intersections[0] + 0.5);
            double y2 = CurveFunc(arc.RightArc.Focus, Directrix, intersections[0] + 0.5);
            return y1 < y2 ? intersections[0] : intersections[1];
        }
    }
}
