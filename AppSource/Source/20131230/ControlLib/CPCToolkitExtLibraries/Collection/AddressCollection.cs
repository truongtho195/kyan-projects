using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace CPCToolkitExtLibraries
{
    [Serializable]
    public class AddressControlCollection : ObservableCollection<AddressControlModel>
    {
        public AddressControlCollection()
        {

        }

        public bool IsEditingData
        {
            get
            {
                return (this != null && this.Count > 0
                    && this.Count(x => x.IsDirty) > 0);
            }
        }

        public bool IsErrorData
        {
            get;
            //{
            //    return (this != null && this.Count > 0
            //        && this.Count(x => x.IsSelected && x.Errors.Count > 0) > 0);
            //}
            set;
        }
    }
}
