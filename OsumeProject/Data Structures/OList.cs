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

        public OList<T> sort(OList<T> list)
        {
            T[] array = list.convertToArray();
            int mid = array.Length / 2;
            T[] left = new T[mid];
            T[] right = new T[array.Length - mid];
            if (array.Length <= 1) return new OList<T>(array);
            Array.Copy(array, left, mid);
            Array.Reverse(array);
            Array.Copy(array, right, array.Length - mid);
            Array.Reverse(array);
            left = sort(new OList<T>(left)).convertToArray();
            right = sort(new OList<T>(right)).convertToArray();
            T[] combined = new T[left.Length + right.Length];
            int indexL = 0;
            int indexR = 0;
            int indexF = 0;
            while (indexL < left.Length || indexR < right.Length)
            {
                if (indexL < left.Length && indexR < right.Length)
                {
                    if (left[indexL].CompareTo(right[indexR]) <= 0)
                    {
                        combined[indexF] = left[indexL];
                        indexL++;
                        indexF++;
                    } else
                    {
                        combined[indexF] = right[indexR];
                        indexR++;
                        indexF++;
                    }

                } else if (indexL < left.Length)
                {
                    combined[indexF] = left[indexL];
                    indexL++;
                    indexF++;
                } else if (indexR < right.Length)
                {
                    combined[indexF] = right[indexR];
                    indexR++;
                    indexF++;
                }
            }
            return new OList<T>(combined);

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
