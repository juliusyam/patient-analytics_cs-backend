using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PatientAnalytics.Blazor.Localization;

namespace PatientAnalytics.Blazor.Models;

public class Modal
{
    private static readonly IOptions<LocalizationOptions> LocalizationOptionsDefined =
        Options.Create(new LocalizationOptions());

    private static readonly ResourceManagerStringLocalizerFactory LocalizerFactory =
        new(LocalizationOptionsDefined, NullLoggerFactory.Instance);
    
    private static readonly IStringLocalizer<Localized> Localized = 
        new StringLocalizer<Localized>(LocalizerFactory);
    
    public string Title { get; private set; } = "";
    public string ConfirmButtonText { get; private set; } = Localized["Button_Confirm"];
    public string CancelButtonText { get; private set; } = Localized["Button_Cancel"];
    public Action OnConfirm { get; private set; } = null!;
    public Action OnCancel { get; private set; } = null!;

    public static Modal ConfirmUserDeactivation(Action onConfirm, Action onCancel)
    {
        return new Modal()
        {
            Title = Localized["Title_DeactivateUser"],
            OnConfirm = onConfirm,
            OnCancel = onCancel
        };
    }
    
    public static Modal ConfirmUserActivation(Action onConfirm, Action onCancel)
    {
        return new Modal()
        {
            Title = Localized["Title_ActivateUser"],
            OnConfirm = onConfirm,
            OnCancel = onCancel
        };
    }
}