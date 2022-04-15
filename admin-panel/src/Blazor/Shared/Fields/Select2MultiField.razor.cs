using System.Linq;
using System.Threading.Tasks;

namespace ED.AdminPanel.Blazor.Shared.Fields
{
    public partial class Select2MultiField
    {
        protected override async Task NotifyValueChangedAsync(Select2Option[] selected)
        {
            await this.ValueChanged.InvokeAsync(selected.Select(s => s.Id).ToArray());
        }
    }
}
