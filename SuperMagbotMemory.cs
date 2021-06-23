using System;
using Voxif.AutoSplitter;
using Voxif.Helpers.Unreal;
using Voxif.IO;
using Voxif.Memory;

namespace LiveSplit.SuperMagbot {
    public class SuperMagbotMemory : Memory {

        protected override string[] ProcessNames => new string[] { "SuperMagbot-" };

        public Pointer<byte> TransitionType{ get; private set; }
        public Pointer<int> ElapsedCentiseconds { get; private set; }
        public Pointer<IntPtr> EndLevelWidget { get; private set; }
        public StringPointer LevelName { get; private set; }

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
            
            IntPtr gameEngine = unreal.GetUObject("GameEngine");

            TransitionType = ptrFactory.Make<byte>(gameEngine + 0x8B8);

            var playerController = ptrFactory.Make<IntPtr>(gameEngine + 0xDE8, 0x38, 0x0, 0x30);

            ElapsedCentiseconds = ptrFactory.Make<int>(playerController, 0x578, 0x338);

            EndLevelWidget = ptrFactory.Make<IntPtr>(playerController, 0x580);

            LevelName = ptrFactory.MakeString(playerController, 0x588, 0x260, 0x128, 0x48, 7*2);
            LevelName.StringType = EStringType.UTF16;

            logger.Log(ptrFactory.ToString());
            
            unrealTask = null;
        }

        public override bool Update() => base.Update() && unrealTask == null;
    }
}
