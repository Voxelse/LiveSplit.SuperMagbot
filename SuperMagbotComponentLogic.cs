using System;
using System.Collections.Generic;

namespace LiveSplit.SuperMagbot {
    public partial class SuperMagbotComponent {

        private int deathCount;

        private readonly RemainingDictionary remainingSplits;
        private bool allLevelsSplit;

        private int totalGameTime;
        
        private int pauseFrames;
        private bool inMenu;

        public override bool Update() {
            if(!memory.Update()) {
                return false;
            }

            if(DeathCounter && memory.MapTriesIncreased()) {
                UpdateDeathCounter(++deathCount);
            }

            if(memory.TransitionType.New == ETransitionType.None) {
                if(pauseFrames > 0) {
                    pauseFrames--;
                }
            } else if(memory.TransitionType.New == ETransitionType.Paused) {
                pauseFrames = 6;
            }

            memory.ElapsedCentiseconds.ForceUpdate();

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
            return memory.FadeOpacity.Old == 0 && memory.FadeOpacity.New > 0
                && memory.LevelGridIndex.New == 0 && (memory.WorldTitle.New == "MAGTERRA" || settings.Start == (int)EStart.AnyWorld)
                && memory.InLevelSelection();
        }

        public override void OnStart() {
            ResetData();

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
            return memory.InMainMenu() || memory.FromLevelToWorld();
        }

        public override void OnReset() {
            ResetData();
        }

        public override TimeSpan? GameTime() {
            return TimeSpan.FromMilliseconds(totalGameTime + (!inMenu ? memory.ElapsedCentiseconds.New * 10 : 0));
        }

        private void AddGameTime() {
            if(timer.CurrentState.CurrentPhase == Model.TimerPhase.NotRunning) {
                return;
            }
            totalGameTime += memory.ElapsedCentiseconds.Old * 10;
        }

        private void ResetData() {
            totalGameTime = 0;
            UpdateDeathCounter(deathCount = 0);
        }

        private void UpdateDeathCounter(int count) {
            deathCounterComponent.Value = count.ToString();
        }
    }
}