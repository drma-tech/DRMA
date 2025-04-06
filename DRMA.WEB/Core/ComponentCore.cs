using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace DRMA.WEB.Core
{
    /// <summary>
    /// if you implement the OnAfterRenderAsync method, call 'await base.OnAfterRenderAsync(firstRender);'
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ComponentCore<T> : ComponentBase where T : class
    {
        [Inject] protected ILogger<T> Logger { get; set; } = default!;
        [Inject] protected INotificationService Toast { get; set; } = default!;
        [Inject] protected IModalService ModalService { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;

        protected virtual Task LoadDataRender()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// if you implement the OnAfterRenderAsync method, call 'await base.OnAfterRenderAsync(firstRender);'
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            try
            {
                if (firstRender)
                {
                    await LoadDataRender();

                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                ex.ProcessException(Toast, Logger);
            }
        }
    }

    /// <summary>
    /// if you implement the OnAfterRenderAsync method, call 'await base.OnAfterRenderAsync(firstRender);'
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PageCore<T> : ComponentCore<T> where T : class
    {
        [CascadingParameter] protected Task<AuthenticationState>? authenticationState { get; set; }

        protected bool IsAuthenticated { get; set; } = false;
        protected ClaimsPrincipal? User { get; set; }
        protected string? UserId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (authenticationState is not null)
            {
                var authState = await authenticationState;

                User = authState?.User;
                IsAuthenticated = User?.Identity is not null && User.Identity.IsAuthenticated;
                UserId = User?.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            }
        }
    }
}