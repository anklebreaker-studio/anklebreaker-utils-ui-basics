namespace AnkleBreaker.Utils.UIBasics
{
    /// <summary>
    /// Contract for UI elements that support a selected / deselected toggle state.
    /// </summary>
    public interface ISelectable
    {
        bool IsSelected { get; }

        void SetIsSelected(bool selected);
    }
}