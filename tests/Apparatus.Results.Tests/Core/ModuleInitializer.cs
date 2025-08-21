using System.Runtime.CompilerServices;

namespace Apparatus.Results.Tests.Core;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyDiffPlex.Initialize();
    }
}