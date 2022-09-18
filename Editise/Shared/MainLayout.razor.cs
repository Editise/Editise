//this code will add a p-pagename class to the main div
using Microsoft.AspNetCore.Components;

namespace Editise.Shared
{
    public partial class MainLayout
    {
        public string PageType { get; set; }
        protected override void OnParametersSet()
        {
            PageType = $"p-{(this.Body.Target as RouteView)?.RouteData?.PageType?.Name.ToLower()}";
        }
    }
}
