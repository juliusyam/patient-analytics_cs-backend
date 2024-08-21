using PatientAnalytics.Blazor.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PatientAnalytics.Blazor.Localization;

namespace PatientAnalytics.Blazor.Controllers;

public class BlazorAlertController
{
    public bool IsVisible => Modal is not null;
    
    public Modal? Modal { get; private set; }

    public void OnDisplayAlert(IStringLocalizer<Localized> localized, string titleKey, Action handleProceed)
    {
        Modal = new Modal(localized, localized[titleKey], handleProceed, RemovalModal);
    }

    public void RemovalModal() => Modal = null;
}