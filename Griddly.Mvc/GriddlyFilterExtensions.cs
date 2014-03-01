using System;
using System.Collections;
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

            if (field == null)
                field = GetField(column);

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

            if (field == null)
                field = GetField(column) + "Start";
            if (fieldEnd == null)
                fieldEnd = GetField(column) + "End";

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

        public static GriddlyFilterList FilterList(this GriddlyColumn column, IEnumerable<SelectListItem> items, bool isMultiple = true, bool defaultSelectAll = true, string nullItemText = null, bool isNoneAll = true, string field = null, string name = null)
        {
            return column.FilterList(items, (object)null, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, name);
        }

        public static GriddlyFilterList FilterList(this GriddlyColumn column, IEnumerable<SelectListItem> items, Array defaultValue, bool isMultiple = true, bool defaultSelectAll = true, string nullItemText = null, bool isNoneAll = true, string field = null, string name = null)
        {
            return column.FilterList(items, (object)defaultValue, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, name);
        }

        public static GriddlyFilterList FilterList(this GriddlyColumn column, IEnumerable<SelectListItem> items, object defaultValue, bool isMultiple = true, bool defaultSelectAll = true, string nullItemText = null, bool isNoneAll = true, string field = null, string name = null)
        {
            if (name == null)
                name = column.Caption;

            if (field == null)
                field = GetField(column);

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Name must be specified.");
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentNullException("field", "Field must be specified.");

            List<SelectListItem> itemsList = new List<SelectListItem>();

            if (!string.IsNullOrWhiteSpace(nullItemText))
                itemsList.Add(new SelectListItem() { Text = nullItemText, Value = "" });

            itemsList.AddRange(items);

            if (isMultiple && defaultSelectAll && defaultValue == null)
            {
                foreach (SelectListItem item in itemsList)
                    item.Selected = true;
            }
            else if (!isMultiple || defaultValue != null)
            {
                if (defaultValue != null)
                {
                    IEnumerable defaultValues = defaultValue as IEnumerable;

                    if (defaultValues == null)
                    {
                        string value = GetDefaultValueString(defaultValue);

                        foreach (SelectListItem item in itemsList)
                            item.Selected = item.Value == value;
                    }
                    else
                    {
                        foreach (object value in defaultValues)
                        {
                            string valueString = GetDefaultValueString(value);

                            foreach (SelectListItem item in itemsList.Where(x => x.Value == valueString))
                                item.Selected = true;
                        }
                    }
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
                Default = defaultValue,
                Items = itemsList,
                Name = name,
                IsMultiple = isMultiple,
                IsNoneAll = isNoneAll,
                IsNullable = !string.IsNullOrWhiteSpace(nullItemText)
            };
        }

        static string GetDefaultValueString(object defaultValue)
        {
            if (defaultValue.GetType().IsEnum)
                return Convert.ToInt32(defaultValue).ToString();
            else
                return defaultValue.ToString();
        }

        static string GetField(GriddlyColumn column)
        {
            string value = null;

            if (column.SortField != null)
            {
                value = column.SortField.Split('.').Last();

                if (value.Length > 1)
                    value = char.ToLower(value[0]) + value.Substring(1);
            }

            return value;
        }

        public static GriddlyFilterList FilterEnum<T>(this GriddlyColumn column, bool isMultiple = true, bool defaultSelectAll = true, string nullItemText = null, bool isNoneAll = true, string field = null, string name = null)
            where T : struct
        {
            return column.FilterList(Extensions.ToSelectListItems<T>(), null, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, name);
        }

        public static GriddlyFilterList FilterEnum<T>(this GriddlyColumn column, T? defaultValue, bool isMultiple = true, bool defaultSelectAll = true, string nullItemText = null, bool isNoneAll = true, string field = null, string name = null)
            where T : struct
        {
            return column.FilterList(Extensions.ToSelectListItems<T>(), defaultValue, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, name);
        }

        public static GriddlyFilterList FilterEnum<T>(this GriddlyColumn column, T[] defaultValue, bool isMultiple = true, bool defaultSelectAll = true, string nullItemText = null, bool isNoneAll = true, string field = null, string name = null)
            where T : struct
        {
            return column.FilterList(Extensions.ToSelectListItems<T>(), defaultValue, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, name);
        }

        public static GriddlyFilterList FilterEnum<T>(this GriddlyColumn column, T?[] defaultValue, bool isMultiple = true, bool defaultSelectAll = true, string nullItemText = null, bool isNoneAll = true, string field = null, string name = null)
            where T : struct
        {
            return column.FilterList(Extensions.ToSelectListItems<T>(), defaultValue, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, name);
        }

        public static GriddlyFilterList FilterBool(this GriddlyColumn column, bool isMultiple = false, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = false, string trueLabel = "Yes", string falseLabel = "No", string field = null, string name = null)
        {
            return column.FilterBool((object)null, isMultiple, defaultSelectAll, nullItemText, isNoneAll, trueLabel, falseLabel, field, name);
        }

        public static GriddlyFilterList FilterBool(this GriddlyColumn column, bool? defaultValue, bool isMultiple = false, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = false, string trueLabel = "Yes", string falseLabel = "No", string field = null, string name = null)
        {
            return column.FilterBool(defaultValue, isMultiple, defaultSelectAll, nullItemText, isNoneAll, trueLabel, falseLabel, field, name);
        }

        public static GriddlyFilterList FilterBool(this GriddlyColumn column, bool[] defaultValue, bool isMultiple = false, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = false, string trueLabel = "Yes", string falseLabel = "No", string field = null, string name = null)
        {
            return column.FilterBool(defaultValue, isMultiple, defaultSelectAll, nullItemText, isNoneAll, trueLabel, falseLabel, field, name);
        }

        public static GriddlyFilterList FilterBool(this GriddlyColumn column, bool?[] defaultValue, bool isMultiple = false, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = false, string trueLabel = "Yes", string falseLabel = "No", string field = null, string name = null)
        {
            return column.FilterBool(defaultValue, isMultiple, defaultSelectAll, nullItemText, isNoneAll, trueLabel, falseLabel, field, name);
        }

        static GriddlyFilterList FilterBool(this GriddlyColumn column, object defaultValue, bool isMultiple = false, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = false, string trueLabel = "Yes", string falseLabel = "No", string field = null, string name = null)
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
