using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using OfficeOpenXml;

namespace Griddly.Mvc
{
    public class GriddlyExcelResult<T> : ActionResult
    {
        IList<T> _data;
        GriddlySettings _settings;
        string _name;

        public GriddlyExcelResult(IList<T> data, GriddlySettings settings, string name)
        {
            _data = data;
            _settings = settings;
            _name = name;
        }

        static readonly Regex _aMatch = new Regex(@"<a\s[^>]*\s?href=""(.*?)"">(.*?)</a>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex _htmlMatch = new Regex(@"<[^>]*>", RegexOptions.Compiled);

        public override void ExecuteResult(ControllerContext context)
        {
            using (ExcelPackage p = new ExcelPackage())
            {
                ExcelWorksheet ws = p.Workbook.Worksheets.Add(_name);

                for (int i = 0; i < _settings.Columns.Count; i++)
                {
                    ws.Cells[1, i + 1].Value = _settings.Columns[i].Caption;
                    ws.Cells[1, i + 1].Style.Font.Bold = true;
                }

                for (int y = 0; y < _data.Count; y++)
                {
                    T row = _data[y];

                    for (int x = 0; x < _settings.Columns.Count; x++)
                    {
                        object renderedValue = _settings.Columns[x].RenderCell(row, false);

                        string value;

                        if (renderedValue != null)
                            value = renderedValue.ToString();
                        else
                            value = "";

                        if (value.StartsWith("<a"))
                        {
                            Match match = _aMatch.Match(value);

                            value = match.Groups[2].Value;

                            ws.Cells[y + 2, x + 1].Hyperlink = new Uri(context.HttpContext.Request.Url, match.Groups[1].Value);
                        }
                        else
                        {
                            value = _htmlMatch.Replace(value, "").Trim().Replace("  ", " ");
                        }

                        ws.Cells[y + 2, x + 1].Value = value;
                    }
                }

                context.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.HttpContext.Response.AddHeader("content-disposition", "attachment;  filename=" + _name + ".xlsx");
                context.HttpContext.Response.BinaryWrite(p.GetAsByteArray());
            }
        }
    }
}
