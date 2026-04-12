using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedPlan.Shared.Models
{
    public enum AppMode
    {
        SeedPlan,
        DahliaBox
    }
    public class AppState
    {
        public AppMode CurrentMode { get; private set; } = AppMode.SeedPlan;

        public event Action OnChange;
        public void SetMode(AppMode mode)
        {
            CurrentMode = mode;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
