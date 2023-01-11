using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OsumeProject
{
    public class OList<T>
    {
        private T[] elements;
        private int length;
        public OList() {
            length = 0;
            elements = new T[length];
        }
        public int getLength()
        {
            return this.length;
        }
        
        public void printAll()
        {
            foreach (T element in elements)
            {
                Trace.WriteLine(element);
            }
        }

        public void add(T element)
        {
            T[] temp = new T[length + 1];
            Array.Copy(elements, temp, length);
            temp[length] = element;
            elements = temp;
        }

        public bool contains(T element)
        {
            foreach (T e in elements)
            {
                if (EqualityComparer<T>.Default.Equals(element, e)) return true;
            }
            return false;
        }

        public T getByIndex(int index)
        {
            return elements[index];
        }

        public void delete(T element)
        {
            T[] final = new T[length - 1];
            for (int i = 0; i < length; i++)
            {
                if (EqualityComparer<T>.Default.Equals(element, elements[i]))
                {
                    T[] temp1 = new T[i];
                    Array.Copy(elements, temp1, i);
                    Array.Reverse(elements);
                    T[] temp2 = new T[length - i - 1];
                    Array.Copy(elements, temp2, length - i - 1);
                    Array.Reverse(temp2);
                    final = (T[])temp1.Concat(temp2);
                    elements = final;
                }
            }
        }
        public T[] convertToArray()
        {
            return elements;
        }
    }
}
