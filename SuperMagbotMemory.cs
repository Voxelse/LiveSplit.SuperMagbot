using System;
using Voxif.AutoSplitter;
using Voxif.Helpers.Unreal;
using Voxif.IO;
using Voxif.Memory;

namespace LiveSplit.SuperMagbot {
    public class SuperMagbotMemory : Memory {

        protected override string[] ProcessNames => new string[] { "SuperMagbot-" };

        public Pointer<ETransitionType> TransitionType { get; private set; }

        public Pointer<int> ElapsedCentiseconds { get; private set; }
        public Pointer<IntPtr> EndLevelWidget { get; private set; }
        public StringPointer LevelName { get; private set; }

        public Pointer<float> FadeOpacity { get; private set; }

        private Pointer<int> GameModeFName { get; set; }
        private Pointer<int> MapTriesCounter { get; set; }

        private Pointer<int> SubLevelFName { get; set; }
        public Pointer<int> LevelGridIndex { get; private set; }
        public StringPointer WorldTitle { get; private set; }

        private int fnameMenuMap;
        private int fnameWorldMap;
        private int fnameLevelMap;
        private int fnameGameGameMode;

        private UnrealHelperTask unrealTask;

        public SuperMagbotMemory(Logger logger) : base(logger) {
            OnHook += () => {
                unrealTask = new UnrealHelperTask(game, logger);
                unrealTask.Run(new Version(4, 25), InitPointers);
            };

            OnExit += () => {
                if(unrealTask != null) {
                    unrealTask.Dispose();
                    unrealTask = null;
                }
            };
        }

        private void InitPointers(IUnrealHelper unreal) {
            NestedPointerFactory ptrFactory = new NestedPointerFactory(game);

            const string NameGameEngine = "GameEngine";
            const string NameMenuMap = "SMMenuMainMap";
            const string NameWorldMap = "SMMenuWorldSelectionMap";
            const string NameLevelMap = "SMLevelSelectionMap";
            const string NameGameGameMode = "SMGameGameModeBP_C";
            var fnames = unreal.GetFNames(NameGameEngine, NameMenuMap, NameWorldMap, NameLevelMap, NameGameGameMode);
            fnameMenuMap = fnames[NameMenuMap];
            fnameWorldMap = fnames[NameWorldMap];
            fnameLevelMap = fnames[NameLevelMap];
            fnameGameGameMode = fnames[NameGameGameMode];

            IntPtr gameEngine = unreal.GetUObject(fnames[NameGameEngine]);

            TransitionType = ptrFactory.Make<ETransitionType>(gameEngine + 0x8B8);

            var player = ptrFactory.Make<IntPtr>(gameEngine + 0xDE8, 0x38, 0x0);
            {
                var playerController = ptrFactory.Make<IntPtr>(player, 0x30);
                {
                    ElapsedCentiseconds = ptrFactory.Make<int>(playerController, 0x578, 0x338);

                    EndLevelWidget = ptrFactory.Make<IntPtr>(playerController, 0x580);

                    LevelName = ptrFactory.MakeString(playerController, 0x588, 0x260, 0x128, 0x48, 7*2);
                    LevelName.StringType = EStringType.UTF16;
                }

                var world = ptrFactory.Make<IntPtr>(player, 0x70, 0x78);
                {
                    FadeOpacity = ptrFactory.Make<float>(world, 0x30, 0xA8, 0x78, 0x230, 0x240, 0xC4);
                    FadeOpacity.UpdateOnNullPointer = false;

                    var gameMode = ptrFactory.Make<IntPtr>(world, 0x128);
                    {
                        GameModeFName = ptrFactory.Make<int>(gameMode, 0x18);
                        MapTriesCounter = ptrFactory.Make<int>(gameMode, 0x340);
                    }

                    var subLevel = ptrFactory.Make<IntPtr>(world, 0x148, 0x8);
                    {
                        SubLevelFName = ptrFactory.Make<int>(subLevel, 0x20, 0x18);

                        var levelSelector = ptrFactory.Make<IntPtr>(subLevel, 0xE8, 0x230);
                        {
                            LevelGridIndex = ptrFactory.Make<int>(levelSelector, 0x270, 0x158, 0x40);
                        
                            WorldTitle = ptrFactory.MakeString(levelSelector, 0x2A8, 0x128, 0x8, 0x0, 0x0);
                            WorldTitle.StringType = EStringType.UTF16;
                        }

                    }
                }
            }

            unrealTask = null;
        }

        public override bool Update() => base.Update() && unrealTask == null;

        public bool InLevelSelection() => SubLevelFName.New == fnameLevelMap;

        public bool InMainMenu() => SubLevelFName.New == fnameMenuMap;

        public bool FromLevelToWorld() => SubLevelFName.Old == fnameLevelMap && SubLevelFName.New == fnameWorldMap;

        public bool MapTriesIncreased() {
            return GameModeFName.New == fnameGameGameMode
                && MapTriesCounter.New > 1 && MapTriesCounter.Old < MapTriesCounter.New;
        }
    }

    public enum ETransitionType : byte {
        None,
        Paused,
        Loading,
        Saving,
        Connecting,
        Precaching,
        WaitingToConnect,
        MAX,
    }
}
