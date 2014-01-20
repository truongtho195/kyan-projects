using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace CPC.Toolkit.Behavior
{
    /// <summary>
    /// Specifies the case of characters typed manually into a TextBox control.
    /// </summary>
    public class CharacterCasingBehavior : Behavior<TextBox>
    {
        #region Defines

        /// <summary>
        /// CharacterCasingTypes enum
        /// </summary>
        public enum CharacterCasingTypes
        {
            Lower,
            Normal,
            Title,
            Upper
        }

        /// <summary>
        /// Gets or sets the CharacterCasingType
        /// </summary>
        public CharacterCasingTypes CharacterCasingType { get; set; }

        #endregion

        #region Attached & Detached

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            // Register TextInput event to AssociatedObject
            AssociatedObject.AddHandler(TextBox.TextInputEvent, new TextCompositionEventHandler(AssociatedObject_TextInput), true);
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        /// <remarks>Override this to unhook functionality from the AssociatedObject.</remarks>
        protected override void OnDetaching()
        {
            // Recommended best practice: 
            // Detach the registered event handler to avoid memory leaks.
            base.OnDetaching();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the TextInput event of the TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void AssociatedObject_TextInput(object sender, TextCompositionEventArgs e)
        {
            if (!CPC.POS.Define.CONFIGURATION.IsAllowFirstCap)
            {
                AssociatedObject.RemoveHandler(TextBox.TextInputEvent, new TextCompositionEventHandler(AssociatedObject_TextInput));
                return;
            }

            // Get current position cursor
            int cursorPosition = this.AssociatedObject.SelectionStart;

            // Get current text
            string currentText = this.AssociatedObject.Text;

            switch (this.CharacterCasingType)
            {
                case CharacterCasingTypes.Lower:
                    this.AssociatedObject.Text = currentText.ToLower();
                    break;
                case CharacterCasingTypes.Normal:
                    break;
                case CharacterCasingTypes.Title:
                    this.AssociatedObject.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(currentText);
                    break;
                case CharacterCasingTypes.Upper:
                    this.AssociatedObject.Text = currentText.ToUpper();
                    break;
            }

            this.AssociatedObject.SelectionStart = cursorPosition;
        }

        #endregion
    }
}