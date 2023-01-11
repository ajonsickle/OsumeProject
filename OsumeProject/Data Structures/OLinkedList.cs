using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Markup;
using System.Xml.Linq;

namespace OsumeProject
{
    public class OLinkedList<T>
    {
        private ONode<T> firstElement;
        private int length;
        public int getLength()
        {
            return length;
        }
        public OLinkedList()
        {
            length = 0;
            firstElement = null;
        }
        public void printAll()
        {
            ONode<T> x = firstElement;

            while (x != null)
            {

                Trace.WriteLine(x.getValue());

                x = x.next;
            }

        }
        public void addToEnd(T data)
        {
            ONode<T> a = new ONode<T>(data);
            ONode<T> x = firstElement;
            if (x == null) firstElement = a;
            else
            {
                while (x.next != null)
                {
                    x = x.next;
                }
                x.next = a;
            }
            length += 1;

        }
        public void addToStart(T data)
        {
            ONode<T> x = new ONode<T>(data);
            x.next = firstElement;
            firstElement = x;
            length += 1;
        }
        public void delete(T data)
        {
            ONode<T> x = firstElement;
            if (x != null)
            {
                if (EqualityComparer<T>.Default.Equals(x.getValue(), data))
                {
                    if (x.next != null) x = x.next;
                    else x = null;
                    firstElement = x;
                    length -= 1;
                    return;
                }
            }
            while (x.next != null && !EqualityComparer<T>.Default.Equals(x.next.getValue(), data))
            {
                x = x.next;
            }
            if (x.next != null && EqualityComparer<T>.Default.Equals(x.next.getValue(), data))
            {
                x.next = x.next.next;
                x = null;
                length -= 1;
            }
            else
            {
                Console.WriteLine("Data not found");
            }
        }
        public ONode<T> getByIndex(int index)
        {
            ONode<T> x = firstElement;
            if (index == 0)
            {
                return x;
            }
            if (x != null)
            {
                int i = 0;
                while (x.next != null && i <= index)
                {
                    x = x.next;
                    i++;
                }
                return x;
            }
            else throw new Exception("List empty");
        }

        public OLinkedList<T> concatenateLists(OLinkedList<T> b)
        {
            OLinkedList<T> final = new OLinkedList<T>();
            ONode<T> aElement = this.firstElement;
            ONode<T> bElement = b.firstElement;
            while (aElement != null)
            {
                final.addToEnd(aElement.getValue());
                aElement = aElement.next;
            }
            while (bElement != null)
            {
                final.addToEnd(bElement.getValue());
                bElement = bElement.next;
            }

            return final;
        }

        public bool contains(T value)
        {
            ONode<T> x = this.firstElement;
            while (x != null)
            {
                if (EqualityComparer<T>.Default.Equals(x.getValue(), value)) return true;
                x = x.next;
            }
            return false;
        }

        public T[] convertToArray()
        {
            T[] array = new T[this.getLength()];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this.getByIndex(i).getValue();
            }
            return array;
        }

    }
}