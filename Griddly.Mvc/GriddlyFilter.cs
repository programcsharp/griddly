using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public abstract class GriddlyFilter
    {
        public string Name { get; set; }

        public string NamePlural
        {
            get
            {
                return PluralizationService.CreateService(CultureInfo.CurrentUICulture).Pluralize(Name);
            }
        }

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
        }

        public List<SelectListItem> Items { get; set; }

        public bool IsMultiple { get; set; }
        public bool IsNoneAll { get; set; }
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
