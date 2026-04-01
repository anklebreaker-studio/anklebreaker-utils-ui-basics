using System;
using System.Collections.Generic;
using UnityEngine.Events;
using AnkleBreaker.Utils.Inspector;

namespace AnkleBreaker.Utils.UIBasics
{
    [Serializable]
    public class UISwitchButtonsDic<K, V> : AB_SerializedDictionary<K, V>
        where K : Enum
        where V : UISwitchButton
    {
        public K DefaultKey { get; set; }
        public K SelectedKey { get; private set; }
        public bool NoneSelected { get; private set; } = true;
        public bool IsInitialized { get; private set; }

        public Action<K> OnSelectionChanged { get; set; }
        public Action<K> OnSwitchClick { get; set; }

        private readonly Dictionary<K, UnityAction> _clickDelegates = new Dictionary<K, UnityAction>();

        public void Init(K defaultKey = default)
        {
            if (IsInitialized)
                Reset();

            DefaultKey = defaultKey;
            SelectedKey = defaultKey;
            NoneSelected = false;
            IsInitialized = true;

            foreach (KeyValuePair<K, V> kvp in this)
            {
                if (kvp.Value != null)
                {
                    K key = kvp.Key;
                    UnityAction clickDelegate = () => OnSwitchClick?.Invoke(key);
                    _clickDelegates[key] = clickDelegate;

                    kvp.Value.onClick.AddListener(clickDelegate);
                    kvp.Value.SwitchOnClick = false;
                }

                kvp.Value.SetIsOn(kvp.Key.Equals(DefaultKey));
            }
        }

        public void Reset()
        {
            foreach (KeyValuePair<K, V> kvp in this)
            {
                if (kvp.Value != null && _clickDelegates.TryGetValue(kvp.Key, out UnityAction clickDelegate))
                    kvp.Value.onClick.RemoveListener(clickDelegate);
            }

            _clickDelegates.Clear();
            IsInitialized = false;
            NoneSelected = true;
            SelectedKey = default;
        }

        public bool Select(K keyToOpen)
        {
            if (TryGetValue(keyToOpen, out V sw) == false || sw.IsOn)
                return false;

            foreach (KeyValuePair<K, V> kvp in this)
                if (kvp.Value != null && kvp.Key.Equals(keyToOpen) == false)
                    kvp.Value.SetIsOn(false);

            sw.SetIsOn(true);
            SelectedKey = keyToOpen;
            NoneSelected = false;
            OnSelectionChanged?.Invoke(keyToOpen);
            return true;
        }

        public void ClearSelection()
        {
            foreach (KeyValuePair<K, V> kvp in this)
                if (kvp.Value != null)
                    kvp.Value.SetIsOn(false);

            NoneSelected = true;
        }
    }
}
