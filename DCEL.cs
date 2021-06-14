using System;
using System.Collections.Generic;
using System.Text;

namespace Voronoi
{
    class DCEL
    {
        public DCEL()
        {
            Faces = new List<Face>();
        }

        public List<Face> Faces { get; set; }
    }

    public class HalfEdge
    {
        public HalfEdge(Face i, Vertex o, Vertex d, HalfEdge t, HalfEdge p, HalfEdge n)
        {
            Incident = i;
            Origin = o;
            Destination = d;
            Twin = t;
            Prev = p;
            Next = n;
        }
        public Face Incident { get; set; }
        public Vertex Origin { get; set; }
        public Vertex Destination { get; set; }
        public HalfEdge Twin { get; set; }
        public HalfEdge Prev { get; set; }
        public HalfEdge Next { get; set; }
    }

    public class Face
    {
        public Face(Vertex f)
        {
            Focus = f;
            Edges = new List<HalfEdge>();
        }
        public Vertex Focus { get; }
        public List<HalfEdge> Edges { get; set; }
    }
    public class Vertex
    {
        public Vertex(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }
    }
}
