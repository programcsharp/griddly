using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public abstract class GriddlyFilter
    {
        string _name;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    NamePlural = PluralizationService.CreateService(CultureInfo.CurrentUICulture).Pluralize(value);
                else
                    NamePlural = null;

                _name = value;
            }
        }

        public string NamePlural { get; set; }

        public string Field { get; set; }
        public object Default { get; set; }
        public virtual FilterDataType DataType { get; set; }

        public string GetFormattedValue(object value)
        {
            return Extensions.GetFormattedValue(value, DataType);
        }

        public string GetEditValue(object value)
        {
            FilterDataType dataType = DataType;

            if (dataType == FilterDataType.Currency)
                dataType = FilterDataType.Decimal;

            return Extensions.GetFormattedValue(value, dataType);
        }
    }

    public class GriddlyFilterBox : GriddlyFilter
    {
        public GriddlyFilterBox()
        {
            DataType = FilterDataType.String;
        }
    }

    public class GriddlyFilterRange : GriddlyFilter
    {
        public GriddlyFilterRange()
        {
            DataType = FilterDataType.Decimal;
        }

        public override FilterDataType DataType
        {
            get
            {
                return base.DataType;
            }
            set
            {
                if (value == FilterDataType.String)
                    throw new ArgumentOutOfRangeException("String is not a valid DataType for Range filter.");

                base.DataType = value;
            }
        }
        public string FieldEnd { get; set; }
        public object DefaultEnd { get; set; }
    }

    public class GriddlyFilterList : GriddlyFilter
    {
        public GriddlyFilterList()
        {
            IsMultiple = true;
            IsNoneAll = true;
            DisplayItemCount = 1;
        }

        public List<SelectListItem> Items { get; set; }
        public List<SelectListItem> SelectableItems { get; internal set; }

        public bool IsMultiple { get; set; }
        public bool IsNoneAll { get; set; }
        public bool IsNullable { get; set; }

        public int DisplayItemCount { get; set; }
    }

    public enum FilterDataType
    {
        Other = 0,
        String = 1,
        Integer = 2,
        Decimal = 3,
        Currency = 4,
        Date = 5,
        //Percent = 6,
    }
}
