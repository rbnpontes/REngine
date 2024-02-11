using System.Collections;
using REngine.Core.Exceptions;

namespace REngine.Core.Web;

public sealed partial class JSArray : IList, IDisposable, IJavaScriptContract
{
    private readonly int pArray;
    
    public bool IsFixedSize => false;
    public bool IsReadOnly => false;

    public object? this[int index]
    {
        get
        {
            ObjectDisposedException.ThrowIf(pDisposed, this);
#if WEB
            return js_array_get(pArray, index);
#else
            throw new RequiredPlatformException(PlatformType.Web);
#endif
        }
        set
        {
            ObjectDisposedException.ThrowIf(pDisposed, this);
#if WEB
            js_array_set(pArray, index, WebMarshal.CreateJsObject(value));
#else
            throw new RequiredPlatformException(PlatformType.Web);
#endif
        } 
    }

    public int Count
    {
        get
        {
#if WEB
            ObjectDisposedException.ThrowIf(pDisposed, this);
            return js_array_length(pArray);
#else
            throw new RequiredPlatformException(PlatformType.Web);
#endif
        }
    }
    public int Length => Count;
    
    public bool IsSynchronized { get; } = false;
    public object SyncRoot { get; } = false;

    public JSArray()
    {
#if WEB
        pArray = js_array_new();
#endif
    }
    internal JSArray(int arrayId)
    {
        pArray = arrayId;
    }

#if WEB
    ~JSArray()
    {
        js_array_free(pArray);
    }
    
    private bool pDisposed;
#endif
    public void Dispose()
    {
#if WEB
        if (pDisposed)
            return;
        pDisposed = true;
        js_array_free(pArray);
        GC.SuppressFinalize(this);
#endif
    }
    public int Add(object? value)
    {
#if WEB
        ObjectDisposedException.ThrowIf(pDisposed, this);
        var idx = Count;
        js_array_push(pArray, WebMarshal.CreateJsObject(value));
        return idx;
#else
        throw new RequiredPlatformException(PlatformType.Web); 
#endif
    }

    public void Clear()
    {
#if WEB
        ObjectDisposedException.ThrowIf(pDisposed, this);
        js_array_clear(pArray);
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public bool Contains(object? value)
    {
#if WEB
        ObjectDisposedException.ThrowIf(pDisposed, this);
        return IndexOf(value) != -1;
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public int IndexOf(object? value)
    {
#if WEB
        ObjectDisposedException.ThrowIf(pDisposed, this);
        return js_array_indexof(pArray, value);
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public void Insert(int index, object? value)
    {
#if WEB
        ObjectDisposedException.ThrowIf(pDisposed, this);
        js_array_insert(pArray, index, WebMarshal.CreateJsObject(value));
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public void Remove(object? value)
    {
#if WEB
        ObjectDisposedException.ThrowIf(pDisposed, this);
        RemoveAt(IndexOf(value));
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public void RemoveAt(int index)
    {
#if WEB
        ObjectDisposedException.ThrowIf(pDisposed, this);
        js_array_remove(pArray, index);
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }
    
    public IEnumerator GetEnumerator()
    {
#if WEB
        ObjectDisposedException.ThrowIf(pDisposed, this);
        for (var i = 0; i < Count; ++i)
            yield return this[i];
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public void CopyTo(Array array, int index)
    {
#if WEB
        ObjectDisposedException.ThrowIf(pDisposed, this);
        for (var idx = index; idx < Count; idx++)
            array.SetValue(this[idx], idx);
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }
    
    public object GetJsObject()
    {
#if WEB
        ObjectDisposedException.ThrowIf(pDisposed, this);
        return js_array_get_native(pArray);
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

#if WEB
    internal int GetId()
    {
        return pArray;
    }
#endif
}