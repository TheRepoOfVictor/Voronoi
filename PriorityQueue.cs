using System;
using System.Collections.Generic;
using System.Text;

namespace Voronoi
{
    public class Event
    {
        public enum Type
        {
            Site,
            Circle
        }

        public Event(Node n, bool v, Type t, Node i = null, Arc a =null)
        {
            Priority = n.Y;
            Node = n;
            T = t;
            Valid = v;
            Intersection = i;
            Arc = a;
        }

        public Arc Arc { get; }
        public double Priority { get; }
        public bool Valid { get; set; }
        public Node Intersection { get; }
        public Node Node { get; set; }
        public Type T { get; set; }
    }

    class PriorityQueue
    {
        List<Event> Heap;
        public PriorityQueue()
        {
            Heap = new List<Event>();
        }
        
        public bool Empty()
        {
            return Heap.Count == 0;
        }

        public void Insert(Event val)
        {
            Heap.Add(val);
            FixUp(Heap.Count - 1);
        }

        public Event Pop()
        {
            Event first = Heap[0];
            Event last = Heap[Heap.Count - 1];
            Heap.RemoveAt(Heap.Count - 1);
            if (!Empty())
            {
                Heap[0] = last;
                FixDown(0);
            }
            return first;
        }
        
        private void FixDown(int index)
        {
            int left = 2 * index + 1;
            int right = 2 * index + 2;
            if (left >= Heap.Count && right >= Heap.Count) return;

            if(left < Heap.Count && (right >= Heap.Count || Heap[left].Priority < Heap[right].Priority) && Heap[left].Priority < Heap[index].Priority)
            {
                Event temp = Heap[left];
                Heap[left] = Heap[index];
                Heap[index] = temp;
                FixDown(left);
            }
            else if(right < Heap.Count && Heap[right].Priority < Heap[index].Priority)
            {
                Event temp = Heap[right];
                Heap[right] = Heap[index];
                Heap[index] = temp;
                FixDown(right);
            }
        }

        private void FixUp(int index)
        {
            if (index == 0) return;

            int parInd = (index - 1) / 2;
            if (Heap[parInd].Priority > Heap[index].Priority)
            {
                Event temp = Heap[index];
                Heap[index] = Heap[parInd];
                Heap[parInd] = temp;
                FixUp(parInd);
            }
        }
    }
}
