using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomInspector
{
    [Serializable]
    public class ReorderableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [MessageBox("Please add the [Dictionary]-attribute to this Dictionary for correct displaying")]
        [HideField, SerializeField] bool _;
#endif

        [SerializeField] List<SerializableKeyValuePair> keyValuePairs = new();

        public ReorderableDictionary() : base()
        { }
        public ReorderableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
        {
#if UNITY_EDITOR
            keyValuePairs = dictionary.Select(_ => new SerializableKeyValuePair(_.Key, _.Value, true)).ToList();
#endif
        }
        public ReorderableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection)
        {
#if UNITY_EDITOR
            // keyValuePairs.Clear();
            foreach (KeyValuePair<TKey, TValue> item in collection)
            {
                if (!keyValuePairs.Any(x => x.key.Equals(item.Key)))
                    keyValuePairs.Add(new SerializableKeyValuePair(item.Key, item.Value, true));
                else    
                    keyValuePairs.Add(new SerializableKeyValuePair(item.Key, item.Value, false));
            }
#endif
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() //from object to serialized
        {
            //Retrieve values from inner Dictionary
            List<SerializableKeyValuePair> values = this.Select(_ => new SerializableKeyValuePair(_.Key, _.Value, true)).ToList();
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize() //from serialized to object
        {
            //Set values to inner Dictionary
            base.Clear();
            foreach (var item in keyValuePairs)
            {
                if(item.isValid)
                {
                    if (this.TryAdd(item.key, item.value))
                        item.isValid = true;
                    else
                        item.isValid = false;
                }
            }
        }

        [System.Serializable]
        public class SerializableKeyValuePair
        {
            public TKey key;
            public TValue value;

            public bool isValid;

            public SerializableKeyValuePair(TKey key, TValue value, bool isValid)
            {
                this.key = key;
                this.value = value;

                this.isValid = isValid;
            }
        }
    }
}
