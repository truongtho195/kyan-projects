﻿using System;
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


       /// <summary>
       /// Gets or sets the property value.
       /// </summary>
       private List<LessonModel> _lessonCollection;
       public List<LessonModel> LessonCollection
       {
           get { return _lessonCollection; }
           set
           {
               if (_lessonCollection != value)
               {
                   this.OnLessonCollectionChanging(value);
                   _lessonCollection = value;
                   RaisePropertyChanged(() => LessonCollection);
                   this.OnLessonCollectionChanged();
               }
           }
       }

       protected virtual void OnLessonCollectionChanging(List<LessonModel> value) { }
       protected virtual void OnLessonCollectionChanged() { }



       private int _typeOf;
       /// <summary>
       /// Gets or sets the property value.
       /// </summary>
       public int TypeOf
       {
           get { return _typeOf; }
           set
           {
               if (_typeOf != value)
               {
                   this.OnTypeOfChanging(value);
                   _typeOf = value;
                   RaisePropertyChanged(() => TypeOf);
                   this.OnTypeOfChanged();
               }
           }
       }

       protected virtual void OnTypeOfChanging(int value) { }
       protected virtual void OnTypeOfChanged() { }






       #endregion
   }
}