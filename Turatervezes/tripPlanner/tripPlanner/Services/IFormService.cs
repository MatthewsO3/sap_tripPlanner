using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tripPlanner.Services.Form
{
    public interface IFormService
{
    Stream Stream { get; }

    string ExportToString(Dictionary<string, string> replaces);
}

}
