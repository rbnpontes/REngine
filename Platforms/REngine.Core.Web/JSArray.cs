using System.Collections;

namespace REngine.Core.Web;

public sealed partial class JSArray : IList, IDisposable
{
    private readonly int pArray;
    
    public bool IsFixedSize => false;
    public bool IsReadOnly => false;

    public object? this[int index]
    {
        get
        {
            ObjectDisposedException.ThrowIf(pDisposed, this);
            return js_array_get(pArray, index);
        }
        set
        {
            ObjectDisposedException.ThrowIf(pDisposed, this);
            js_array_set(pArray, index, WebMarshal.CreateJsObject(value));
        } 
    }

    public int Count
    {
        get
        {
            ObjectDisposedException.ThrowIf(pDisposed, this);
            return js_array_length(pArray);
        }
    }
    public int Length => Count;
    
    public bool IsSynchronized { get; } = false;
    public object SyncRoot { get; } = false;

    public JSArray()
    {
        pArray = js_array_new();
    }
    internal JSArray(int arrayId)
    {
        pArray = arrayId;
    }

    ~JSArray()
    {
        js_array_free(pArray);
    }
    
    private bool pDisposed;
    public void Dispose()
    {
        if (pDisposed)
            return;
        pDisposed = true;
        js_array_free(pArray);
        GC.SuppressFinalize(this);
    }
    public int Add(object? value)
    {
        ObjectDisposedException.ThrowIf(pDisposed, this);
        var idx = Count;
        js_array_push(pArray, WebMarshal.CreateJsObject(value));
        return idx;
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(pDisposed, this);
        js_array_clear(pArray);
    }

    public bool Contains(object? value)
    {
        ObjectDisposedException.ThrowIf(pDisposed, this);
        return IndexOf(value) != -1;
    }

    public int IndexOf(object? value)
    {
        ObjectDisposedException.ThrowIf(pDisposed, this);
        return js_array_indexof(pArray, value);
    }

    public void Insert(int index, object? value)
    {
        ObjectDisposedException.ThrowIf(pDisposed, this);
        js_array_insert(pArray, index, WebMarshal.CreateJsObject(value));
    }

    public void Remove(object? value)
    {
        ObjectDisposedException.ThrowIf(pDisposed, this);
        RemoveAt(IndexOf(value));
    }

    public void RemoveAt(int index)
    {
        ObjectDisposedException.ThrowIf(pDisposed, this);
        js_array_remove(pArray, index);
    }
    
    public IEnumerator GetEnumerator()
    {
        ObjectDisposedException.ThrowIf(pDisposed, this);
        for (var i = 0; i < Count; ++i)
            yield return this[i];
    }

    public void CopyTo(Array array, int index)
    {
        ObjectDisposedException.ThrowIf(pDisposed, this);
        for (var idx = index; idx < Count; idx++)
            array.SetValue(this[idx], idx);
    }
    
    public object GetJsObject()
    {
        ObjectDisposedException.ThrowIf(pDisposed, this);
        return js_array_get_native(pArray);
    }

    internal int GetId()
    {
        return pArray;
    }
}