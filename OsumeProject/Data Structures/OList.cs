using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OsumeProject
{
    public class OList<T> where T : IComparable
    {
        private T[] elements;
        private int length;
        public OList() {
            length = 0;
            elements = new T[length];
        }
        public OList(T[] elements)
        {
            this.elements = elements;
            length = elements.Length;
        }

        public void quicksort(ref OList<T> list, int left, int right, bool ascending)
        {
            if (left < right)
            {
                int num = split(ref list, left, right, ascending);
                quicksort(ref list, left, num - 1, ascending);
                quicksort(ref list, num + 1, right, ascending);
            }
        }

        static private int split(ref OList<T> list, int left, int right, bool ascending)
        {
            T[] array = list.convertToArray();
            T pivot = array[right];
            T temp;
            int j = left;

            if (ascending)
            {
                for (int i = left; i < right; i++)
                {
                    if (array[i].CompareTo(pivot) <= 0)
                    {
                        temp = array[i];
                        array[i] = array[j];
                        array[j] = temp;
                        j++;
                    }
                }
            } else
            {
                for (int i = left; i < right; i++)
                {
                    if (array[i].CompareTo(pivot) >= 0)
                    {
                        temp = array[i];
                        array[i] = array[j];
                        array[j] = temp;
                        j++;
                    }
                }
            }
            array[right] = array[j];
            array[j] = pivot;
            return j; 
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
            length++;
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
