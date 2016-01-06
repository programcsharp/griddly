using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

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

        public override void ExecuteResult(ControllerContext context)
        {
            string format = _format == GriddlyExportFormat.Tsv ? "tsv" : "csv";

            context.HttpContext.Response.ContentType = "text/" + format;
            context.HttpContext.Response.AddHeader("content-disposition", "attachment;  filename=" + _name + "." + format);

            using (CsvWriter w = new CsvWriter(context.HttpContext.Response.Output, new CsvConfiguration() { HasHeaderRecord = true, Delimiter = _format == GriddlyExportFormat.Tsv ? "\t" : "," }))
            {
                var export = _settings.Exports.FirstOrDefault(x => x.Name == _exportName);
                var columns = export == null ? _settings.Columns : export.Columns;
                if (export != null && export.UseGridColumns) columns.InsertRange(0, _settings.Columns);


                for (int i = 0; i < columns.Count; i++)
                    w.WriteField(columns[i].Caption);

                w.NextRecord();

                int y = 0;

                foreach (T row in _data)
                {
                    for (int x = 0; x < columns.Count; x++)
                    {
                        object renderedValue = columns[x].RenderCellValue(row, true);

                        w.WriteField(renderedValue);
                    }

                    w.NextRecord();

                    y++;
                }
            }
        }
    }
}
