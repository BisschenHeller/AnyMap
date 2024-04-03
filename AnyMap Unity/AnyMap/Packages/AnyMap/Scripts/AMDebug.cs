using System.Collections;
using UnityEngine;

namespace AnyMap
{
    /**<summary>Static class for colored debugging. Method prototypes were chosen to include multiple strings to ensure meaningful messages that help
     * with debugging</summary> */
    public static class AMDebug
    {
        public static string debugMessageColor = "#C4A987FF";

        public static void Log(string message)
        {
            Debug.Log(getStringFromInput(null, message, ""));
        }

        public static void LogError(object sender, string message, string possibleSolution)
        {
            Debug.LogError(getStringFromInput(sender, message, possibleSolution));
        }

        public static void LogWarning(object sender, string message, string possibleSolution)
        {
            Debug.LogWarning(getStringFromInput(sender, message, possibleSolution));
        }



        private static string getStringFromInput(object sender, string message, string possibleSolution)
        {
            return "<color=" + debugMessageColor + ">" + (sender == null? "" : sender.GetType().Name + ".cs: ") + message + " " + possibleSolution + "</color>";
        }
    }
}