
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

        /// <summary>
        /// Initializes a new instance of the <see cref="FormService"/> class, loading the specified form definition from embedded resources.
        /// </summary>
        /// <param name="formSRF">The name of the SRF (Screen Form) file embedded in the assembly (e.g., "turatervezo_form.xml").</param>
        /// <exception cref="ArgumentNullException">Thrown when the specified form resource is not found in the assembly.</exception>
        public FormService(string formSRF)
        {
            formStream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"tripPlanner.Forms.{formSRF}"));
        }

        /// <summary>
        /// Gets the underlying stream of the form definition.
        /// </summary>
        /// <value>The stream associated with the form definition resource.</value>
        public Stream Stream
        {
            get
            {
                return formStream.BaseStream;
            }
        }

        /// <summary>
        /// Reads the form definition from the resource stream and applies optional string replacements.
        /// </summary>
        /// <param name="replaces">A dictionary of key-value pairs for replacing placeholders in the form definition XML. Keys are placeholders, and values are their replacements.</param>
        /// <returns>The form definition as a string with any specified replacements applied.</returns>
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

        /// <summary>
        /// Disposes of the resources used by the <see cref="FormService"/> instance, specifically the underlying stream reader.
        /// </summary>
        public void Dispose()
        {
            formStream?.Dispose();
        }

    }

}
