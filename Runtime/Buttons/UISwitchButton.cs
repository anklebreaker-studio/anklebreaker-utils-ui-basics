using System;
using UnityEngine;
using UnityEngine.Events;
using AnkleBreaker.Utils.Inspector;

namespace AnkleBreaker.Utils.UIBasics
{
    /// <summary>
    /// A toggle-style button that switches between on / off states.
    /// Fires UnityEvents so visual feedback (tweens, animations, etc.) can be wired
    /// from the inspector without any hard dependency.
    /// </summary>
    public class UISwitchButton : ABButton, IUISwitch
    {
        // ──────────────────────────── Settings ────────────────────────────

        [SectionHeader("UISwitchButton")]
        [SerializeField] private bool _switchOnClick = true;
        public bool SwitchOnClick { get => _switchOnClick; set => _switchOnClick = value; }

        [SerializeField, LabelText("Toggle Object")]
        [ABToolTip("Optional GameObject activated when on, deactivated when off.")]
        private GameObject _objToToggleOnSwitch;

        // ──────────────────────────── Events ──────────────────────────────

        [FoldoutGroup("Switch Events", Style = FoldoutGroupStyle.Line)]
        [SerializeField] private UnityEvent _onSwitchedOn = new UnityEvent();

        [FoldoutGroup("Switch Events")]
        [SerializeField] private UnityEvent _onSwitchedOff = new UnityEvent();

        [FoldoutGroup("Switch Events")]
        [SerializeField] private UnityEvent<bool> _onSwitchChanged = new UnityEvent<bool>();

        /// <summary>Inspector-wirable event fired when the button switches on.</summary>
        public UnityEvent OnSwitchedOnEvent => _onSwitchedOn;

        /// <summary>Inspector-wirable event fired when the button switches off.</summary>
        public UnityEvent OnSwitchedOffEvent => _onSwitchedOff;

        /// <summary>Inspector-wirable event fired on every switch change (true = on).</summary>
        public UnityEvent<bool> OnSwitchChangedEvent => _onSwitchChanged;

        /// <summary>C# event fired on every switch change (true = on).</summary>
        public event Action<bool> OnSwitchValueChanged;

        // ──────────────────────────── State ───────────────────────────────

        [ShowInInspector(RuntimeOnly = true)]
        public bool IsOn { get; private set; }

        // ──────────────────────────── API ─────────────────────────────────

        public void SetIsOn(bool on)
        {
            if (IsOn == on)
                return;

            IsOn = on;

            if (IsOn)
            {
                if (_objToToggleOnSwitch != null)
                    _objToToggleOnSwitch.SetActive(true);

                _onSwitchedOn?.Invoke();
            }
            else
            {
                _onSwitchedOff?.Invoke();

                if (_objToToggleOnSwitch != null)
                    _objToToggleOnSwitch.SetActive(false);
            }

            SwitchValueChanged();
        }

        /// <summary>
        /// Called after IsOn changes. Override to add custom logic in subclasses.
        /// </summary>
        protected virtual void SwitchValueChanged()
        {
            OnSwitchValueChanged?.Invoke(IsOn);
            _onSwitchChanged?.Invoke(IsOn);
        }

        protected override void OnBtnClick()
        {
            base.OnBtnClick();

            if (_switchOnClick)
                SetIsOn(!IsOn);
        }
    }
}
