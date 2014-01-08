
using Microsoft.Windows.Controls.Ribbon;
using CPC.Control;

namespace CPC.Toolkit.Layout
{
    public class Layout
    {
        #region Property ModuleCollection
        /// <summary>
        /// Property ModuleCollection.
        /// </summary>
        private static System.Collections.Generic.IList<RibbonButton> _moduleCollection;
        public static System.Collections.Generic.IList<RibbonButton> ModuleCollection
        {
            get {
                if (_moduleCollection == null)
                {
                    _moduleCollection = new System.Collections.Generic.List<RibbonButton>();
                }
                return _moduleCollection; }
        }
        #endregion // end Property ModuleCollection

        //public static RibbonButton MakeRibbonButton(
        //    string title,
        //    string imagePath,
        //    string parameter,
        //    System.Windows.Input.ICommand actualCommand)
        //{
        //    var uri = new System.Uri(imagePath);

        //    var command = new NonRoutedRibbonCommandDelegator2();
        //    command.LabelTitle = title;
        //    command.ActualCommand = actualCommand;
        //    command.LargeImageSource = new System.Windows.Media.Imaging.BitmapImage(uri);
        //    command.SmallImageSource = new System.Windows.Media.Imaging.BitmapImage(uri);

        //    var ribbonButton = new RibbonButton();
        //    ribbonButton.Command = command;
        //    ribbonButton.CommandParameter = parameter;

        //    return ribbonButton;
        //}

        /// <summary>
        /// Get ContainerView from children DependencyObject
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public static ContainerView FindParent(System.Windows.DependencyObject child)
        {
            System.Windows.DependencyObject parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
            // Check if this is the end of the tree       
            if (parent == null)
                return null;

            ContainerView parentWindow = parent as ContainerView;
            if (parentWindow != null)
                return parentWindow;
            else
                // Use recursion until it reaches a Window           
                return FindParent(parent);
        }
    }
}
