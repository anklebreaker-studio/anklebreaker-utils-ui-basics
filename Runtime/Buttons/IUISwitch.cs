namespace AnkleBreaker.Utils.UIBasics
{
    /// <summary>
    /// Contract for UI elements that support an on/off toggle state.
    /// </summary>
    public interface IUISwitch
    {
        bool IsOn { get; }

        void SetIsOn(bool on);
    }
}
