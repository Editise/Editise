using Editiser.Services;


namespace Editiser.Components
{
    public partial class ClearCache
    {
        private void ClearNow()
        {
            contentService.ClearCache();
            StateHasChanged();
            NavigationManager.NavigateTo(NavigationManager.Uri, true);
        }
    }
}
