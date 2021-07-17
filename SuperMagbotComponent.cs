using LiveSplit.Model;
using LiveSplit.RuntimeText;
using LiveSplit.UI.Components;
using System;
using System.ComponentModel;
using System.Linq;
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

        public enum EOption {
            [Description("Death Counter"), Type(typeof(OptionCheckBox))]
            DeathCounter
        }

        protected override SettingsInfo? StartSettings => new SettingsInfo((int)EStart.NewGame, GetEnumDescriptions<EStart>());
        protected override OptionsInfo? OptionsSettings => new OptionsInfo(null, CreateControlsFromEnum<EOption>());
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
            settings.OptionChanged += OptionChanged;

            remainingSplits = new RemainingDictionary(logger);
        }

        private void OptionChanged(object sender, OptionEventArgs e) {
            switch(Enum.Parse(typeof(EOption), e.Name)) {
                case EOption.DeathCounter:
                    DeathCounter = e.State == 1;
                    break;
            }
        }

        public override void Dispose() {
            settings.OptionChanged -= OptionChanged;
            DeathCounter = false;
            memory.Dispose();
            memory = null;
            base.Dispose();
        }

        private const string DeathCounterName = "Death Counter";
        private RuntimeTextComponent deathCounterComponent = null;
        private bool deathCounter = false;
        public bool DeathCounter {
            get => deathCounter;
            set {
                if(deathCounter = value) {
                    if(deathCounterComponent == null) {
                        deathCounterComponent = (RuntimeTextComponent)timer.CurrentState.Layout.Components.FirstOrDefault(c => c.ComponentName == "Runtime Text" || c.ComponentName == DeathCounterName);
                        if(deathCounterComponent == null) {
                            deathCounterComponent = new RuntimeTextComponent(timer.CurrentState, DeathCounterName, "Deaths") {
                                Value = "0"
                            };
                            timer.CurrentState.Layout.LayoutComponents.Add(new LayoutComponent("LiveSplit.RuntimeText.dll", deathCounterComponent));
                        }
                    }
                } else {
                    if(deathCounterComponent != null) {
                        foreach(ILayoutComponent component in timer.CurrentState.Layout.LayoutComponents) {
                            if(component.Component.ComponentName == "Runtime Text" || component.Component.ComponentName == DeathCounterName) {
                                timer.CurrentState.Layout.LayoutComponents.Remove(component);
                                break;
                            }
                        }
                        deathCounterComponent = null;
                    }
                }
            }
        }
    }
}