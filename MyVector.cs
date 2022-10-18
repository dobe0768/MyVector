using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data.Common;
using System.Collections;
using System.Runtime.CompilerServices;

class IsReadOnlyException : Exception { }

interface IVector<T> : System.Collections.Generic.ICollection<T>
{

    public void PopBack();

    public int Capacity();

    public void ChangeCapacity(int newCapacity);

    public void ShrinkToFit();

    public int Size();

    public T this[int index]
    {
        get;
        set;
    }
}

class VectorEnum<T> : IEnumerator, IEnumerator<T>
{
    private readonly Vector<T> vec;
    private int index = 0;

    public VectorEnum(Vector<T> vec)
    {
        this.vec = vec;
    }

    public object Current => vec[index];

    T IEnumerator<T>.Current => vec[index];

    public void Dispose() { }

    public bool MoveNext()
    {
        if (index++ == vec.Size())
        {
            return false;
        }
        return true;
    }

    public void Reset()
    {
        index = 0;
    }
}

class Vector<T> : IVector<T>
{
    private T[] vec;
    private int sz;
    bool readOnly;

    public Vector()
    {
        vec = Array.Empty<T>();
        sz = 0;
        readOnly = false;
    }

    public Vector(Vector<T> other)
    {
        vec = new T[other.vec.Length];
        sz = other.sz;
        Array.Copy(other.vec, vec, sz);
        readOnly = other.readOnly;
    }

    public Vector(int sz)
    {
        this.sz = sz;
        vec = new T[sz];
        readOnly = false;
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= sz)
            {
                throw new IndexOutOfRangeException();
            }
            return vec[index];
        }
        set
        {
            if (IsReadOnly)
            {
                throw new IsReadOnlyException();
            }
            if (index < 0 || index >= sz)
            {
                throw new IndexOutOfRangeException();
            }
            vec[index] = value;
        }
    }

    public int Count
    {
        get => sz;
    }

    public bool IsReadOnly
    {
        get => readOnly;
        set => readOnly = value;
    }

    public void Add(Vector<T> other)
    {
        for (int i= 0; i < other.Size(); ++i)
        {
            Add(other[i]);
        }
    }

    public void Add(T item)
    {
        if (IsReadOnly)
        {
            throw new IsReadOnlyException();
        }
        if (sz == vec.Length)
        {
            ChangeCapacity(2 * sz > 1 ? 2 * sz : 1);
        }
        vec[sz++] = item;
    }

    public int Capacity()
    {
        return vec.Length;
    }

    public void ChangeCapacity(int newCapacity)
    {
        if (newCapacity < sz)
        {
            throw new IndexOutOfRangeException();
        }
        T[] newVec = new T[newCapacity];
        Array.Copy(vec, newVec, sz);
        vec = newVec;
    }

    public void Clear()
    {
        if (readOnly)
        {
            throw new IsReadOnlyException();
        }
        vec = Array.Empty<T>();
        sz = 0;
    }

    public bool Contains(T item, Comparer<T> comparer)
    {
        for (int i = 0; i < sz; ++i)
        {
            if (comparer.Compare(item, vec[i]) == 0)
            {
                return true;
            }
        }
        return false;
    }

    public bool Contains(T item)
    {
        return Contains(item, Comparer<T>.Default);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        Array.Copy(vec, 0, array, arrayIndex, sz);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new VectorEnum<T>(this);
    }

    public void PopBack()
    {
        if (readOnly)
        {
            throw new IsReadOnlyException();
        }
        if (sz == 0)
        {
            throw new IndexOutOfRangeException();
        }
        --sz;
        if (vec.Length / 4 > sz)
        {
            ChangeCapacity(vec.Length / 2);
        }
    }

    private void Remove(int index)
    {
        --sz;
        T[] newVec = new T[sz < vec.Length / 4 ? vec.Length / 2 : vec.Length];
        int c = 0;
        for (int i = 0; i < sz + 1; ++i)
        {
            if (i != index)
            {
                c = 1;
                continue;
            }
            newVec[i - c] = vec[i];
        }
        vec = newVec;
    }

    public bool Remove(T item, Comparer<T> comparer)
    {
        if (readOnly)
        {
            throw new IsReadOnlyException();
        }
        for (int i = 0; i < sz; ++i)
        {
            if (comparer.Compare(item, vec[i]) == 0)
            {
                Remove(i);
                return true;
            }
        }
        return false;
    }

    public bool Remove(T item)
    {
        return Remove(item, Comparer<T>.Default);
    }

    public void ShrinkToFit()
    {
        ChangeCapacity(sz);
    }

    public int Size()
    {
        return sz;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new VectorEnum<T>(this);
    }

    public static explicit operator T[](Vector<T> vec)
    {
        T[] result = new T[vec.sz];
        Array.Copy(vec.vec, result, vec.sz);
        return result;
    }
}
