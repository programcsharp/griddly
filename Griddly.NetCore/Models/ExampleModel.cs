
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Griddly.Models
{
    public class ExampleModel
    {
        IWebHostEnvironment _env;
        public ExampleModel(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ParentView { get; set; }

        public string GridView { get; set; }

        public string GridAction { get; set; }

        public string MapPath(string path)
        {
            return Path.Join(_env.ContentRootPath, path);
        }
    }
}