using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace CPCToolkitExtLibraries
{
    [Serializable]
    public class DataCollection : ObservableCollection<DataModel>
    {
        public DataCollection()
        {

        }
    }
}
