using LiveSplit.Model;
using LiveSplit.UI.Components;
using System.ComponentModel;
using Voxif.AutoSplitter;
using Voxif.IO;

[assembly: ComponentFactory(typeof(Factory))]
namespace LiveSplit.SuperMagbot {
    public partial class SuperMagbotComponent : Voxif.AutoSplitter.Component {

        public enum EStart {
            [Description("Off")]
            Off,
            [Description("New Game")]
            NewGame,
            [Description("Any World")]
            AnyWorld,
        }

        protected override SettingsInfo? StartSettings => new SettingsInfo((int)EStart.NewGame, GetEnumDescriptions<EStart>());
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