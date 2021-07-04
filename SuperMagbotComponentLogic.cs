using System;
using System.Collections.Generic;

namespace LiveSplit.SuperMagbot {
    public partial class SuperMagbotComponent {

        private readonly RemainingDictionary remainingSplits;
        private bool allLevelsSplit;

        private int totalGameTime;
        
        private int pauseFrames;
        private bool inMenu;

        public override bool Update() {
            if(!memory.Update()) {
                return false;
            }

            if(memory.TransitionType.New == 0) {
                if(pauseFrames > 0) {
                    pauseFrames--;
                }
            } else if(memory.TransitionType.New == 1) {
                pauseFrames = 6;
            }

            if(String.IsNullOrEmpty(memory.LevelName.New)) {
                inMenu = true;
                if(!String.IsNullOrEmpty(memory.LevelName.Old)) {
                    AddGameTime();
                }
            } else if(memory.ElapsedCentiseconds.Old > memory.ElapsedCentiseconds.New) {
                if(inMenu) {
                    if(!String.IsNullOrEmpty(memory.LevelName.New)) {
                        inMenu = false;
                    }
                } else {
                    AddGameTime();
                }
            }

            return true;
        }

        public override bool Start() {
            return memory.FadeOpacityPtr.New != default && memory.FadeOpacity.Old == 0 && memory.FadeOpacity.New > 0
                && memory.LevelGridIndex.New == 0 && (memory.WorldTitle.New == "MAGTERRA" || settings.Start == (int)EStart.AnyWorld)
                && memory.InLevelSelection();
        }

        public override void OnStart() {
            totalGameTime = 0;

            HashSet<string> splitsCopy = new HashSet<string>(settings.Splits);
            allLevelsSplit = splitsCopy.Remove("AllLevels");
            remainingSplits.Setup(splitsCopy);
        }

        public override bool Split() {
            bool canSplit;
            if(!inMenu) {
                canSplit = memory.EndLevelWidget.Old == default && memory.EndLevelWidget.New != default;
            } else {
                canSplit = "4-27".Equals(memory.LevelName.Old) && String.IsNullOrEmpty(memory.LevelName.New) && pauseFrames == 0;
            }
            return canSplit && (allLevelsSplit || (remainingSplits.ContainsKey("Level") && remainingSplits.Split("Level", memory.LevelName.Old)));
        }

        public override bool Reset() {
            return memory.InMainMenu();
        }

        public override void OnReset() {
            totalGameTime = 0;
        }

        private void AddGameTime() {
            if(timer.CurrentState.CurrentPhase == Model.TimerPhase.NotRunning) {
                return;
            }
            totalGameTime += memory.ElapsedCentiseconds.Old * 10;
        }

        public override TimeSpan? GameTime() {
            return TimeSpan.FromMilliseconds(totalGameTime + (!inMenu ? memory.ElapsedCentiseconds.New * 10 : 0));
        }
    }
}