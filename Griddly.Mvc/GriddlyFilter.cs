using System.Globalization;
#if NETFRAMEWORK
using System.Data.Entity.Design.PluralizationServices;
#endif

namespace Griddly.Mvc;

public abstract class GriddlyFilter
{
    string _caption;

#if NETFRAMEWORK
    static readonly PluralizationService _pluralizationService = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-US"));
#else
    static readonly PluralizationServiceInstance _pluralizationService = new PluralizationServiceInstance();
#endif

    public string Caption
    {
        get
        {
            return _caption;
        }
        set
        {
            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en" && !string.IsNullOrWhiteSpace(value))
            {
                CaptionPlural = _pluralizationService.Pluralize(value);
            }
            else
            {
                CaptionPlural = Caption;
            }

            _caption = value;
            CaptionInline = value;
        }
    }

    public string CaptionPlural { get; set; }
    public string CaptionInline { get; set; }

    public string HtmlClass { get; set; }

    public string Group { get; set; }

    public string Field { get; set; }
    public virtual FilterDataType DataType { get; set; }

    public IDictionary<string, object> InputHtmlAttributes { get; set; }

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
    public bool DefaultSelectAll { get; set; }
    public bool DisplayIncludeCaption { get; set; }

    public int DisplayItemCount { get; set; }

    public void SetSelectedItems(object value)
    {
        if (IsMultiple && DefaultSelectAll && !IsNoneAll && value == null)
        {
            // TODO: set default value to all selected
            foreach (SelectListItem item in SelectableItems)
                item.Selected = true;
        }
        else if (!IsMultiple || value != null)
        {
            if (value != null)
            {
                IEnumerable defaultValues = value as IEnumerable;

                if (defaultValues == null || value is string)
                {
                    string valueString = GetValueString(value);

                    foreach (SelectListItem item in SelectableItems)
                        item.Selected = item.Value == valueString;
                }
                else
                {
                    foreach (object valueObject in defaultValues)
                    {
                        if (valueObject != null)
                        {
                            string valueString = GetValueString(valueObject);

                            foreach (SelectListItem item in SelectableItems.Where(x => x.Value == valueString))
                                item.Selected = true;
                        }
                        else
                        {
                            foreach (SelectListItem item in SelectableItems.Where(x => string.IsNullOrWhiteSpace(x.Value)))
                                item.Selected = true;
                        }
                    }
                }
            }
            else
            {
                // TODO: set default value to all selected
                for (int i = 0; i < SelectableItems.Count; i++)
                    SelectableItems[i].Selected = (i == 0);
            }
        }
    }

    string GetValueString(object value)
    {
        string result = value.ToString();

        if (value is bool)
            result = result.ToLower();

        return result;
    }
    //string GetValueString(object value)
    //{
    //    Type valueType = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();

    //    if (valueType.IsEnum)
    //        return Convert.ChangeType(value, Enum.GetUnderlyingType(valueType)).ToString();
    //    else
    //        return value.ToString();
    //}
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
