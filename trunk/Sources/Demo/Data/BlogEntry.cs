//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BlogNet.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class BlogEntry
    {
        public BlogEntry()
        {
            this.EntryTagDetails = new HashSet<EntryTagDetail>();
        }
    
        public int BlogEntryID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public Nullable<int> AuthorID { get; set; }
        public string Slug { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModified { get; set; }
        public Nullable<int> CategoryID { get; set; }
        public string Description { get; set; }
    
        public virtual Account Account { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<EntryTagDetail> EntryTagDetails { get; set; }
    }
}