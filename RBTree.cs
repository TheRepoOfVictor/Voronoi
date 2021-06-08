using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Voronoi
{
    public class RBTree
    {
        private enum Colour
        {
            Red,
            Black,
            DoubleBlack,
        }

        private class Node
        {
            private Colour colour;
            private Arc val;
            private Node left;
            private Node right;
            private Node parent;

            public Node(Arc i, Node p, Colour col)
            {
                val = i;
                colour = col;
                left = null;
                right = null;
                parent = p;
            }

            public Node Left
            {
                get { return left; }
                set { left = value; }
            }

            public Node Right
            {
                get { return right; }
                set { right = value; }
            }

            public Node Parent
            {
                get { return parent; }
                set { parent = value; }
            }

            public Arc Val
            {
                get { return val; }
                set { val = value; }
            }

            public Colour Colour
            {
                get { return colour; }
                set { colour = value; }
            }

            public void Print(int indents)
            {
                for (int i=0; i < indents; i++)
                {
                    Debug.Write("  ");
                }
                Debug.WriteLine("|-- [" + "(" + val.Focus.X + "," + val.Focus.Y + ")" + " , " + colour + "]");

                if (left != null)
                {
                    left.Print(indents + 1);
                }
                else
                {
                    for (int i = 0; i < indents + 1; i++)
                    {
                        Debug.Write("  ");
                    }
                    Debug.WriteLine("|-- [null]");
                }

                if (right != null)
                {
                    right.Print(indents + 1);
                }
                else
                {
                    for (int i = 0; i < indents + 1; i++)
                    {
                        Debug.Write("  ");
                    }
                    Debug.WriteLine("|-- [null]");
                }
            }
        }

        private Node root;
        public RBTree()
        {
            root = null;
        }

        public void Insert(Arc value)
        {
            if (root == null)
            {
                root = new Node(value, null, Colour.Black);
            }
            else
            {
                Insert(root, value);
            }
            //PrintTree();
        }

        public void Delete(Arc value)
        {
            Delete(root, value);
            //PrintTree();
        }

        private Arc Find(Node r, double x)
        {
            if(r == null) return null;
            Arc arc = r.Val;
            if (x > VDiagram.GetCurrentRightIntersectionOfArc(arc))
            {
                return Find(r.Right, x);
            }
            else if (x < VDiagram.GetCurrentLeftIntersectionOfArc(arc))
            {
                return Find(r.Left, x);
            }
            return arc;
        }

        public Arc Find(double x)
        {
            return Find(root, x);
        }

        public Arc LeftMost()
        {
            return LeftMost(root);
        }

        private Arc LeftMost(Node r)
        {
            if(r.Left == null)
            {
                return r.Val;
            }
            return LeftMost(r.Left);
        }

        private void Delete(Node r, Arc value)
        {
            if (r == null)
            {
                return;
            }

            if (r.Val > value)
            {
                Delete(r.Left, value);
            }
            else if (r.Val < value)
            {
                Delete(r.Right, value);
            }
            else
            {
                if (r.Left == null)
                {
                    if (r.Right != null)
                    {
                        if (r.Colour == Colour.Red ||r.Right.Colour == Colour.Red)
                        {
                            r.Right.Colour = Colour.Black;
                        }
                        else
                        {
                            r.Right.Colour = Colour.DoubleBlack;
                        }
                    }

                    if (IsRoot(r))
                    {
                        root = r.Right;
                        if (r.Right != null)
                        {
                            r.Right.Parent = null;
                            r.Right.Colour = Colour.Black;
                        }
                    }
                    else
                    {
                        bool leftChild = IsLeftChild(r);
                        if (leftChild)
                        {
                            r.Parent.Left = r.Right;
                        }
                        else
                        {
                            r.Parent.Right = r.Right;
                        }

                        if (r.Right != null) 
                            r.Right.Parent = r.Parent;
                        if(r.Colour == Colour.Black)
                            FixDoubleBlack(r.Parent, r.Right, leftChild);
                    }
                }
                else if (r.Right == null)
                {
                    if (r.Left != null)
                    {
                        if (r.Colour == Colour.Red || r.Left.Colour == Colour.Red)
                        {
                            r.Left.Colour = Colour.Black;
                        }
                        else
                        {
                            r.Left.Colour = Colour.DoubleBlack;
                        }
                    }

                    if (IsRoot(r))
                    {
                        root = r.Left;
                        if (r.Left != null)
                        {
                            r.Left.Parent = null;
                            r.Left.Colour = Colour.Black;
                        }
                    }
                    else
                    {
                        bool leftChild = IsLeftChild(r);
                        if (leftChild)
                        {
                            r.Parent.Left = r.Left;
                        }
                        else
                        {
                            r.Parent.Right = r.Left;
                        }

                        if(r.Left != null) 
                            r.Left.Parent = r.Parent;

                        if (r.Colour == Colour.Black)
                            FixDoubleBlack(r.Parent, r.Left, leftChild);
                    }
                }
                else
                {
                    r.Val = InorderSuccessorVal(r.Right);
                    Delete(r.Right, r.Val);
                }
            }
        }

        private void FixDoubleBlack(Node Parent, Node Child, bool isLeftChild)
        {
            if (Child != null && IsRoot(Child))
            {
                Child.Colour = Colour.Black;
                return;
            }

            if (Child != null && Child.Colour != Colour.DoubleBlack) 
                return;

            if (Child != null)
                Child.Colour = Colour.Black;

            if (isLeftChild)
            {
                Node RightChild = Parent.Right;

                Node LeftNephew = null;
                if (RightChild != null && RightChild.Left != null && RightChild.Left.Colour == Colour.Red)
                    LeftNephew = RightChild.Left;

                Node RightNephew = null;
                if (RightChild != null && RightChild.Right != null && RightChild.Right.Colour == Colour.Red)
                    RightNephew = RightChild.Right;

                if (LeftNephew != null || RightNephew != null)
                {
                    if (RightNephew != null)
                    {
                        LeftRotate(Parent);
                        RightNephew.Colour = Colour.Black;
                    }
                    else
                    {
                        RightRotate(RightChild);
                        LeftRotate(Parent);
                        LeftNephew.Colour = Colour.Black;
                    }
                }
                else if (RightChild == null || RightChild.Colour == Colour.Black)
                {

                    if (RightChild != null)
                        RightChild.Colour = Colour.Red;

                    if (Parent.Colour == Colour.Red)
                    {
                        Parent.Colour = Colour.Black;
                    }
                    else
                    {
                        if (!IsRoot(Parent))
                        {
                            Parent.Colour = Colour.DoubleBlack;
                            FixDoubleBlack(Parent.Parent, Parent, IsLeftChild(Parent));
                        }
                        else
                        {
                            Parent.Colour = Colour.Black;
                        }
                    }
                }
                else
                {
                    LeftRotate(Parent);
                    RightChild.Colour = Colour.Black;
                    Parent.Colour = Colour.Red;
                    FixDoubleBlack(Parent, Child, isLeftChild);
                }
            }
            else
            {
                Node LeftChild = Parent.Left;

                Node LeftNephew = null;
                if (LeftChild != null && LeftChild.Left != null && LeftChild.Left.Colour == Colour.Red)
                    LeftNephew = LeftChild.Left;

                Node RightNephew = null;
                if (LeftChild != null && LeftChild.Right != null && LeftChild.Right.Colour == Colour.Red)
                    RightNephew = LeftChild.Right;

                if (LeftNephew != null || RightNephew != null)
                {
                    if (LeftNephew != null)
                    {
                        RightRotate(Parent);
                        LeftNephew.Colour = Colour.Black;
                    }
                    else
                    {
                        LeftRotate(LeftChild);
                        RightRotate(Parent);
                        RightNephew.Colour = Colour.Black;
                    }
                }
                else if (LeftChild == null || LeftChild.Colour == Colour.Black)
                {

                    if (LeftChild != null)
                        LeftChild.Colour = Colour.Red;

                    if (Parent.Colour == Colour.Red)
                    {
                        Parent.Colour = Colour.Black;
                    }
                    else
                    {
                        if (!IsRoot(Parent))
                        {
                            Parent.Colour = Colour.DoubleBlack;
                            FixDoubleBlack(Parent.Parent, Parent, IsLeftChild(Parent));
                        }
                        else
                        {
                            Parent.Colour = Colour.Black;
                        }
                    }
                }
                else
                {
                    RightRotate(Parent);
                    LeftChild.Colour = Colour.Black;
                    Parent.Colour = Colour.Red;
                    FixDoubleBlack(Parent, Child, isLeftChild);
                }
            }
        }

        private Arc InorderSuccessorVal(Node r)
        {
            if (r.Left == null) return r.Val;
            else return InorderSuccessorVal(r.Left);
        }

        private void Insert(Node pn, Arc value)
        {
            if (pn.Val > value)
            {
                if (pn.Left == null)
                {
                    pn.Left = new Node(value, pn, Colour.Red);
                    if(!IsRoot(pn))
                    {
                        DetectAndFixViolation(pn.Parent, pn, pn.Left);
                    }
                    return;
                }
                Insert(pn.Left, value);
            }
            else
            {
                if (pn.Right == null)
                {
                    pn.Right = new Node(value, pn, Colour.Red);
                    if (!IsRoot(pn))
                    {
                        DetectAndFixViolation(pn.Parent, pn, pn.Right);
                    }
                    return;
                }
                Insert(pn.Right, value);
            }
        }

        private bool IsLeftChild(Node n)
        {
            if (n.Parent.Left == null) return false;
            return n.Parent.Left.GetHashCode() == n.GetHashCode();
        }

        private bool IsRoot(Node n)
        {
            return n.Parent == null;
        }

        private void LeftRotate(Node n)
        {
            Node child = n.Right;
            Node parent = n.Parent;
            n.Right = child.Left;
            if (n.Right != null)
            {
                n.Right.Parent = n;
            }

            if (parent == null)
            {
                root = child;
                child.Parent = null;
            }
            else if (IsLeftChild(n))
            {
                parent.Left = child;
                child.Parent = parent;
            }
            else
            {
                parent.Right = child;
                child.Parent = parent;
            }
            child.Left = n;
            n.Parent = child;
        }

        private void RightRotate(Node n)
        {
            Node child = n.Left;
            Node parent = n.Parent;
            n.Left = child.Right;
            if (n.Left != null)
            {
                n.Left.Parent = n;
            }

            if (parent == null)
            {
                root = child;
                child.Parent = null;
            }
            else if (IsLeftChild(n))
            {
                parent.Left = child;
                child.Parent = parent;
            }
            else
            {
                parent.Right = child;
                child.Parent = parent;
            }

            child.Right = n;
            n.Parent = child;
        }

        private void DetectAndFixViolation(Node gparent, Node parent, Node child)
        {
            if (parent.Colour == Colour.Black) return;
            if (IsLeftChild(parent))
            {
                Node uncle = gparent.Right;
                if(uncle != null && uncle.Colour == Colour.Red)
                {
                    parent.Colour = uncle.Colour = Colour.Black;
                    if(!IsRoot(gparent)) gparent.Colour = Colour.Red;
                    if (!IsRoot(gparent) && !IsRoot(gparent.Parent))
                    {
                        DetectAndFixViolation(gparent.Parent.Parent, gparent.Parent, gparent);
                    }
                }
                else
                {
                    if (!IsLeftChild(child))
                    {
                        LeftRotate(parent);
                        Swap(ref child, ref parent);
                    }
                    RightRotate(gparent);
                    Colour temp = gparent.Colour;
                    gparent.Colour = parent.Colour;
                    parent.Colour = temp;
                }
            }
            else
            {
                Node uncle = gparent.Left;
                if (uncle != null && uncle.Colour == Colour.Red)
                {
                    parent.Colour = uncle.Colour = Colour.Black;
                    if (!IsRoot(gparent)) gparent.Colour = Colour.Red;
                    if (!IsRoot(gparent) && !IsRoot(gparent.Parent))
                    {
                        DetectAndFixViolation(gparent.Parent.Parent, gparent.Parent, gparent);
                    }
                }
                else
                {
                    if (IsLeftChild(child))
                    {
                        RightRotate(parent);
                        Swap(ref child, ref parent);
                    }
                    LeftRotate(gparent);
                    Colour temp = gparent.Colour;
                    gparent.Colour = parent.Colour;
                    parent.Colour = temp;
                }
            }
        }

        public void PrintTree()
        {
            if (root == null)
            {
                Debug.WriteLine("Empty Tree");
                Debug.WriteLine("____________________________");
            }
            else
            {
                root.Print(0);
                Debug.WriteLine("____________________________");
            }
        }
        static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}
