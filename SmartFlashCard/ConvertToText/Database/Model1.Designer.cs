﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.EntityClient;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;

[assembly: EdmSchemaAttribute()]
#region EDM Relationship Metadata

[assembly: EdmRelationshipAttribute("SmartFlashCardDBModel", "FK_BackSide_0", "Lesson", System.Data.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(ConvertToText.Database.Lesson), "BackSide", System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(ConvertToText.Database.BackSide), true)]

#endregion

namespace ConvertToText.Database
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class SmartFlashCardDBEntities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new SmartFlashCardDBEntities object using the connection string found in the 'SmartFlashCardDBEntities' section of the application configuration file.
        /// </summary>
        public SmartFlashCardDBEntities() : base("name=SmartFlashCardDBEntities", "SmartFlashCardDBEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new SmartFlashCardDBEntities object.
        /// </summary>
        public SmartFlashCardDBEntities(string connectionString) : base(connectionString, "SmartFlashCardDBEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new SmartFlashCardDBEntities object.
        /// </summary>
        public SmartFlashCardDBEntities(EntityConnection connection) : base(connection, "SmartFlashCardDBEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        #endregion
    
        #region Partial Methods
    
        partial void OnContextCreated();
    
        #endregion
    
        #region ObjectSet Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<BackSide> BackSides
        {
            get
            {
                if ((_BackSides == null))
                {
                    _BackSides = base.CreateObjectSet<BackSide>("BackSides");
                }
                return _BackSides;
            }
        }
        private ObjectSet<BackSide> _BackSides;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<Lesson> Lessons
        {
            get
            {
                if ((_Lessons == null))
                {
                    _Lessons = base.CreateObjectSet<Lesson>("Lessons");
                }
                return _Lessons;
            }
        }
        private ObjectSet<Lesson> _Lessons;

        #endregion
        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the BackSides EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToBackSides(BackSide backSide)
        {
            base.AddObject("BackSides", backSide);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Lessons EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToLessons(Lesson lesson)
        {
            base.AddObject("Lessons", lesson);
        }

        #endregion
    }
    

    #endregion
    
    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="SmartFlashCardDBModel", Name="BackSide")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class BackSide : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new BackSide object.
        /// </summary>
        /// <param name="backSideID">Initial value of the BackSideID property.</param>
        public static BackSide CreateBackSide(global::System.Int64 backSideID)
        {
            BackSide backSide = new BackSide();
            backSide.BackSideID = backSideID;
            return backSide;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int64 BackSideID
        {
            get
            {
                return _BackSideID;
            }
            set
            {
                if (_BackSideID != value)
                {
                    OnBackSideIDChanging(value);
                    ReportPropertyChanging("BackSideID");
                    _BackSideID = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("BackSideID");
                    OnBackSideIDChanged();
                }
            }
        }
        private global::System.Int64 _BackSideID;
        partial void OnBackSideIDChanging(global::System.Int64 value);
        partial void OnBackSideIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int64> LessonID
        {
            get
            {
                return _LessonID;
            }
            set
            {
                OnLessonIDChanging(value);
                ReportPropertyChanging("LessonID");
                _LessonID = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("LessonID");
                OnLessonIDChanged();
            }
        }
        private Nullable<global::System.Int64> _LessonID;
        partial void OnLessonIDChanging(Nullable<global::System.Int64> value);
        partial void OnLessonIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Content
        {
            get
            {
                return _Content;
            }
            set
            {
                OnContentChanging(value);
                ReportPropertyChanging("Content");
                _Content = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Content");
                OnContentChanged();
            }
        }
        private global::System.String _Content;
        partial void OnContentChanging(global::System.String value);
        partial void OnContentChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Boolean> IsCorrect
        {
            get
            {
                return _IsCorrect;
            }
            set
            {
                OnIsCorrectChanging(value);
                ReportPropertyChanging("IsCorrect");
                _IsCorrect = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("IsCorrect");
                OnIsCorrectChanged();
            }
        }
        private Nullable<global::System.Boolean> _IsCorrect;
        partial void OnIsCorrectChanging(Nullable<global::System.Boolean> value);
        partial void OnIsCorrectChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String BackSideName
        {
            get
            {
                return _BackSideName;
            }
            set
            {
                OnBackSideNameChanging(value);
                ReportPropertyChanging("BackSideName");
                _BackSideName = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("BackSideName");
                OnBackSideNameChanged();
            }
        }
        private global::System.String _BackSideName;
        partial void OnBackSideNameChanging(global::System.String value);
        partial void OnBackSideNameChanged();

        #endregion
    
        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute("SmartFlashCardDBModel", "FK_BackSide_0", "Lesson")]
        public Lesson Lesson
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<Lesson>("SmartFlashCardDBModel.FK_BackSide_0", "Lesson").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<Lesson>("SmartFlashCardDBModel.FK_BackSide_0", "Lesson").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<Lesson> LessonReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<Lesson>("SmartFlashCardDBModel.FK_BackSide_0", "Lesson");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<Lesson>("SmartFlashCardDBModel.FK_BackSide_0", "Lesson", value);
                }
            }
        }

        #endregion
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="SmartFlashCardDBModel", Name="Lesson")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class Lesson : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new Lesson object.
        /// </summary>
        /// <param name="lessonID">Initial value of the LessonID property.</param>
        public static Lesson CreateLesson(global::System.Int64 lessonID)
        {
            Lesson lesson = new Lesson();
            lesson.LessonID = lessonID;
            return lesson;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int64 LessonID
        {
            get
            {
                return _LessonID;
            }
            set
            {
                if (_LessonID != value)
                {
                    OnLessonIDChanging(value);
                    ReportPropertyChanging("LessonID");
                    _LessonID = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("LessonID");
                    OnLessonIDChanged();
                }
            }
        }
        private global::System.Int64 _LessonID;
        partial void OnLessonIDChanging(global::System.Int64 value);
        partial void OnLessonIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String LessonName
        {
            get
            {
                return _LessonName;
            }
            set
            {
                OnLessonNameChanging(value);
                ReportPropertyChanging("LessonName");
                _LessonName = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("LessonName");
                OnLessonNameChanged();
            }
        }
        private global::System.String _LessonName;
        partial void OnLessonNameChanging(global::System.String value);
        partial void OnLessonNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Description
        {
            get
            {
                return _Description;
            }
            set
            {
                OnDescriptionChanging(value);
                ReportPropertyChanging("Description");
                _Description = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Description");
                OnDescriptionChanged();
            }
        }
        private global::System.String _Description;
        partial void OnDescriptionChanging(global::System.String value);
        partial void OnDescriptionChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int64> CategoryID
        {
            get
            {
                return _CategoryID;
            }
            set
            {
                OnCategoryIDChanging(value);
                ReportPropertyChanging("CategoryID");
                _CategoryID = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("CategoryID");
                OnCategoryIDChanged();
            }
        }
        private Nullable<global::System.Int64> _CategoryID;
        partial void OnCategoryIDChanging(Nullable<global::System.Int64> value);
        partial void OnCategoryIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Boolean> IsActived
        {
            get
            {
                return _IsActived;
            }
            set
            {
                OnIsActivedChanging(value);
                ReportPropertyChanging("IsActived");
                _IsActived = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("IsActived");
                OnIsActivedChanged();
            }
        }
        private Nullable<global::System.Boolean> _IsActived;
        partial void OnIsActivedChanging(Nullable<global::System.Boolean> value);
        partial void OnIsActivedChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int64> KindID
        {
            get
            {
                return _KindID;
            }
            set
            {
                OnKindIDChanging(value);
                ReportPropertyChanging("KindID");
                _KindID = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("KindID");
                OnKindIDChanged();
            }
        }
        private Nullable<global::System.Int64> _KindID;
        partial void OnKindIDChanging(Nullable<global::System.Int64> value);
        partial void OnKindIDChanged();

        #endregion
    
        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute("SmartFlashCardDBModel", "FK_BackSide_0", "BackSide")]
        public EntityCollection<BackSide> BackSides
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<BackSide>("SmartFlashCardDBModel.FK_BackSide_0", "BackSide");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<BackSide>("SmartFlashCardDBModel.FK_BackSide_0", "BackSide", value);
                }
            }
        }

        #endregion
    }

    #endregion
    
}