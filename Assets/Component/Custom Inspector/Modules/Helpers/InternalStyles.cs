
namespace CustomInspector.Extensions
{
    public enum InternalLabelStyle
    {
        /// <summary>
        /// No label. draws the value over full width
        /// </summary>
        NoLabel,
        /// <summary>
        /// No label, but draws the value only in the value Area
        /// </summary>
        EmptyLabel,
        /// <summary>
        /// Makes the label as small as the label text
        /// </summary>
        NoSpacing,
        /// <summary>
        /// The common: label has default labelwith and value field starts in next column
        /// </summary>
        FullSpacing,
    }  
}