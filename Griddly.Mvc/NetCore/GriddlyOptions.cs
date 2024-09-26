namespace Griddly.Mvc;

public class GriddlyOptions
{
    public string DefaultEmptyGridMessage
    {
        get { return GriddlySettings.DefaultEmptyGridMessage; }
        set { GriddlySettings.DefaultEmptyGridMessage = value; }
    }
    public GriddlyCss DefaultCss 
    {
        get { return GriddlySettings.DefaultCss; }
        set { GriddlySettings.DefaultCss = value; }
    }
    public void ConfigureBootstrap4Defaults()
    {
        GriddlySettings.ConfigureBootstrap4Defaults();
    }
    public string ButtonTemplate
    {
        get { return GriddlySettings.ButtonTemplate; }
        set { GriddlySettings.ButtonTemplate = value; }
    }
    public string ButtonListTemplate
    {
        get { return GriddlySettings.ButtonListTemplate; }
        set { GriddlySettings.ButtonListTemplate = value; }
    }
    public HtmlString DefaultBoolTrueHtml
    {
        get { return GriddlySettings.DefaultBoolTrueHtml; }
        set { GriddlySettings.DefaultBoolTrueHtml = value; }
    }
    public HtmlString DefaultBoolFalseHtml
    {
        get { return GriddlySettings.DefaultBoolFalseHtml; }
        set { GriddlySettings.DefaultBoolFalseHtml = value; }
    }
    public string ColumnLinkTemplate
    {
        get { return GriddlySettings.ColumnLinkTemplate; }
        set { GriddlySettings.ColumnLinkTemplate = value; }
    }
    public int? DefaultPageSize
    {
        get { return GriddlySettings.DefaultPageSize; }
        set { GriddlySettings.DefaultPageSize = value; }
    }
    public FilterMode? DefaultInitialFilterMode
    {
        get { return GriddlySettings.DefaultInitialFilterMode; }
        set { GriddlySettings.DefaultInitialFilterMode = value; }
    }
    public bool DefaultShowRowSelectCount
    {
        get { return GriddlySettings.DefaultShowRowSelectCount; }
        set { GriddlySettings.DefaultShowRowSelectCount = value; }
    }
    public bool ExportCurrencySymbol
    {
        get { return GriddlySettings.ExportCurrencySymbol; }
        set { GriddlySettings.ExportCurrencySymbol = value; }
    }
    public bool DisableHistoryParameters
    {
        get { return GriddlySettings.DisableHistoryParameters; }
        set { GriddlySettings.DisableHistoryParameters = value; }
    }
    public bool IsCookiesDisabledDefault
    {
        get { return GriddlySettings.IsCookiesDisabledDefault; }
        set { GriddlySettings.IsCookiesDisabledDefault = value; }
    }
}