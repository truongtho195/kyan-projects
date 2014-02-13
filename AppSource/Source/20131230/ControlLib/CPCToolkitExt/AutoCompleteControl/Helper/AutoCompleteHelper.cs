using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CPCToolkitExt
{
    public static class AutoCompleteHelper
    {
        #region SetTypeBinding
        /// <summary>
        /// Get type of property 
        /// Return value
        /// </summary>
        /// <returns></returns>
        public static object SetTypeBinding(object data)
        {
            try
            {
                if (data == null) return null;
                //get type
                Type dataType = data.GetType() as Type;
                //return value for this data.
                if (dataType.Name.Equals("Int32"))
                    return int.Parse("0");
                else if (dataType.Name.Equals("Double"))
                    return Double.Parse("0");
                else if (dataType.Name.Equals("Decimal"))
                    return Decimal.Parse("0");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<SetTypeBinding>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            return null;
        }

        #endregion

        #region GetDataBinding
        /// <summary>
        /// Get type of property 
        /// Return value
        /// </summary>
        /// <returns></returns>
        public static string GetDataBinding(object data, string value)
        {
            try
            {
                if (data == null) return null;
                //get type
                Type dataType = data.GetType() as Type;
                //return value for this data.
                if (dataType.Name.Equals("Int32"))
                    return int.Parse(value).ToString();
                else if (dataType.Name.Equals("Double"))
                    return Double.Parse(value).ToString();
                else if (dataType.Name.Equals("Decimal"))
                    return Decimal.Parse(value).ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<SetTypeBinding>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            return string.Empty;
        }
        #endregion

        #region GetDataGridCell
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            try
            {
                int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < numVisuals; i++)
                {
                    Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                    child = v as T;
                    if (child == null)
                    {
                        child = GetVisualChild<T>(v);
                    }
                    if (child != null)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<GetVisualChild>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            return child;
        }

        public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int column)
        {
            try
            {
                if (row != null)
                {
                    DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);

                    if (presenter == null)
                    {
                        grid.ScrollIntoView(row, grid.Columns[column]);
                        presenter = GetVisualChild<DataGridCellsPresenter>(row);
                    }

                    DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                    return cell;
                }
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<<<GetCell>>>>>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            return null;
        }

        #endregion

        #region DeepClone
        /// <summary>
        /// Copy value 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object DeepClone(object obj)
        {
            try
            {
                object objResult = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, obj);
                    ms.Position = 0;
                    objResult = bf.Deserialize(ms);
                }
                return objResult;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<AutoCompleteHelper_DeepClone>>>>>>>>>>>>>" + ex.ToString());
            }
            return null;
        }
        #endregion
    }
}
