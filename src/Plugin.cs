using BepInEx;
using BepInEx.Logging;

using Motorways.Models;
using Motorways;

using UnityEngine;
using HarmonyLib;

namespace NetworkFlowSolver
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony _harmony;
        public static ManualLogSource s_Logger;

        private void Awake()
        {
            s_Logger = Logger;
            s_Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            _harmony = Harmony.CreateAndPatchAll(typeof(Plugin));
        }


        private void OnDestroy()
        {
            UIHandler.OnDestroy();
            _harmony?.UnpatchSelf();
        }

        [HarmonyPatch(typeof(MotorwaysGame), nameof(MotorwaysGame.Tick))]
        [HarmonyPostfix]
        static void GameTickPostfix(MotorwaysGame __instance)
        {
            UIHandler.UpdateUI(__instance);
            if (__instance.StartedWithGameMode != GameMode.Normal) return;
            Server.ISimulation simulation = __instance.Simulation;
            // create groups
            foreach (DestinationModel destination in simulation.GetModels<DestinationModel>())
            {
                s_Logger.LogDebug(destination.TileModels);
            }
        }
    }

    public class UIHandler
    {
        private static GameObject s_uiObject;
        private static UIScript s_uiScript;

        public static void UpdateUI(MotorwaysGame game)
        {
            if (!s_uiScript)
            {
                s_uiObject = new GameObject();
                s_uiScript = s_uiObject.AddComponent<UIScript>();
                GameObject.DontDestroyOnLoad(s_uiObject);
            }
            Server.ISimulation simulation = game.Simulation;
            s_uiScript.destinations = simulation.GetModels<DestinationModel>().Count;
            s_uiScript.houses = simulation.GetModels<HouseModel>().Count;
            s_uiScript.vehicles = simulation.GetModels<VehicleModel>().Count;
            s_uiScript.gameMode = game.StartedWithGameMode.ToString();
        }

        public static void OnDestroy()
        {
            if (s_uiObject)
                GameObject.Destroy(s_uiObject);
        }
    }

    public class UIScript : MonoBehaviour
    {
        public int destinations = 0;
        public int houses = 0;
        public int vehicles = 0;
        public string gameMode = "";

        public void OnGUI()
        {
            // Constrain all drawing to be within a 800x600 pixel area centered on the screen.
            GUI.BeginGroup(new Rect(10, 10, 170, 200));

            // Draw a box in the new coordinate space defined by the BeginGroup.
            // Notice how (0,0) has now been moved on-screen
            GUI.Box(new Rect(0, 0, 170, 20), "<color=green>Traffic Router Bot</color>");
            GUI.Box(new Rect(0, 20, 170, 20), "pepsipu");
            GUI.Box(new Rect(0, 40, 170, 20), $"Houses: {houses}");
            GUI.Box(new Rect(0, 60, 170, 20), $"Destinations: {destinations}");
            GUI.Box(new Rect(0, 80, 170, 20), $"Vehicles: {vehicles}");
            GUI.Box(new Rect(0, 100, 170, 20), $"Game Mode: {gameMode}");

            // We need to match all BeginGroup calls with an EndGroup
            GUI.EndGroup();
        }
    }
}
