using UnityEngine;


namespace SigmaHeatShifterPlugin
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Version : MonoBehaviour
    {
        public static readonly System.Version number = new System.Version("0.1.0");
        static bool first = true;

        void Awake()
        {
            if (first)
            {
                first = false;
                Debug.Log("[SigmaLog] Version Check:   Sigma HeatShifter v" + number);
            }
        }
    }
}
