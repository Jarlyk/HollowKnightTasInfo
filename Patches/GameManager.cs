// ReSharper disable All

using System;
using System.Collections;
using System.Collections.Generic;
using Assembly_CSharp.TasInfo.mm.Source;
using Mono.Cecil;
using MonoMod;

#pragma warning disable CS0649, CS0414

class patch_GameManager : GameManager {
    private static readonly long TasInfoMark = 1234567890123456789;
    public static string TasInfo;

    //[MonoModIgnore]
    //public extern void orig_LeftScene(bool doAdditiveLoad);

    //[NoInlining]
    //public new void LeftScene(bool doAdditiveLoad) {
    //    //RandomInjection.OnLeftScene();
    //    orig_LeftScene(doAdditiveLoad);
    //}

#if V1028 || V1028_KRYTHOM || V1037
    [MonoModIgnore]
    private extern void orig_ManualLevelStart();
    private void ManualLevelStart() {
        orig_ManualLevelStart();
        Assembly_CSharp.TasInfo.mm.Source.TasInfo.AfterManualLevelStart();
    }
#endif
}

[MonoModCustomMethodAttribute("NoInlining")]
public class NoInlining : Attribute {
}

namespace MonoMod {
    static partial class MonoModRules {
        // ReSharper disable once UnusedParameter.Global
        public static void NoInlining(MethodDefinition method, CustomAttribute attrib) => method.NoInlining = true;
    }
}