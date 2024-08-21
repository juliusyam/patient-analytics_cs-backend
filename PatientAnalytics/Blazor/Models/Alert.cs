using Microsoft.Extensions.Localization;
using PatientAnalytics.Blazor.Localization;

namespace PatientAnalytics.Blazor.Models;

public class Alert
{ 
    public Alert(
        IStringLocalizer<Localized> localized,
        string title,
        Action onConfirm,
        Action onCancel,
        string? confirmButtonText = null,
        string? cancelButtonText = null
        )
    {
        Title = title;
        OnConfirm = onConfirm;
        OnCancel = onCancel;
        ConfirmButtonText = confirmButtonText ?? localized["Button_Confirm"];
        CancelButtonText = cancelButtonText ?? localized["Button_Cancel"];
    }
    
    public string Title { get; private set; }
    public string ConfirmButtonText { get; private set; }
    public string CancelButtonText { get; private set; }
    public Action OnConfirm { get; private set; }
    public Action OnCancel { get; private set; }
}