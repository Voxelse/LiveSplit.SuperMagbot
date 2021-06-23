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

            if("4-27".Equals(memory.LevelName.New)) {
                if(memory.TransitionType.New == 0 && pauseFrames > 0) {
                    pauseFrames--;
                }
                if(memory.TransitionType.New == 1) {
                    pauseFrames = 6;
                }
            }

            if(!String.IsNullOrEmpty(memory.LevelName.Old) && String.IsNullOrEmpty(memory.LevelName.New)) {
                inMenu = true;
                AddGameTime();
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
            return "1-1".Equals(memory.LevelName.New) && (memory.ElapsedCentiseconds.Old > memory.ElapsedCentiseconds.New);
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