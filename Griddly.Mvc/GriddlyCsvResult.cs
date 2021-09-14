using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
#if NET45_OR_GREATER
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace Griddly.Mvc
{
    public class GriddlyCsvResult<T> : ActionResult
    {
        IEnumerable<T> _data;
        GriddlySettings _settings;
        string _name;
        GriddlyExportFormat _format;
        string _exportName;

        public GriddlyCsvResult(IEnumerable<T> data, GriddlySettings settings, string name, GriddlyExportFormat format = GriddlyExportFormat.Csv, string exportName = null)
        {
            _data = data;
            _settings = settings;
            _name = name;
            _format = format;
            _exportName = exportName;
        }

#if NET45_OR_GREATER
        public override void ExecuteResult(ControllerContext context)
#else
        public override async Task ExecuteResultAsync(ActionContext context)
#endif
        {
            string format = _format == GriddlyExportFormat.Tsv ? "tsv" : "csv";

            context.HttpContext.Response.ContentType = "text/" + format;

            context.HttpContext.Response.Headers.Add("content-disposition", "attachment;  filename=" + _name + "." + format);

#if NET45_OR_GREATER
            var tw = context.HttpContext.Response.Output;
#else
            using (var tw = new StreamWriter(context.HttpContext.Response.Body))
#endif
            using (CsvWriter w = new CsvWriter(tw, new CsvConfiguration(CultureInfo.CurrentCulture) { HasHeaderRecord = true, Delimiter = _format == GriddlyExportFormat.Tsv ? "\t" : "," }))
            {
                var export = _settings.Exports.FirstOrDefault(x => x.Name == _exportName);
                var columns = export == null ? _settings.Columns : export.Columns;

                if (export != null && export.UseGridColumns)
                    columns.InsertRange(0, _settings.Columns);

                columns.RemoveAll(x => !x.Visible || !x.RenderMode.HasFlag(ColumnRenderMode.Export));

                for (int i = 0; i < columns.Count; i++)
                    w.WriteField(columns[i].Caption);

                w.NextRecord();

                int y = 0;

                foreach (T row in _data)
                {
                    for (int x = 0; x < columns.Count; x++)
                    {
                        object renderedValue = columns[x].RenderCellValue(row, context.HttpContext, true);

                        w.WriteField(renderedValue);
                    }
#if NET45_OR_GREATER
                    w.NextRecord();
#else
                    await w.NextRecordAsync(); //Fix: Synchronous operations are disallowed. Call WriteAsync or set AllowSynchronousIO to true instead.
#endif
                    y++;
                }

            }
        }
    }
}
