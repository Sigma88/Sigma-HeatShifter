using UnityEngine;
using System.Reflection;


namespace SigmaHeatShifterPlugin
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    internal class MethodReplacer : MonoBehaviour
    {
        void Start()
        {
            Replace<AeroGUI, AeroGUIPlus>("GetThermalStats", "GetThermalStatsPlus");
            Replace<CelestialBody, CelestialBodyPlus>("GetAtmoThermalStats", "GetAtmoThermalStats");
        }

        void Replace<TSource, TDestination>(string source, string destination)
        {
            MethodInfo methodToReplace = typeof(TSource).GetMethod(source, BindingFlags.Instance | BindingFlags.Public);
            MethodInfo methodToInject = typeof(TDestination).GetMethod(destination, BindingFlags.Instance | BindingFlags.Public);
            bool result = Detourer.TryDetourFromTo(methodToReplace, methodToInject);
        }
    }
}
