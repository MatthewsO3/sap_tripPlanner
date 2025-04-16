
using SAPbouiCOM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace tripPlanner.Services.Form
{
    public class FormService : IFormService, IDisposable
    {
        StreamReader formStream;

        public FormService(string formSRF)
        {
            formStream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"tripPlanner.Forms.{formSRF}"));
        }

        public Stream Stream
        {
            get
            {
                return formStream.BaseStream;
            }
        }

        public string ExportToString(Dictionary<string, string> replaces = null)
        {
            if (formStream.EndOfStream)
                formStream.BaseStream.Position = 0;

            string xmlString = formStream.ReadToEnd();

            if (replaces != null)
            {
                foreach (var replace in replaces)
                {
                    xmlString = xmlString.Replace(replace.Key, replace.Value);
                }
            }

            return xmlString;
        }

        public void Dispose()
        {
            formStream?.Dispose();
        }

    }

}
