using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{
    public class OStack<T>
    {
        private OLinkedList<T> elements;
        public OStack()
        {
            elements = new OLinkedList<T>();
        }
        public int getLength()
        {
            return elements.getLength();
        }
        public void push(T item)
        {
            elements.addToStart(item);
        }
        public T pop()
        {
            T val = elements.getByIndex(0).getValue();
            elements.delete(elements.getByIndex(0).getValue());
            return val;
        }
        public T peek()
        {
            return elements.getByIndex(0).getValue();
        }
        public void printAll()
        {
            elements.printAll();
        }
    }
}
