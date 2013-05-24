using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.POS.Model;

namespace CPC.POS.Interfaces
{
    public interface IEditableAttachment
    {
        /// <summary>
        /// Update current attachment.
        /// </summary>
        /// <param name="currentAttachment">Current attachment to update.</param>
        void Update(base_AttachmentModel currentAttachment);
    }
}
