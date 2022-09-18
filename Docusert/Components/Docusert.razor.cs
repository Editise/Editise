using System;
using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace adapptTesters.Components
{
    public partial class Docusert
    {
        [Parameter]
        public string DocSelect { get; set; }
        [Parameter]
        public string BlockSelect { get; set; }
        [Parameter]
        public bool Plaintext { get; set; }
        public Docusert()
        {
        }
        private string StripHTML(string input)
        {
            string retval = "";
            if (!string.IsNullOrEmpty(input))
            {
                retval = Regex.Replace(input, "<.*?>", String.Empty);
            }
            return retval;
        }
    }
}

