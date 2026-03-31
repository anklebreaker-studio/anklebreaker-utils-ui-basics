using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using AnkleBreaker.Utils.Inspector;

namespace AnkleBreaker.Utils.UIBasics
{
    /// <summary>
    /// Base button that exposes UI.Button selection state transitions
    /// via C# event and per-state UnityEvents for inspector wiring.
    /// </summary>
    public class ABButton : Button
    {
        [FoldoutGroup("ABButton - Events", FoldoutGroupStyle.CenterLine)]
        [SerializeField] private UnityEvent _onNormal = new UnityEvent();

        [FoldoutGroup("ABButton - Events")]
        [SerializeField] private UnityEvent _onHighlighted = new UnityEvent();

        [FoldoutGroup("ABButton - Events")]
        [SerializeField] private UnityEvent _onPressed = new UnityEvent();

        [FoldoutGroup("ABButton - Events")]
        [SerializeField] private UnityEvent _onSelected = new UnityEvent();

        [FoldoutGroup("ABButton - Events")]
        [SerializeField] private UnityEvent _onDisabled = new UnityEvent();

        /// <summary>C# event fired on every state transition (prevState, newState).</summary>
        public event Action<ESelectionState, ESelectionState> OnStateChanged;

        private ESelectionState _currentState = ESelectionState.Normal;

        /// <summary>The selection state before the last transition.</summary>
        [ShowInInspector(RuntimeOnly = true)]
        public ESelectionState PrevState { get; private set; }

        /// <summary>The current selection state.</summary>
        [ShowInInspector(RuntimeOnly = true)]
        public ESelectionState CurrentState => _currentState;

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            ESelectionState newState = (ESelectionState)state;
            if (_currentState == newState) return;

            PrevState = _currentState;
            _currentState = newState;
            OnStateChanged?.Invoke(PrevState, newState);
            InvokeStateEvent(newState);
        }

        private void InvokeStateEvent(ESelectionState state)
        {
            switch (state)
            {
                case ESelectionState.Normal:      _onNormal?.Invoke(); break;
                case ESelectionState.Highlighted:  _onHighlighted?.Invoke(); break;
                case ESelectionState.Pressed:      _onPressed?.Invoke(); break;
                case ESelectionState.Selected:     _onSelected?.Invoke(); break;
                case ESelectionState.Disabled:     _onDisabled?.Invoke(); break;
            }
        }

        /// <summary>
        /// Called on button click. Override in subclasses to add behavior.
        /// </summary>
        protected virtual void OnBtnClick() { }

        protected override void Awake()
        {
            base.Awake();
            onClick.AddListener(OnBtnClick);
        }

        protected override void OnDestroy()
        {
            onClick.RemoveListener(OnBtnClick);
            base.OnDestroy();
        }

        /// <summary>
        /// Public mirror of Selectable.SelectionState for external use.
        /// </summary>
        public enum ESelectionState
        {
            Normal = 0,
            Highlighted = 1,
            Pressed = 2,
            Selected = 3,
            Disabled = 4,
        }
    }
}