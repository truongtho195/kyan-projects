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
    
    public partial class Category
    {
        public Category()
        {
            this.BlogEntries = new HashSet<BlogEntry>();
        }
    
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public Nullable<int> Parent { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<int> Posts { get; set; }
    
        public virtual ICollection<BlogEntry> BlogEntries { get; set; }
    }
}
