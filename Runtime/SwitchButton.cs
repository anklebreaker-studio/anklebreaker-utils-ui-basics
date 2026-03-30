using System;
using UnityEngine;
using UnityEngine.Events;
using AnkleBreaker.Utils.Inspector;

namespace AnkleBreaker.Utils.UIBasics
{
    /// <summary>
    /// A toggle-style button that switches between selected / deselected states.
    /// Fires UnityEvents so visual feedback (tweens, animations, etc.) can be wired
    /// from the inspector without any hard dependency.
    /// </summary>
    public class SwitchButton : ButtonBase, ISelectable
    {
        // ──────────────────────────── Settings ────────────────────────────

        [SectionHeader("Switch Settings")]
        [SerializeField] private bool _switchOnClick = true;
        public bool SwitchOnClick { get => _switchOnClick; set => _switchOnClick = value; }

        [SerializeField, LabelText("Toggle Object")]
        [ABToolTip("Optional GameObject activated when selected, deactivated when deselected.")]
        private GameObject _objToToggleOnSelection;

        // ──────────────────────────── Events ──────────────────────────────

        [FoldoutGroup("Switch Events", Style = FoldoutGroupStyle.Line)]
        [SerializeField] private UnityEvent _onSelected = new UnityEvent();

        [FoldoutGroup("Switch Events")]
        [SerializeField] private UnityEvent _onDeselected = new UnityEvent();

        [FoldoutGroup("Switch Events")]
        [SerializeField] private UnityEvent<bool> _onSelectionChangedEvent = new UnityEvent<bool>();

        /// <summary>Inspector-wirable event fired when the button becomes selected.</summary>
        public UnityEvent OnSelectedEvent => _onSelected;

        /// <summary>Inspector-wirable event fired when the button becomes deselected.</summary>
        public UnityEvent OnDeselectedEvent => _onDeselected;

        /// <summary>Inspector-wirable event fired on every selection change (true = selected).</summary>
        public UnityEvent<bool> OnSelectionChangedEvent => _onSelectionChangedEvent;

        /// <summary>C# event fired on every selection change (true = selected).</summary>
        public event Action<bool> OnSelectionValueChanged;

        // ──────────────────────────── State ───────────────────────────────

        [ShowInInspector(RuntimeOnly = true)]
        public bool IsSelected { get; private set; }

        // ──────────────────────────── API ─────────────────────────────────

        public void SetIsSelected(bool selected)
        {
            if (IsSelected == selected)
                return;

            IsSelected = selected;

            if (IsSelected)
            {
                if (_objToToggleOnSelection != null)
                    _objToToggleOnSelection.SetActive(true);

                _onSelected?.Invoke();
            }
            else
            {
                _onDeselected?.Invoke();

                if (_objToToggleOnSelection != null)
                    _objToToggleOnSelection.SetActive(false);
            }

            SelectionValueChanged();
        }

        /// <summary>
        /// Called after IsSelected changes. Override to add custom logic in subclasses.
        /// </summary>
        protected virtual void SelectionValueChanged()
        {
            OnSelectionValueChanged?.Invoke(IsSelected);
            _onSelectionChangedEvent?.Invoke(IsSelected);
        }

        protected override void OnBtnClick()
        {
            base.OnBtnClick();

            if (_switchOnClick)
                SetIsSelected(!IsSelected);
        }
    }
}