using System.Windows.Controls;
using System.Xml.Linq;

namespace CPC.POS.Interfaces
{
    interface IDemonstrator
    {
        /// <summary>
        /// Gets the name of the control.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the title of the control.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the description of the control.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Creates a instance of this object.
        /// </summary>
        UserControl Create(XElement configuration = null);
    }
}
