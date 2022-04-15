using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ED.AdminPanel
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key,
            Func<TKey, TValue> valueFactory)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }

            var value = valueFactory(key);
            dict.Add(key, value);
            return value;
        }


        public static async Task<TValue> GetOrAddAsync<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key,
            Func<TKey, CancellationToken, Task<TValue>> valueFactory,
            CancellationToken ct)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }

            var value = await valueFactory(key, ct);
            dict.Add(key, value);
            return value;
        }

        public static TValue AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key,
            TValue value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }

            return value;
        }

        // Summary:
        //     Adds a key/value pair to the dictionary if the key does not already exist,
        //     or updates a key/value pair in the dictionary
        //     by using the specified function if the key already exists.
        //
        // Parameters:
        //   key:
        //     The key to be added or whose value should be updated
        //
        //   addValue:
        //     The value to be added for an absent key
        //
        //   updateValueFactory:
        //     The function used to generate a new value for an existing key based on the key's
        //     existing value
        //
        // Returns:
        //     The new value for the key. This will be either be addValue (if the key was absent)
        //     or the result of updateValueFactory (if the key was present).
        public static TValue AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key,
            TValue addValue,
            Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue value;
            if (dict.ContainsKey(key))
            {
                value = updateValueFactory(key, dict[key]);
                dict[key] = value;
            }
            else
            {
                value = addValue;
                dict.Add(key, value);
            }

            return value;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key)
            => GetValueOrDefault(dict, key, default(TValue));

        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key,
            TValue defaultValue)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }

            return defaultValue;
        }
    }
}
