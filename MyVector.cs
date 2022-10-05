using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MyVector
{
    public class NotSupportedInterfaceException : Exception
    {
        public NotSupportedInterfaceException() { }

        public NotSupportedInterfaceException(string? message) : base(message) { }

        public NotSupportedInterfaceException(string? message, Exception? innerException) : base(message, innerException) { }

        protected NotSupportedInterfaceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    public class Vector<T> : IEquatable<Vector<T>>, ICloneable, IComparable<Vector<T>>
    {
        protected T[] vec;
        protected int sz;

        protected static void CopyArray(T[] source, T[] destination, int sz)
        {
            for (int i = 0; i < sz; ++i)
            {
                if (source[i] is ICloneable cloneable)
                {
                    destination[i] = (T)cloneable.Clone();
                }
                else
                {
                    destination[i] = source[i];
                }
            }
        }

        protected void ChangeCapacity(int cap)
        {
            T[] temp = new T[cap];
            Array.Copy(vec, temp, sz);
            vec = temp;
        }

        public Vector()
        {
            vec = new T[2];
            sz = 0;
        }

        public Vector(T[] arr)
        {
            int cap = 0;
            for (; cap < arr.Length; cap <<= 1) { }
            vec = new T[cap];
            sz = arr.Length;
            CopyArray(arr, vec, sz);
        }

        public Vector(Vector<T> vector)
        {
            vec = new T[vector.vec.Length];
            sz = vector.sz;
            CopyArray(vector.vec, vec, sz);
        }

        public Vector(int sz)
        {
            int cap = 2;
            for (; cap < sz; cap <<= 1) { }
            this.sz = sz;
            vec = new T[cap];
        }

        public Vector(int sz, ref T el)
        {
            int cap = 2;
            for (; cap < sz; cap <<= 1) { }
            vec = new T[cap];
            this.sz = sz;
            if (el is ICloneable cloneable)
            {
                for (int i = 0; i < sz; ++i)
                {
                    vec[i] = (T)cloneable.Clone();
                }
            }
            else
            {
                for (int i = 0; i < sz; ++i)
                {
                    vec[i] = el;
                }
            }
        }

        public Vector(int sz, T el) : this(sz, ref el) { }

        public void PushBack(ref T el)
        {
            if (sz == vec.Length)
            {
                ChangeCapacity(2 * sz);
            }
            if (el is ICloneable cloneable)
            {
                vec[sz++] = (T)cloneable.Clone();
            }
            else
            {
                vec[sz++] = el;
            }
        }

        public void PushBack(T el)
        {
            PushBack(ref el);
        }

        public void PopBack()
        {
            if (sz != 0)
            {
                --sz;
                if (sz < vec.Length / 4)
                {
                    ChangeCapacity(vec.Length / 2);
                }
            }
        }

        public int Size()
        {
            return sz;
        }

        public int Capacity()
        {
            return vec.Length;
        }

        public void ShrinkToFit()
        {
            ChangeCapacity(sz);
        }

        public void Reserve(int capacity)
        {
            if (capacity > vec.Length)
            {
                ChangeCapacity(capacity);
            }
        }

        public void Clear()
        {
            vec = new T[2];
            sz = 0;
        }

        public int Find(ref T el)
        {
            if (sz == 0)
            {
                return -1;
            }
            if (el is IEquatable<T> equatable)
            {
                for (int i = 0; i < sz; ++i)
                {
                    if (equatable.Equals(vec[i]))
                    {
                        return i;
                    }
                }
            }
            else
            {
                if (vec[0] == null)
                {
                    throw new ArgumentNullException();
                }
                else
                {
                    throw new NotSupportedInterfaceException("Class not supported IEquatable interface");
                }
            }
            return -1;
        }

        public int Find(T el)
        {
            return Find(ref el);
        }

        protected static void Copy(T[] source, T[] destination, int start, int sz)
        {
            for (int i = 0; i < sz; ++i)
            {
                destination[start + i] = source[i];
            }
        }

        protected void Merge(int start, int mid, int end, Func<T, T, bool> IsLower)
        {
            T[] temp = new T[end - start + 1];
            int midCp = mid;
            for (int i = 0; i < temp.Length; ++i)
            {
                if (mid == end + 1 || start < midCp && IsLower(vec[start], vec[mid]))
                {
                    temp[i] = vec[start++];
                }
                else
                {
                    temp[i] = vec[mid++];
                }
            }
            Copy(temp, vec, end - temp.Length + 1, temp.Length);
        }

        public void Sort(Func<T, T, bool> IsLower, int start = 0, int end = -2)
        {
            if (end == -2)
            {
                end = sz - 1;
            }
            if (end <= start)
            {
                return;
            }
            if (end == start + 1)
            {
                if (!IsLower(vec[start], vec[end]))
                {
                    (vec[start], vec[end]) = (vec[end], vec[start]);
                }
                return;
            }
            int mid = (start + end) / 2;
            Sort(IsLower, start, mid);
            Sort(IsLower, mid + 1, end);

            Merge(start, mid + 1, end, IsLower);
        }

        public void Sort()
        {
            if (sz == 0)
            {
                return;
            }
            if (vec[0] is IComparable)
            {
                static bool isLower(T a, T b)
                {
                    if (a == null || b == null)
                    {
                        throw new ArgumentNullException();
                    }
                    return ((IComparable)(a)).CompareTo(b) < 0;
                }
                Sort(isLower);
            }
            else
            {
                if (vec[0] != null)
                {
                    throw new NotSupportedInterfaceException("Class not supported IComparable interface");
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
        }

        bool IEquatable<Vector<T>>.Equals(Vector<T>? other)
        {
            if (other == null)
            {
                throw new ArgumentNullException();
            }
            if (other.Size() == 0 && Size() == 0)
            {
                return true;
            }
            if (other.Size() != Size())
            {
                return false;
            }
            if (vec[0] is not IEquatable<Vector<T>>)
            {
                if (vec[0] == null)
                {
                    throw new ArgumentNullException();
                }
                throw new NotSupportedInterfaceException("Class not supported IEquatable interface");
            }
            /*
            for (int i = 0; i < Size(); ++i)
            {
                if (!((IEquatable<Vector<T>>)(vec[i])).Equals(other.vec[i]))
                {
                    return false;
                }
            }
            */
            return true;
        }


        public override string ToString()
        {
            if (sz == 0)
            {
                return "[]";
            }
            string result = "[";
            for (int i = 0; i < sz - 1; ++i)
            {
                if (vec[i] != null)
                {
                    result += vec[i].ToString() + ", ";
                }
                else
                {
                    result += "Null, ";
                }
            }
            if (vec[sz - 1] != null)
            {
                result += vec[sz - 1].ToString() + "]";
            }
            else
            {
                result += "Null]";
            }
            return result;
        }

        object ICloneable.Clone()
        {
            Vector<T> result = new(this);
            return result;
        }

        int IComparable<Vector<T>>.CompareTo(Vector<T>? other)
        {
            if (other == null)
            {
                throw new ArgumentNullException();
            }
            if (other.sz == 0 && sz == 0)
            {
                return 0;
            }
            if (sz == 0)
            {
                return -1;
            }
            if (other.sz == 0)
            {
                return 1;
            }
            if (vec[0] is not IComparable<T>)
            {
                if (vec[0] == null)
                {
                    throw new ArgumentNullException();
                }
                throw new NotSupportedInterfaceException("Class  not supported IComparable interface");
            }
            return 1;
            // Not realized yet
        }

        public T this[int ind]
        {
            get
            {
                return vec[ind];
            }
            set
            {
                if (value is ICloneable cloneable)
                {
                    vec[ind] = (T)cloneable.Clone();
                }
                else
                {
                    vec[ind] = value;
                }
            }
        }

        public override bool Equals(object? obj)
        {
            return ((IEquatable<Vector<T>>)this).Equals(obj as Vector<T>);
        }

        public override int GetHashCode()
        {
            return vec.GetHashCode();
        }

        public static explicit operator T[](Vector<T> v)
        {
            T[] result = new T[v.Size()];
            CopyArray(v.vec, result, v.Size());
            return result;
        }
    }

}
