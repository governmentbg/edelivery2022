using System.Linq;
using System.Threading.Tasks;

namespace ED.AdminPanel.Blazor.Shared.Fields
{
    public partial class Select2Field
    {
        protected override async Task NotifyValueChangedAsync(Select2Option[] selected)
        {
            await this.ValueChanged.InvokeAsync(selected.FirstOrDefault()?.Id);
        }
    }
}
