using Microsoft.AspNetCore.Components;
using MudBlazor;
using DRMA.WEB.Shared;

namespace DRMA.WEB.Core.Helper;

public static class PopupHelper
{
    public static readonly EventCallbackFactory Factory = new();

    public static async Task SettingsPopup(this IDialogService service)
    {
        await service.ShowAsync<SettingsPopup>("Settings", Options(MaxWidth.Small));
    }

    public static DialogOptions Options(MaxWidth width)
    {
        return new DialogOptions
        {
            CloseOnEscapeKey = true,
            CloseButton = true,
            Position = DialogPosition.Center,
            MaxWidth = width
        };
    }
}