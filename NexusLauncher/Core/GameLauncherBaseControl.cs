
using System.Windows.Controls;
using System.Windows.Media;

namespace Game_Launcher.Core
{
    // Concrete class for XAML root — satisfies abstract members
    public class GameLauncherBaseControl : GameLauncherBase
    {
        // Return null here; Cubical will override with actual controls
        public override Button PlayButton => null;
        public override Button OptionButton => null;
        public override ProgressBar ProgressBar => null;
        public override TextBlock ProgressLabel => null;

        public override SolidColorBrush DefaultButtonColor => null;
        public override SolidColorBrush DisabledButtonColor => null;
        public override SolidColorBrush DefaultTextColor => null;
        public override SolidColorBrush DisabledTextColor => null;

        public override string VersionUrl => string.Empty;
        public override string ZipUrl => string.Empty;
    }
}
