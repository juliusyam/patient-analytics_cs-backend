using Microsoft.Extensions.Localization;
using PatientAnalytics.Blazor.Localization;
using PatientAnalytics.Blazor.Models;

namespace PatientAnalytics.Blazor.Controllers;

public class BlazorAlertController
{
    public bool IsVisible => Alert is not null;
    
    public Alert? Alert { get; private set; }

    public void OnDisplayAlert(IStringLocalizer<Localized> localized, string titleKey, Action handleAction)
    {
        Alert = new Alert(localized, localized[titleKey], handleAction, RemovalModal);
    }

    public void RemovalModal() => Alert = null;
}