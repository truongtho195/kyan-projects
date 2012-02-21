using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMHelper.Services
{
    public interface IOpenFileDialogService
    {
        // Property
        string[] FileNames { get; }

        string FileName { get; set; }

        string InitialDirectory { get; set; }

        string Filter { get; set; }

        int FilterIndex { get; set; }

        bool Multiselect { get; set; }

        bool CheckPathExists { get; set; }

        bool CheckFileExists { get; set; }

        // Methods
        bool? ShowDialog();

        // Events
        System.ComponentModel.CancelEventHandler FileOk { set; }
    }
}
