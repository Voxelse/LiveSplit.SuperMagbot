using LiveSplit.Model;
using LiveSplit.UI.Components;
using Voxif.AutoSplitter;
using Voxif.IO;

[assembly: ComponentFactory(typeof(Factory))]
namespace LiveSplit.SuperMagbot {
    public partial class SuperMagbotComponent : Component {
        protected override SettingsInfo? ResetSettings => null;
        protected override EGameTime GameTimeType => EGameTime.GameTime;
        protected override bool IsGameTimeDefault => false;

        private SuperMagbotMemory memory;

        public SuperMagbotComponent(LiveSplitState state) : base(state) {
#if DEBUG
            logger = new ConsoleLogger();
#else
            logger = new  FileLogger("_" + Factory.ExAssembly.GetName().Name.Substring(10) + ".log");
#endif
            logger.StartLogger();

            memory = new SuperMagbotMemory(logger);

            settings = new TreeSettings(state, StartSettings, ResetSettings, OptionsSettings);

            remainingSplits = new RemainingDictionary(logger);
        }

        public override void Dispose() {
            memory.Dispose();
            memory = null;
            base.Dispose();
        }
    }
}