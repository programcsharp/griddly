using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using CsvHelper;
using CsvHelper.Configuration;
using OfficeOpenXml;

namespace Griddly.Mvc
{
    public class GriddlyCsvResult<T> : ActionResult
    {
        IEnumerable<T> _data;
        GriddlySettings _settings;
        string _name;
        GriddlyExportFormat _format;

        public GriddlyCsvResult(IEnumerable<T> data, GriddlySettings settings, string name, GriddlyExportFormat format = GriddlyExportFormat.Csv)
        {
            _data = data;
            _settings = settings;
            _name = name;
            _format = format;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            string format = _format == GriddlyExportFormat.Tsv ? "tsv" : "csv";

            context.HttpContext.Response.ContentType = "text/" + format;
            context.HttpContext.Response.AddHeader("content-disposition", "attachment;  filename=" + _name + "." + format);

            using (CsvWriter w = new CsvWriter(context.HttpContext.Response.Output, new CsvConfiguration() { HasHeaderRecord = true, Delimiter = _format == GriddlyExportFormat.Tsv ? "\t" : "," }))
            {
                for (int i = 0; i < _settings.Columns.Count; i++)
                    w.WriteField(_settings.Columns[i].Caption);

                w.NextRecord();

                int y = 0;

                foreach (T row in _data)
                {
                    for (int x = 0; x < _settings.Columns.Count; x++)
                    {
                        object renderedValue = _settings.Columns[x].RenderCellValue(row, true);

                        w.WriteField(renderedValue);
                    }

                    w.NextRecord();

                    y++;
                }
            }
        }
    }
}
