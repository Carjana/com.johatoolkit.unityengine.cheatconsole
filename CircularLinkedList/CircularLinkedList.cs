namespace CircularLinkedList
{
    public class CircularLinkedList<T>
    {
        private class Element
        {
            public T Content;
            public Element Next;
        }
        
        private Element _first;
        private Element _last;
        public int MaxCount { get; }
        public int Count { get; private set; }

        public CircularLinkedList(int maxCount)
        {
            MaxCount = maxCount;
            Count = 0;
        }
        
        public void Add(T content)
        {
            Count++;
            if (_first == null)
            {
                _first = new Element { Content = content };
                _last = _first;
                _first.Next = _first;
                return;
            }

            if (_first == _last)
            {
                _last = new Element { Content = content };
                _first.Next = _last;
                _last.Next = _first;
                return;
            }

            Element newElement = new() { Content = content };

            _last.Next = newElement;
            _last = newElement;
            
            if (Count > MaxCount)
            {
                _last.Next = _first.Next;
                _first = _first.Next;
                Count--;
            }
            else
            {
                _last.Next = _first;
            }
        }

        public void Clear()
        {
            _first = null;
            _last = null;
            Count = 0;
        }
        
        public T Get(int index)
        {
            if(index < 0)
                throw new System.IndexOutOfRangeException();
            
            Element current = _first;
            for (int i = 0; i < index; i++)
            {
                current = current.Next;
            }

            return current.Content;

        }
    }
}
