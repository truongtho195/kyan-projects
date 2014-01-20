using CPC.POS.Model;

namespace CPC.POS.Interfaces
{
    public interface IEditableFolder
    {
        /// <summary>
        /// Update current folder.
        /// </summary>
        /// <param name="currentFolder">Current folder to update.</param>
        void Update(base_VirtualFolderModel currentFolder);
    }
}
