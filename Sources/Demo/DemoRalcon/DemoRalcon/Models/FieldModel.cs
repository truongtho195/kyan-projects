using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using DemoFalcon.Helper;
using System.ComponentModel;

namespace DemoFalcon.Model
{
    public class FieldModel : ModelBase
    {
        #region Constructors
        public FieldModel()
        {

        }
        #endregion

        #region Properties

        private int _iD;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int ID
        {
            get { return _iD; }
            set
            {
                if (_iD != value)
                {
                    _iD = value;
                    RaisePropertyChanged(() => ID);
                }
            }
        }


        private string _name;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }




        #endregion

        
    }
}
