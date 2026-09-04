using BCellResearchApp.Models;
using System.Runtime.InteropServices;

namespace BCellResearchApp.Services;

public static class NativeScorer
{
    [DllImport("bcell_native", CallingConvention = CallingConvention.Cdecl, EntryPoint = "score_clone")]
    private static extern double ScoreCloneNative(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string antigen,
        int memoryCell,
        double confidence);

    public static double Score(BCellClone clone, out bool nativeUsed)
    {
        try
        {
            nativeUsed = true;
            return Math.Clamp(ScoreCloneNative(clone.Antigen, clone.MemoryCell ? 1 : 0, clone.Confidence), 0, 1);
        }
        catch (DllNotFoundException)
        {
            nativeUsed = false;
            return ManagedFallback(clone);
        }
        catch (EntryPointNotFoundException)
        {
            nativeUsed = false;
            return ManagedFallback(clone);
        }
    }

    private static double ManagedFallback(BCellClone clone)
    {
        var antigen = clone.Antigen.ToLowerInvariant();
        double score = clone.MemoryCell ? 0.20 : 0;
        if (antigen.Contains("sars-cov-2") || antigen.Contains("covid")) score += 0.55;
        if (antigen.Contains("spike") || antigen.Contains("rbd") || antigen.Contains("nucleocapsid")) score += 0.15;
        score += 0.10 * Math.Clamp(clone.Confidence, 0, 1);
        return Math.Clamp(score, 0, 1);
    }
}
