using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tims;
using CPC.Control;
using CPC.POS.Model;

namespace CPC.Helper
{
    static class ToStatusModel
    {
        public static IList<StatusModel> ToStatusModelCollection(this ICollection<ComboItem> collection)
        {
            IList<StatusModel> itemModelList = new List<StatusModel>();
            StatusModel itemModel; ;
            foreach (ComboItem item in collection)
            {
                itemModel = new StatusModel();
                itemModel.Key = item.Value;
                itemModel.Content = item.Text;
                itemModelList.Add(itemModel);
            }
            return itemModelList;
        }
    }
}
