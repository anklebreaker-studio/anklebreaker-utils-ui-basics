using System;
using System.Collections.Generic;
using AnkleBreaker.Utils.Inspector;

namespace AnkleBreaker.Utils.UIBasics
{
    [Serializable]
    public class UISwitchButtonsDic<K, V> : AB_SerializedDictionary<K, V>
        where K : Enum
        where V : UISwitchButton
    {
        public K DefaultTab { get; set; }
        public K CurrentTab { get; private set; }
        public bool AllTabClosed { get; private set; }

        public Action<K> OnSelectionChanged { get; set; }
        public Action<K> OnSwitchClick { get; set; }

        public void Init(K defaultTab = default)
        {
            DefaultTab = defaultTab;

            foreach (KeyValuePair<K, V> kvp in this)
            {
                if (kvp.Value != null)
                {
                    K key = kvp.Key;
                    kvp.Value.onClick.AddListener(() => OnSwitchClick?.Invoke(key));
                    kvp.Value.SwitchOnClick = false;
                }

                kvp.Value.SetIsOn(kvp.Key.Equals(DefaultTab));
            }
        }

        public bool SelectTab(K keyToOpen)
        {
            if (TryGetValue(keyToOpen, out V sw) == false || sw.IsOn)
                return false;

            foreach (KeyValuePair<K, V> kvp in this)
                if (kvp.Key.Equals(keyToOpen) == false)
                    kvp.Value.SetIsOn(false);

            sw.SetIsOn(true);
            CurrentTab = keyToOpen;
            AllTabClosed = false;
            OnSelectionChanged?.Invoke(keyToOpen);
            return true;
        }

        public void CloseAllTab()
        {
            foreach (KeyValuePair<K, V> kvp in this)
                kvp.Value.SetIsOn(false);

            AllTabClosed = true;
        }
    }
}
