using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuadKeyDict<T1, T2, T3, T4, Val>
{
    Dictionary<T1, TripleKeyDict<T2, T3, T4, Val>> _key1Dict;
    Dictionary<T2, TripleKeyDict<T1, T3, T4, Val>> _key2Dict;
    Dictionary<T3, TripleKeyDict<T1, T2, T4, Val>> _key3Dict;
    Dictionary<T4, TripleKeyDict<T1, T2, T3, Val>> _key4Dict;

    public QuadKeyDict()
    {
        _key1Dict = new Dictionary<T1, TripleKeyDict<T2, T3, T4, Val>>();
        _key2Dict = new Dictionary<T2, TripleKeyDict<T1, T3, T4, Val>>();
        _key3Dict = new Dictionary<T3, TripleKeyDict<T1, T2, T4, Val>>();
        _key4Dict = new Dictionary<T4, TripleKeyDict<T1, T2, T3, Val>>();
    }
    public QuadKeyDict(QuadKeyDict<T1, T2, T3,T4, Val> original)
    {
        foreach (var kvp in original._key1Dict)
        {
            _key1Dict[kvp.Key] = new TripleKeyDict<T2, T3,T4, Val>(kvp.Value);
        }
        foreach (var kvp in original._key2Dict)
        {
            _key2Dict[kvp.Key] = new TripleKeyDict<T1, T3, T4, Val>(kvp.Value);
        }
        foreach (var kvp in original._key3Dict)
        {
            _key3Dict[kvp.Key] = new TripleKeyDict<T1, T2, T4, Val>(kvp.Value);
        }
        foreach (var kvp in original._key4Dict)
        {
            _key4Dict[kvp.Key] = new TripleKeyDict<T1, T2, T3, Val>(kvp.Value);
        }
    }
    public void Clear()
    {
        _key1Dict.Clear();
        _key2Dict.Clear();
        _key3Dict.Clear();
        _key4Dict.Clear();
    }

    public IEnumerable<T1> GetKeys1()
    {
        return _key1Dict.Keys;
    }
    public IEnumerable<T2> GetKeys2()
    {
        return _key2Dict.Keys;
    }
    public IEnumerable<T3> GetKeys3()
    {
        return _key3Dict.Keys;
    }
    public IEnumerable<T4> GetKeys4()
    {
        return _key4Dict.Keys;
    }
    public bool TryGetValue(T1 key1, T2 key2, T3 key3, T4 key4, out Val val)
    {
        val = default(Val);
        if (_key1Dict.TryGetValue(key1, out var tripledict))
        {
            return tripledict.TryGetValue(key2, key3, key4, out val);
        }
        return false;
    }
    public bool TryGetValueByKey1(T1 key1, out TripleKeyDict<T2, T3, T4, Val> val)
    {
        val = null;
        if (_key1Dict.TryGetValue(key1, out var dict1))
        {
            val = new TripleKeyDict<T2, T3,T4,Val>(dict1);
            return true;
        }
        return false;
    }
    public bool TryGetValueByKey2(T2 key2, out TripleKeyDict<T1, T3, T4, Val> val)
    {
        val = null;
        if (_key2Dict.TryGetValue(key2, out var dict2))
        {
            val = new TripleKeyDict<T1, T3,T4, Val>(dict2);
            return true;
        }
        return false;
    }
    public bool TryGetValueByKey3(T3 key3, out TripleKeyDict<T1, T2, T4, Val> val)
    {
        val = null;
        if (_key3Dict.TryGetValue(key3, out var dict3))
        {
            val = new TripleKeyDict<T1,T2,T4,Val>(dict3);
            return true;
        }
        return false;
    }
    public bool TryGetValueByKey4(T4 key4, out TripleKeyDict<T1, T2, T3, Val> val)
    {
        val = null;
        if (_key4Dict.TryGetValue(key4, out var dict4))
        {
            val = new TripleKeyDict<T1, T2, T3, Val>(dict4);
            return true;
        }
        return false;
    }
    public bool TryAdd(T1 key1, T2 key2, T3 key3, T4 key4, Val val)
    {
        bool success = true;
        if (!_key1Dict.TryGetValue(key1, out var dict1))
        {
            var tripledict = new TripleKeyDict<T2, T3, T4, Val>();
            _key1Dict.TryAdd(key1, tripledict);
            tripledict.TryAdd(key2, key3,key4, val);
        }
        else
        {
            success = dict1.TryAdd(key2, key3,key4, val);
        }

        if (!_key2Dict.TryGetValue(key2, out var dict2))
        {
            var tripledict = new TripleKeyDict<T1, T3, T4, Val>();
            _key2Dict.TryAdd(key2, tripledict);
            tripledict.TryAdd(key1, key3,key4, val);
        }
        else
        {
            success = dict2.TryAdd(key1, key3, key4, val);
        }

        if (!_key3Dict.TryGetValue(key3, out var dict3))
        {
            var tripledict = new TripleKeyDict<T1, T2, T4, Val>();
            _key3Dict.TryAdd(key3, tripledict);
            tripledict.TryAdd(key1, key2,key4, val);
        }
        else
        {
            success = dict3.TryAdd(key1, key2, key4, val);
        }

        if (!_key4Dict.TryGetValue(key4, out var dict4))
        {
            var tripledict = new TripleKeyDict<T1, T2, T3, Val>();
            _key4Dict.TryAdd(key4, tripledict);
            tripledict.TryAdd(key1, key2,key3, val);
        }
        else
        {
            success = dict4.TryAdd(key1, key2, key3, val);
        }
        return success;
    }
    public void Remove(T1 key1, T2 key2, T3 key3, T4 key4)
    {
        if (_key1Dict.TryGetValue(key1, out var dict1))
        {
            dict1.Remove(key2, key3,key4);
        }
        if (_key2Dict.TryGetValue(key2, out var dict2))
        {
            dict2.Remove(key1, key3, key4);
        }
        if (_key3Dict.TryGetValue(key3, out var dict3))
        {
            dict3.Remove(key1, key2, key4);
        }
        if (_key4Dict.TryGetValue(key4, out var dict4))
        {
            dict4.Remove(key1, key2, key3);
        }
    }
    public void RemoveByKey1(T1 key1)
    {
        if (_key1Dict.TryGetValue(key1, out var dict1))
        {
            dict1.Clear();
        }
        foreach (var kvp in _key2Dict)
        {
            kvp.Value.RemoveByKey1(key1);
        }
        foreach (var kvp in _key3Dict)
        {
            kvp.Value.RemoveByKey1(key1);
        }
        foreach (var kvp in _key4Dict)
        {
            kvp.Value.RemoveByKey1(key1);
        }
    }
    public void RemoveByKey2(T2 key2)
    {
        if (_key2Dict.TryGetValue(key2, out var dict2))
        {
            dict2.Clear();
        }
        foreach (var kvp in _key1Dict)
        {
            kvp.Value.RemoveByKey1(key2);
        }
        foreach (var kvp in _key3Dict)
        {
            kvp.Value.RemoveByKey2(key2);
        }
        foreach (var kvp in _key3Dict)
        {
            kvp.Value.RemoveByKey2(key2);
        }
    }
    public void RemoveByKey3(T3 key3)
    {
        if (_key3Dict.TryGetValue(key3, out var dict3))
        {
            dict3.Clear();
        }
        foreach (var kvp in _key1Dict)
        {
            kvp.Value.RemoveByKey2(key3);
        }
        foreach (var kvp in _key2Dict)
        {
            kvp.Value.RemoveByKey2(key3);
        }
        foreach (var kvp in _key4Dict)
        {
            kvp.Value.RemoveByKey3(key3);
        }
    }
    public void RemoveByKey4(T4 key4)
    {
        if (_key4Dict.TryGetValue(key4, out var dict4))
        {
            dict4.Clear();
        }
        foreach (var kvp in _key1Dict)
        {
            kvp.Value.RemoveByKey3(key4);
        }
        foreach (var kvp in _key2Dict)
        {
            kvp.Value.RemoveByKey3(key4);
        }
        foreach (var kvp in _key3Dict)
        {
            kvp.Value.RemoveByKey3(key4);
        }
    }
}
public class TripleKeyDict<T1, T2, T3,Val>
{
    Dictionary<T1, DoubleKeyDict<T2, T3, Val>> _key1Dict;
    Dictionary<T2, DoubleKeyDict<T1, T3, Val>> _key2Dict;
    Dictionary<T3, DoubleKeyDict<T1, T2, Val>> _key3Dict;

    public TripleKeyDict()
    {
        _key1Dict = new Dictionary<T1, DoubleKeyDict<T2, T3, Val>>();
        _key2Dict = new Dictionary<T2, DoubleKeyDict<T1, T3, Val>>();
        _key3Dict = new Dictionary<T3, DoubleKeyDict<T1, T2, Val>>(); 
    }
    public TripleKeyDict(TripleKeyDict<T1, T2,T3, Val> original)
    {
        foreach (var kvp in original._key1Dict)
        {
            _key1Dict[kvp.Key] = new DoubleKeyDict<T2, T3, Val>(kvp.Value);
        }
        foreach (var kvp in original._key2Dict)
        {
            _key2Dict[kvp.Key] = new DoubleKeyDict<T1, T3, Val>(kvp.Value);
        }
        foreach (var kvp in original._key3Dict)
        {
            _key3Dict[kvp.Key] = new DoubleKeyDict<T1, T2, Val>(kvp.Value);
        }
    }
    public void Clear()
    {
        _key1Dict.Clear();
        _key2Dict.Clear();
        _key3Dict.Clear();
    }
    public IEnumerable<T1> GetKeys1()
    {
        return _key1Dict.Keys;
    }
    public IEnumerable<T2> GetKeys2()
    {
        return _key2Dict.Keys;
    }
    public IEnumerable<T3> GetKeys3()
    {
        return _key3Dict.Keys;
    }
    public bool TryGetValue(T1 key1, T2 key2,T3 key3, out Val val)
    {
        val = default(Val);
        if (key1 == null || key2 == null|| key3 == null)
        {
            return false;
        }
        if (_key1Dict.TryGetValue(key1, out var doubledict))
        {
            return doubledict.TryGetValue(key2, key3, out val);
        }
        return false;
    }
    public bool TryGetValueByKey1(T1 key1, out DoubleKeyDict<T2, T3,Val> val)
    {
        val = null;
        if (key1 == null)
        {
            return false;
        }


        if (_key1Dict.TryGetValue(key1, out var dict1))
        {
            val = new DoubleKeyDict<T2,T3,Val>(dict1);
            return true;
        }
        return false;
    }
    public bool TryGetValueByKey2(T2 key2, out DoubleKeyDict<T1, T3,Val> val)
    {
        val = null;
        if (key2 == null)
        {
            return false;
        }
        if (_key2Dict.TryGetValue(key2, out var dict2))
        {
            val = new DoubleKeyDict<T1, T3, Val>(dict2);
            return true;
        }
        return false;
    }
    public bool TryGetValueByKey3(T3 key3, out DoubleKeyDict<T1, T2, Val> val)
    {
        val = null;
        if (key3 == null)
        {
            return false;
        }
        if (_key3Dict.TryGetValue(key3, out var dict3))
        {
            val = new DoubleKeyDict<T1, T2, Val>(dict3);
            return true;
        }
        return false;
    }
    public bool TryAdd(T1 key1, T2 key2, T3 key3, Val val)
    {
        bool success = true;
        if (!_key1Dict.TryGetValue(key1, out var dict1))
        {
            var doubledict = new DoubleKeyDict<T2, T3, Val>();
            _key1Dict.TryAdd(key1, doubledict);
            doubledict.TryAdd(key2,key3, val);
        }
        else
        {
            success = dict1.TryAdd(key2,key3, val);
        }

        if (!_key2Dict.TryGetValue(key2, out var dict2))
        {
            var doubledict = new DoubleKeyDict<T1, T3, Val>();
            _key2Dict.TryAdd(key2, doubledict);
            doubledict.TryAdd(key1,key3, val);
        }
        else
        {
            success = dict2.TryAdd(key1,key3, val);
        }

        if (!_key3Dict.TryGetValue(key3, out var dict3))
        {
            var doubledict = new DoubleKeyDict<T1, T2, Val>();
            _key3Dict.TryAdd(key3, doubledict);
            doubledict.TryAdd(key1, key2, val);
        }
        else
        {
            success = dict3.TryAdd(key1, key2, val);
        }
        return success;
    }
    public void Remove(T1 key1, T2 key2, T3 key3)
    {
        if (_key1Dict.TryGetValue(key1, out var dict1))
        {
            dict1.Remove(key2,key3);
        }
        if (_key2Dict.TryGetValue(key2, out var dict2))
        {
            dict2.Remove(key1, key3);
        }
        if (_key3Dict.TryGetValue(key3, out var dict3))
        {
            dict3.Remove(key1, key2);
        }
    }
    public void RemoveByKey1(T1 key1)
    {
        if (_key1Dict.TryGetValue(key1, out var dict1))
        {
            dict1.Clear();
        }
        foreach (var kvp in _key2Dict)
        {
            kvp.Value.RemoveByKey1(key1);
        }
        foreach (var kvp in _key3Dict)
        {
            kvp.Value.RemoveByKey1(key1);
        }
    }
    public void RemoveByKey2(T2 key2)
    {
        if (_key2Dict.TryGetValue(key2, out var dict2))
        {
            dict2.Clear();
        }
        foreach (var kvp in _key1Dict)
        {
            kvp.Value.RemoveByKey1(key2);
        }
        foreach (var kvp in _key3Dict)
        {
            kvp.Value.RemoveByKey2(key2);
        }
    }
    public void RemoveByKey3(T3 key3)
    {
        if (_key3Dict.TryGetValue(key3, out var dict3))
        {
            dict3.Clear();
        }
        foreach (var kvp in _key1Dict)
        {
            kvp.Value.RemoveByKey2(key3);
        }
        foreach (var kvp in _key2Dict)
        {
            kvp.Value.RemoveByKey2(key3);
        }
    }
}
public class DoubleKeyDict<T1, T2, Val>
{
    Dictionary<T1, Dictionary<T2, Val>> _key1Dict;
    Dictionary<T2, Dictionary<T1, Val>> _key2Dict;

    public DoubleKeyDict()
    {
        _key1Dict = new Dictionary<T1, Dictionary<T2, Val>>();
        _key2Dict = new Dictionary<T2, Dictionary<T1, Val>>();
    }

    public DoubleKeyDict(DoubleKeyDict<T1, T2, Val> original)
    {
        foreach (var kvp in original._key1Dict)
        {
            _key1Dict[kvp.Key] = kvp.Value;
        }
        foreach (var kvp in original._key2Dict)
        {
            _key2Dict[kvp.Key] = kvp.Value;
        }
    }
    public void Clear()
    {
        _key1Dict.Clear();
        _key2Dict.Clear();  
    }
    public IEnumerable<T1> GetKeys1()
    {
        return _key1Dict.Keys;
    }
    public IEnumerable<T2> GetKeys2()
    {
        return _key2Dict.Keys;
    }

    public IEnumerable<(T1 Key1, T2 Key2, Val Value)> GetEnumerator()
    {
        foreach (var kvp1 in _key1Dict.ToList())
        {
            T1 key1 = kvp1.Key;
            foreach (var kvp2 in kvp1.Value.ToList())
            {
                T2 key2 = kvp2.Key;
                Val value = kvp2.Value;
                yield return (key1, key2, value);
            }
        }
    }

    public bool TryGetValue(T1 key1, T2 key2, out Val val)
    {
        val = default(Val);
        if(key1 == null || key2 == null)
        {
            return false;
        }
        if(_key1Dict.TryGetValue(key1,out var dict))
        {
            if (dict.TryGetValue(key2, out var value))
            {
                val = value;
                return true;
            }
        }
        return false;
    }
    public bool TryGetValueByKey1(T1 key1, out Dictionary<T2, Val> val)
    {
        val = null;
        if (key1 == null)
        {
            return false;
        }

        if (_key1Dict.TryGetValue(key1, out var dict))
        {
            val = new Dictionary<T2, Val>(dict);
            return true;
        }
        return false;
    }
    public bool TryGetValueByKey2(T2 key2, out Dictionary<T1, Val> val)
    {
        val = null;
        if (key2 == null)
        {
            return false;
        }

        if (_key2Dict.TryGetValue(key2, out var dict))
        {
            val = new Dictionary<T1, Val>(dict);
            return true;
        }
        return false;
    }
    
    public bool TryAdd(T1 key1, T2 key2, Val val)
    {
        bool success = true;
        if(!_key1Dict.TryGetValue(key1,out var dict1))
        {
            Dictionary<T2, Val> key2dict = new Dictionary<T2, Val>();
            _key1Dict.TryAdd(key1,key2dict);
            key2dict.Add(key2,val);
        }
        else
        {
            success =  dict1.TryAdd(key2,val);
        }

        if (!_key2Dict.TryGetValue(key2, out var dict2))
        {
            Dictionary<T1, Val> key1dict = new Dictionary<T1, Val>();
            _key2Dict.TryAdd(key2, key1dict);
            key1dict.Add(key1, val);
        }
        else
        {
            success = dict2.TryAdd(key1, val);
        }
        return success;
    }
    public void Remove(T1 key1, T2 key2)
    {
        if (_key1Dict.TryGetValue(key1, out var dict1))
        {
            dict1.Remove(key2);
        }
        if (_key2Dict.TryGetValue(key2, out var dict2))
        {
            dict2.Remove(key1);
        }
    }
    public void RemoveByKey1(T1 key1)
    {
        if (_key1Dict.TryGetValue(key1, out var dict1))
        {
            dict1.Clear();
        }
        foreach(var kvp in _key2Dict)
        {
            if (kvp.Value.TryGetValue(key1, out _))
            {
                kvp.Value.Remove(key1);
            }
        }
    }
    public void RemoveByKey2(T2 key2)
    {
        if (_key2Dict.TryGetValue(key2, out var dict2))
        {
            dict2.Clear();
        }
        foreach (var kvp in _key1Dict)
        {
            if (kvp.Value.TryGetValue(key2, out _))
            {
                kvp.Value.Remove(key2);
            }
        }
    }
}