using System.Xml.Linq;
using System.Windows;

namespace CPC.POS.Interfaces
{
    interface IDashboardItemFunction
    {
        /// <summary>
        /// Determine whether can edit object.
        /// </summary>
        bool CanEdit
        {
            get;
        }

        /// <summary>
        /// Lock object.
        /// </summary>
        void Lock();

        /// <summary>
        /// Unlock object.
        /// </summary>
        void Unlock();

        /// <summary>
        /// Gets XML element that contains configuration.
        /// </summary>
        /// <returns>XElement</returns>
        XElement GetConfiguration();

        /// <summary>
        /// Update size of DashboardItem.
        /// </summary>
        /// <param name="newSize"></param>
        void UpdateSize(Size newSize);
    }
}
