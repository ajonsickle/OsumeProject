using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{
    public class OQueue<T>
    {
        ONode<T> front;
        ONode<T> back;
        public OQueue()
        {
            front = null;
            back = null;
        }
        public void enqueue(T data)
        {
            ONode<T> x = new ONode<T>(data);
            if (front == null)
            {
                front = x;
                back = x;
            }
            else
            {
                back.next = x;
            }
        }
        public T dequeue()
        {
            T data = default(T);
            if (front != null)
            {
                data = front.getValue();
                front = front.next;
            }
            return data;
        }
        public T peek()
        {
            if (front != null)
            {
                return front.getValue();
            }
            else return default(T);
        }
    }
}
