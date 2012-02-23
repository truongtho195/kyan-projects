using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCard.Model
{
   public  class TypeModelBase: ModelBase
   {
       #region Constructor
       public TypeModelBase()
       {

       }
       #endregion

       #region Properties
       /// <summary>
       /// Gets or sets the property value.
       /// </summary>
       private int _typeID;
       public int TypeID
       {
           get { return _typeID; }
           set
           {
               if (_typeID != value)
               {
                   _typeID = value;
                   RaisePropertyChanged(() => TypeID);
               }
           }
       }

       #endregion
   }
}
