using System;
using Voxif.AutoSplitter;
using Voxif.Helpers.Unreal;
using Voxif.IO;
using Voxif.Memory;

namespace LiveSplit.SuperMagbot {
    public class SuperMagbotMemory : Memory {

        protected override string[] ProcessNames => new string[] { "SuperMagbot-" };

        public Pointer<byte> TransitionType { get; private set; }

        public Pointer<int> ElapsedCentiseconds { get; private set; }
        public Pointer<IntPtr> EndLevelWidget { get; private set; }
        public StringPointer LevelName { get; private set; }

        public Pointer<IntPtr> FadeOpacityPtr { get; private set; }
        public Pointer<float> FadeOpacity { get; private set; }
        public Pointer<int> LevelGridIndex { get; private set; }
        public Pointer<int> SubLevelFName { get; private set; }
        public StringPointer WorldTitle { get; private set; }
        
        private int fnameLevelMap;
        private int fnameWorldMap;

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

            const string LevelMap = "SMLevelSelectionMap";
            const string WorldMap = "SMMenuWorldSelectionMap";
            var fnames = unreal.GetFNames(LevelMap, WorldMap);
            fnameLevelMap = fnames[LevelMap];
            fnameWorldMap = fnames[WorldMap];

            IntPtr gameEngine = unreal.GetUObject("GameEngine");

            TransitionType = ptrFactory.Make<byte>(gameEngine + 0x8B8);

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
                    FadeOpacityPtr = ptrFactory.Make<IntPtr>(world, 0x30, 0xA8, 0x78, 0x230, 0x240);
                    FadeOpacity = ptrFactory.Make<float>(FadeOpacityPtr, 0xC4);

                    var subLevel = ptrFactory.Make<IntPtr>(world, 0x148, 0x8);
                    {
                        SubLevelFName = ptrFactory.Make<int>(subLevel, 0x20, 0x18);
                        
                        LevelGridIndex = ptrFactory.Make<int>(subLevel, 0xE8, 0x230, 0x270, 0x158, 0x40);
                        
                        WorldTitle = ptrFactory.MakeString(subLevel, 0xE8, 0x230, 0x2A8, 0x128, 0x8, 0x0, 0x0);
                        WorldTitle.StringType = EStringType.UTF16;
                    }
                }
            }

            logger.Log(ptrFactory.ToString());
            
            unrealTask = null;
        }

        public override bool Update() => base.Update() && unrealTask == null;

        public bool InLevelSelection() => SubLevelFName.New == fnameLevelMap;

        public bool InWorldSelection() => SubLevelFName.New == fnameWorldMap;
    }
}
