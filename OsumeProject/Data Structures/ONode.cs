using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{
    public class ONode<T>
    {
        private T value;
        public ONode<T> next;
        // constructor
        public ONode(T value)
        {
            this.value = value;
        }
        // getter and setter for value
        public T getValue()
        {
            return value;
        }
        public void setValue(T data)
        {
            value = data;
        }


    }
}