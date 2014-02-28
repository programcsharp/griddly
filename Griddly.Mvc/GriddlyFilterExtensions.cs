using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public static class GriddlyFilterExtensions
    {
        public static GriddlyFilterBox FilterBox(this GriddlyColumn column, FilterDataType dataType = FilterDataType.Decimal, object defaultValue = null, string field = null, string name = null)
        {
            if (name == null)
                name = column.Caption;

            if (field == null && column.SortField != null && !column.SortField.Contains("."))
                field = column.SortField;

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Name must be specified.");
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentNullException("field", "Field must be specified.");

            return new GriddlyFilterBox()
            {
                Field = field,
                Default = defaultValue,
                Name = name,
                DataType = dataType
            };
        }

        public static GriddlyFilterRange FilterRange(this GriddlyColumn column, FilterDataType dataType = FilterDataType.Decimal, object defaultValue = null, object defaultValueEnd = null, string field = null, string fieldEnd = null, string name = null)
        {
            if (name == null)
                name = column.Caption;

            if (field == null && column.SortField != null && !column.SortField.Contains("."))
            {
                field = column.SortField + "Start";
                fieldEnd = column.SortField + "End";
            }

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Name must be specified.");
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentNullException("field", "Field must be specified.");
            if (string.IsNullOrWhiteSpace(fieldEnd))
                throw new ArgumentNullException("fieldEnd", "End field must be specified.");

            return new GriddlyFilterRange()
            {
                Field = field,
                FieldEnd = fieldEnd,
                Default = defaultValue,
                DefaultEnd = defaultValueEnd,
                Name = name,
                DataType = dataType
            };
        }

        public static GriddlyFilterList FilterList(this GriddlyColumn column, IEnumerable<SelectListItem> items, object defaultValue = null, bool isMultiple = true, bool defaultSelectAll = true, string nullItemText = null, bool isNoneAll = true, string field = null, string name = null)
        {
            if (name == null)
                name = column.Caption;

            if (field == null && column.SortField != null && !column.SortField.Contains("."))
                field = column.SortField;

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Name must be specified.");
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentNullException("field", "Field must be specified.");

            List<SelectListItem> itemsList = new List<SelectListItem>();

            if (!string.IsNullOrWhiteSpace(nullItemText))
                itemsList.Add(new SelectListItem() { Text = nullItemText });

            itemsList.AddRange(items);

            if (isMultiple && defaultSelectAll)
            {
                foreach (SelectListItem item in itemsList)
                    item.Selected = true;
            }
            else if (!isMultiple || defaultValue != null)
            {
                if (defaultValue != null)
                {
                    string value = defaultValue.ToString();

                    foreach (SelectListItem item in itemsList)
                        item.Selected = item.Value == value;
                }
                else
                {
                    for (int i = 0; i < itemsList.Count; i++)
                        itemsList[i].Selected = (i == 0);
                }
            }

            return new GriddlyFilterList()
            {
                Field = field,
                Items = itemsList,
                Name = name,
                IsMultiple = isMultiple,
                IsNoneAll = isNoneAll
            };
        }

        public static GriddlyFilterList FilterEnum<T>(this GriddlyColumn column, T? defaultValue = null, bool isMultiple = true, bool defaultSelectAll = true, string nullItemText = null, bool isNoneAll = true, string field = null, string name = null)
            where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("Type must be an Enum.");

            IEnumerable<SelectListItem> items = Extensions.ToSelectListItems<T>();
            string value = defaultValue != null ? Convert.ToInt32(defaultValue.Value).ToString() : null;

            return column.FilterList(items, value, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, name);
        }

        public static GriddlyFilterList FilterBool(this GriddlyColumn column, bool? defaultValue = null, bool isMultiple = false, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = false, string trueLabel = "Yes", string falseLabel = "No", string field = null, string name = null)
        {
            List<SelectListItem> items = new List<SelectListItem>()
            {
                new SelectListItem() { Value = "True", Text = trueLabel },
                new SelectListItem() { Value = "False", Text = falseLabel },
            };

            return column.FilterList(items, defaultValue, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, name);
        }
    }
}
