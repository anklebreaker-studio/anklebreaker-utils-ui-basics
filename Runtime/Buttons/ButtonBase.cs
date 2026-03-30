using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using AnkleBreaker.Utils.Inspector;

namespace AnkleBreaker.Utils.UIBasics
{
    /// <summary>
    /// Base button that exposes UI.Button selection state transitions
    /// via both C# events (Action) and UnityEvents for inspector wiring.
    /// </summary>
    public class ButtonBase : Button
    {
        [Serializable]
        public class StateChangedUnityEvent : UnityEvent<ESelectionState, ESelectionState> { }

        [FoldoutGroup("Button Events", Style = FoldoutGroupStyle.Line)]
        [SerializeField] private StateChangedUnityEvent _onStateChangedEvent = new StateChangedUnityEvent();

        [FoldoutGroup("Button Events")]
        [SerializeField] private UnityEvent _onClickEvent = new UnityEvent();

        /// <summary>C# event fired on every state transition (prevState, newState).</summary>
        public event Action<ESelectionState, ESelectionState> OnStateChanged;

        /// <summary>Inspector-wirable event fired on every state transition.</summary>
        public StateChangedUnityEvent OnStateChangedEvent => _onStateChangedEvent;

        /// <summary>Inspector-wirable click event (supplements Button.onClick).</summary>
        public UnityEvent OnClickEvent => _onClickEvent;

        /// <summary>The selection state before the last transition.</summary>
        [ShowInInspector(RuntimeOnly = true)]
        public ESelectionState PrevState { get; private set; }

        /// <summary>The current selection state.</summary>
        [ShowInInspector(RuntimeOnly = true)]
        public ESelectionState CurrentState => (ESelectionState)currentSelectionState;

        protected override void Awake()
        {
            base.Awake();
            onClick.AddListener(HandleClick);
        }

        protected override void OnDestroy()
        {
            onClick.RemoveListener(HandleClick);
            base.OnDestroy();
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            ESelectionState newState = (ESelectionState)state;
            ESelectionState prevState = PrevState;

            OnStateChanged?.Invoke(prevState, newState);
            _onStateChangedEvent?.Invoke(prevState, newState);

            PrevState = newState;
        }

        /// <summary>
        /// Called on button click. Override in subclasses to add behavior.
        /// </summary>
        protected virtual void OnBtnClick() { }

        private void HandleClick()
        {
            OnBtnClick();
            _onClickEvent?.Invoke();
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