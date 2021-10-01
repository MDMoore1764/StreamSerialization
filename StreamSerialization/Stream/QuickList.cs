using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSerialization.Stream
{
    public class QuickList<T> : IEnumerable<T>
    {
        bool hasEnumerated;
        private IEnumerable<T> baseEnumerable;
        private readonly List<T> baseList;
        public QuickList(IEnumerable<T> e)
        {
            baseEnumerable = e;
            hasEnumerated = false;
            baseList = new List<T>();
        }

        public int Count => GetCount();

        private int GetCount()
        {
            IterateTo(-1);
            return baseList.Count;
        }

        /// <summary>
        /// Iterate to the specified position and store in memory. -1 for end of enumerator.
        /// </summary>
        /// <param name="finalPosition"></param>
        private void IterateTo(long finalPosition)
        {
            if (baseEnumerable == null || Complete())
                return;

            IEnumerator<T> enumerator = baseEnumerable.GetEnumerator();

            bool moveToEnd = finalPosition == -1;

            if (moveToEnd || baseList.Count < finalPosition)

            {
                baseList.Clear();
                ulong currentPosition = 0;
                bool canMoveNext;
                while (moveToEnd ? (canMoveNext = enumerator.MoveNext()) : currentPosition < (ulong)finalPosition && (canMoveNext = enumerator.MoveNext()))
                {
                    baseList.Add(enumerator.Current);
                    SetComplete(!canMoveNext);
                }
            }
        }

        private Task EvaluatingList;

        /// <summary>
        /// Collapse the enumerable to a memory-backed list in a background thread and continue operations.
        /// </summary>
        public void Collapse()
        {
            //EvaluatingList = Task.Run(() =>
            //{
            //    IterateTo(-1);
            //});
        }

        public T this[int index]
        {
            get
            {
                IterateTo(index);
                return baseList[index];
            }

            set => baseList[index] = value;
        }

        public int IndexOf(T item)
        {
            IterateTo(-1);
            return baseList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            IterateTo(index);
            baseList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            IterateTo(index);
            baseList.RemoveAt(index);
        }

        public void Add(T item)
        {
            if (hasEnumerated)
            {
                AddToCache(item);
            }
            baseEnumerable = baseEnumerable.Append(item);
        }

        public void Clear()
        {
            hasEnumerated = false;
            baseList.Clear();
        }

        public bool Contains(T item)
        {
            IterateTo(-1);
            return baseList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (hasEnumerated)
            {
                baseList.CopyTo(array, arrayIndex);
            }
            else
            {
                IEnumerator<T> enumerator = baseEnumerable.GetEnumerator();

                baseList.Clear();
                for (int index = 0; enumerator.MoveNext(); index++)
                {
                    if (index >= arrayIndex)
                        array[index] = enumerator.Current;

                    baseList[index] = enumerator.Current;
                }

                hasEnumerated = true;
            }
        }

        public void ForEach(Action<T> action)
        {
            foreach (T item in this)
                action.Invoke(item);
        }

        public bool Remove(T item)
        {
            int count = 0;
            do
            {
                IterateTo(count);
            }
            while (!hasEnumerated && !EqualityComparer<T>.Default.Equals(baseList[count], item));

            return baseList.Remove(item);
        }

        public bool Complete()
        {
            return hasEnumerated;
        }

        void SetComplete(bool complete)
        {
            hasEnumerated = complete;
        }

        void AddToCache(T item)
        {
            baseList.Add(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            //if(EvaluatingList != null && !EvaluatingList.IsCompleted)
            //{
            //    //EvaluatingList.RunSynchronously();
            //    EvaluatingList.Wait();
            //}

            if (!hasEnumerated)
                Clear();
            return new QuickListEnumerator(this, hasEnumerated ? baseList.GetEnumerator() : baseEnumerable.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            //if (!hasEnumerated)
            //    Clear();
            //return new QuickListEnumerator(this, hasEnumerated ? baseList.GetEnumerator() : baseEnumerable.GetEnumerator());
            return GetEnumerator();
        }

        private class QuickListEnumerator : IEnumerator<T>
        {
            private QuickList<T> list;
            private IEnumerator<T> baseEnumerator;

            public QuickListEnumerator(QuickList<T> list, IEnumerator<T> baseEnumerator)
            {
                this.list = list;
                this.baseEnumerator = baseEnumerator;
            }
            public T Current => baseEnumerator.Current;

            object IEnumerator.Current => baseEnumerator.Current;

            public bool MoveNext()
            {

                bool canMove = baseEnumerator.MoveNext();
                if (!list.Complete() && canMove)
                {
                    list.AddToCache(baseEnumerator.Current);
                }
                else if (!canMove && !list.Complete())
                {
                    list.SetComplete(true);
                }

                return canMove;
            }

            public void Reset()
            {
                list.SetComplete(false);
                baseEnumerator.Reset();
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }
        }
    }


}
