using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Griddly.Mvc
{
    public class GriddlyExcelResult<T> : ActionResult
    {
        IEnumerable<T> _data;
        GriddlySettings _settings;
        string _name;

        public GriddlyExcelResult(IEnumerable<T> data, GriddlySettings settings, string name)
        {
            _data = data;
            _settings = settings;
            _name = name;
        }

        // static readonly Regex _aMatch = new Regex(@"<a\s[^>]*\s?href=""(.*?)"">(.*?)</a>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

                int y = 0;

                foreach (T row in _data)
                {
                    for (int x = 0; x < _settings.Columns.Count; x++)
                    {
                        object renderedValue = _settings.Columns[x].RenderCellValue(row, true);

                        ExcelRange cell = ws.Cells[y + 2, x + 1];
                                        
                        cell.Value = renderedValue;

                        if (renderedValue as DateTime? != null)
                        {
                            cell.Style.Numberformat.Format = "mm/dd/yyyy";
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }
                        else if (_settings.Columns[x].Format == "c")
                        {
                            cell.Style.Numberformat.Format = "\"$\"#,##0.00_);(\"$\"#,##0.00)";
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }
                    }

                    y++;
                }

                for (int i = 0; i < _settings.Columns.Count; i++)
                    ws.Column(i + 1).AutoFit();

                context.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.HttpContext.Response.AddHeader("content-disposition", "attachment;  filename=" + _name + ".xlsx");

                p.SaveAs(context.HttpContext.Response.OutputStream);
            }
        }
    }
}
