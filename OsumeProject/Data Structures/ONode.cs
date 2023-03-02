using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{
    public class ONode<T>
    {
        private T value;
        public ONode<T> next { get; set; }
        public ONode(T value)
        {
            this.value = value;
        }
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