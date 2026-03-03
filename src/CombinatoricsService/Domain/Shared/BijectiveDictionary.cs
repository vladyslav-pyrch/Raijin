using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Raijin.CombinatoricsService.Domain.Shared;

public class BijectiveDictionary<TFrom, TTo> : IDictionary<TFrom, TTo> where TFrom : notnull where TTo : notnull
{
    private readonly IDictionary<TFrom, TTo> _forward;

    private readonly IDictionary<TTo, TFrom> _backward;

    public BijectiveDictionary() : this(new Dictionary<TFrom, TTo>(), new Dictionary<TTo, TFrom>())
    { }

    public BijectiveDictionary(IDictionary<TFrom, TTo> dictionary) : this()
    {
        foreach (KeyValuePair<TFrom, TTo> pair in dictionary)
            Add(pair);
    }

    private BijectiveDictionary(IDictionary<TFrom, TTo> forward, IDictionary<TTo, TFrom> backward)
    {
        _forward = forward;
        _backward = backward;
    }

    public BijectiveDictionary<TTo, TFrom> Inverse() => new(_backward, _forward);

    public IEnumerator<KeyValuePair<TFrom, TTo>> GetEnumerator() => _forward.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(KeyValuePair<TFrom, TTo> item)
    {
        _forward.Add(item);
        _backward.Add(new KeyValuePair<TTo, TFrom>(item.Value, item.Key));
    }

    public void Clear()
    {
        _forward.Clear();
        _backward.Clear();
    }

    public bool Contains(KeyValuePair<TFrom, TTo> item) => _forward.Contains(item);

    public void CopyTo(KeyValuePair<TFrom, TTo>[] array, int arrayIndex)
    {
        _forward.CopyTo(array, arrayIndex);
        _backward.CopyTo(array.Select(kv => new KeyValuePair<TTo, TFrom>(kv.Value, kv.Key)).ToArray(), arrayIndex);
    }

    public bool Remove(KeyValuePair<TFrom, TTo> item)
    {
        _backward.Remove(item.Value);
        return _forward.Remove(item);
    }

    public int Count => _forward.Count;
    public bool IsReadOnly => _forward.IsReadOnly;

    public void Add(TFrom key, TTo value)
    {
        _forward.Add(key, value);
        _backward.Add(value, key);
    }

    public bool ContainsKey(TFrom key) => _forward.ContainsKey(key);

    public bool Remove(TFrom key)
    {
        if (!_forward.TryGetValue(key, out TTo? value))
            return false;

        _backward.Remove(value);
        return _forward.Remove(key);
    }

    public bool TryGetValue(TFrom key, [MaybeNullWhen(false)] out TTo value) => _forward.TryGetValue(key, out value);

    public TTo this[TFrom key]
    {
        get => _forward[key];
        set
        {
            if (_forward.TryGetValue(key, out TTo? existingValue))
                _backward.Remove(existingValue);

            if (_backward.TryGetValue(value, out TFrom? existingKey))
                _forward.Remove(existingKey);

            _forward[key] = value;
            _backward[value] = key;
        }
    }

    public ICollection<TFrom> Keys => _forward.Keys;
    public ICollection<TTo> Values => _forward.Values;

    public static implicit operator BijectiveDictionary<TFrom, TTo>(Dictionary<TFrom, TTo> dictionary) => new(dictionary);

    public static implicit operator Dictionary<TFrom, TTo>(BijectiveDictionary<TFrom, TTo> bijectiveDictionary) => bijectiveDictionary._forward.ToDictionary();
}