using Blazorise;
using Microsoft.AspNetCore.Components;

namespace DRMA.WEB.Core;

public static class PopupHelper
{
    public static readonly EventCallbackFactory Factory = new();

    public static async Task OpenPopup<TComponent>(this IModalService service,
        Action<ModalProviderParameterBuilder<TComponent>> parameters, ModalSize size)
        where TComponent : IComponent
    {
        await service.Show(null, parameters, Options(size));
    }

    private static ModalInstanceOptions Options(ModalSize size)
    {
        return new ModalInstanceOptions
        {
            UseModalStructure = false,
            Centered = true,
            Size = size
        };
    }
}