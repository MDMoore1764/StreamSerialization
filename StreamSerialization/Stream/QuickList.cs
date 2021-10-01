using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StreamSerialization.Stream
{
    public class QuickList<T> : List<T>
    {
        private bool hasEnumerated;
        private IEnumerable<T> baseEnumerable;
        public QuickList(IEnumerable<T> e)
        {
            baseEnumerable = e;
            hasEnumerated = false;
        }

        public new int Count => GetCount();

        private int GetCount()
        {
            if (!hasEnumerated)
            {
                IterateTo(-1);
            }
            return base.Count;
        }

        /// <summary>
        /// Iterate to the specified position and store in memory. -1 for end of enumerator.
        /// </summary>
        /// <param name="finalPosition"></param>
        private void IterateTo(long finalPosition)
        {
            if (baseEnumerable == null || hasEnumerated)
                return;

            IEnumerator<T> enumerator = baseEnumerable.GetEnumerator();


            bool moveToEnd = finalPosition == -1;

            if (moveToEnd || base.Count < finalPosition)
            {
                base.Clear();
                ulong currentPosition = 0;
                bool canMoveNext;
                while (moveToEnd ? (canMoveNext = enumerator.MoveNext()) : currentPosition < (ulong)finalPosition && (canMoveNext = enumerator.MoveNext()))
                {
                    base.Add(enumerator.Current);
                    hasEnumerated = !canMoveNext;
                }
            }
        }


        public new T this[int index]
        {
            get
            {
                IterateTo(index);
                return base[index];
            }

            set => base[index] = value;
        }

        public new int IndexOf(T item)
        {
            IterateTo(-1);
            return base.IndexOf(item);
        }

        public new void Insert(int index, T item)
        {
            IterateTo(index);
            base.Insert(index, item);
        }

        public new void RemoveAt(int index)
        {
            IterateTo(index);
            base.RemoveAt(index);
        }

        public new void Add(T item)
        {
            baseEnumerable = baseEnumerable.Append(item);
        }

        public new void Clear()
        {
            base.Clear();
            baseEnumerable = null;
        }

        public new bool Contains(T item)
        {
            IterateTo(-1);
            return base.Contains(item);
        }

        public new void CopyTo(T[] array, int arrayIndex)
        {
            if (hasEnumerated)
            {
                base.CopyTo(array, arrayIndex);
            }
            else
            {
                IEnumerator<T> enumerator = baseEnumerable.GetEnumerator();

                base.Clear();
                for (int index = 0; enumerator.MoveNext(); index++)
                {
                    if (index >= arrayIndex)
                        array[index] = enumerator.Current;

                    base[index] = enumerator.Current;
                }

                hasEnumerated = true;
            }
        }

        public new bool Remove(T item)
        {
            int count = 0;
            do
            {
                IterateTo(count);
            }
            while (!hasEnumerated && !EqualityComparer<T>.Default.Equals(base[count], item));

            return base.Remove(item);
        }

        public new IEnumerator<T> GetEnumerator()
        {
            return new QuickListEnumerator(ref hasEnumerated, this, baseEnumerable.GetEnumerator());
        }

        private class QuickListEnumerator : IEnumerator<T>
        {
            private int position;
            private bool hasEnumerated;
            private List<T> baseList;
            private IEnumerator<T> baseEnumerator;

            public QuickListEnumerator(ref bool hasEnumerated, List<T> baseList, IEnumerator<T> baseEnumerator)
            {
                this.hasEnumerated = hasEnumerated;
                this.baseList = baseList;
                this.baseEnumerator = baseEnumerator;
            }
            public T Current => baseEnumerator.Current;

            object IEnumerator.Current => baseEnumerator.Current;

            public bool MoveNext()
            {
                bool canMove = baseEnumerator.MoveNext();
                if (canMove )
                {
                    position++;
                    if(baseList.Count < position)
                        baseList.Add(baseEnumerator.Current);
                }

                return canMove;
            }

            public void Reset()
            {
                baseEnumerator.Reset();
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }
        }
    }


}
