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
                   this.OnTypeIDChanging(value);
                   _typeID = value;
                   RaisePropertyChanged(() => TypeID);
                   this.OnTypeIDChanged();
               }
           }
       }

       protected virtual void OnTypeIDChanging(int value) { }
       protected virtual void OnTypeIDChanged() { }

       /// <summary>
       /// Gets or sets the property value.
       /// </summary>
       private string _name;
       public string Name
       {
           get { return _name; }
           set
           {
               if (_name != value)
               {
                   this.OnNameChanging(value);
                   _name = value;
                   RaisePropertyChanged(() => Name);
                   this.OnNameChanged();
               }
           }
       }

       protected virtual void OnNameChanging(string value) { }
       protected virtual void OnNameChanged() { }





       #endregion
   }
}
