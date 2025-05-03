using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;

namespace ModMenuSpace
{
    public class ModMenu : MonoBehaviour
    {
        // TODO: Hook WaterBiomeManager.OnGUI()
        public static void ModMenuManager()
        {
            DrawMenu();
            CursorToggle(KeyCode.F10);
            RunSilently(() => CheatInfiniteOxygen());
            RunSilently(() => CheatInfiniteTemperature());
            RunSilently(() => CheatMovementSpeed());
        }
        public static void DrawMenu()
        {
            if (GUI.Button(new Rect(0f, 0f, 100f, 30f), menuLabel))
            {
                menuVisible = !menuVisible;
            }
            if (menuVisible)
            {
                GUI.Box(new Rect(100f, 0f, 500f, 400f), menuLabel);
                if (GUI.Button(new Rect(100f, 0f, 180f, 30f), "DumpState(Player)")) ;
                {
                    RunSilently(() => DumpState(Player.main.gameObject));
                }
                int n = 1;
                cheatActiveInfiniteOxygen = (GUI.Button(new Rect(100f, 30f * n++, 180f, 30f), "InfiniteOxygen [" + cheatActiveInfiniteOxygen.ToString() + "]") ? (!cheatActiveInfiniteOxygen) : cheatActiveInfiniteOxygen);
                cheatActiveInfiniteTemperature = (GUI.Button(new Rect(100f, 30f * n++, 180f, 30f), "InfiniteTemperature [" + cheatActiveInfiniteTemperature.ToString() + "]") ? (!cheatActiveInfiniteTemperature) : cheatActiveInfiniteTemperature);
                cheatActiveMovementSpeed = (GUI.Button(new Rect(100f, 30f * n++, 180f, 30f), "MovementSpeed [" + cheatActiveMovementSpeed.ToString() + "]") ? (!cheatActiveMovementSpeed) : cheatActiveMovementSpeed);
            }
        }
        public static void CheatInfiniteOxygen()
        {
            if (cheatActiveInfiniteOxygen)
            {
                Player.main.oxygenMgr.AddOxygen(1f);
            }
        }
        public static void CheatInfiniteTemperature()
        {
            if (cheatActiveInfiniteTemperature)
            {
                Player.main.GetComponent<BodyTemperature>().currentBodyHeatValue = 99f;
            }
        }
        public static void CheatMovementSpeed()
        {
            if (cheatActiveMovementSpeed)
            {
                /*int numBox = 0;
                DebugFloatBox(ref Player.main.GetComponent<PlayerController>().walkRunForwardMaxSpeed, ref numBox);
                DebugFloatBox(ref Player.main.GetComponent<PlayerController>().walkRunBackwardMaxSpeed, ref numBox);
                DebugFloatBox(ref Player.main.GetComponent<PlayerController>().walkRunStrafeMaxSpeed, ref numBox);
                DebugFloatBox(ref Player.main.GetComponent<GroundMotor>().forwardMaxSpeed, ref numBox);
                DebugFloatBox(ref Player.main.GetComponent<PlayerController>().swimVerticalMaxSpeed, ref numBox);
                DebugFloatBox(ref Player.main.GetComponent<PlayerController>().swimForwardMaxSpeed, ref numBox);
                DebugFloatBox(ref Player.main.GetComponent<PlayerController>().swimBackwardMaxSpeed, ref numBox);
                DebugFloatBox(ref Player.main.GetComponent<PlayerController>().swimStrafeMaxSpeed, ref numBox);
                DebugFloatBox(ref Player.main.GetComponent<UnderwaterMotor>().forwardMaxSpeed, ref numBox);*/
                Player.main.GetComponent<GroundMotor>().forwardMaxSpeed = 99f;
            }
            else
            {
                Player.main.GetComponent<GroundMotor>().forwardMaxSpeed = 4.4f;
            }
        }
        public static void DebugFloatBox(ref float f, ref int numBox)
        {
            f = Convert.ToSingle(GUI.TextField(new Rect(1000f, 30f * numBox++, 300f, 30f), Convert.ToString(f)));
        }
        public static void RunSilently(Action action)
        {
            try { action(); } catch { }
        }

        public static void DumpState(GameObject go)
        {
            GameObjectToJsonDump(go);
            // Sanity check that the file isn't larger than 10Mo
            if (dumpJson.Length < 10 * 1024 * 1024)
            {
                File.WriteAllText($"{dumpDir}/{go.name}.json", dumpJson);
            }
        }

        public static void GameObjectToJsonDump(GameObject go)
        {
            string Safe(Func<object> g) { try { return (g()?.ToString() ?? "").Replace("\"", "\\\""); } catch { return "err"; } }
            void Trim(StringBuilder _sb) { if (_sb.Length > 0 && _sb[_sb.Length - 1] == ',') _sb.Length--; }
            var sb = new StringBuilder();

            // GameObject
            sb.Append("{\"name\":\"").Append(go.name).Append("\",\"components\":[");
            var comps = go.GetComponents<Component>();

            for (int i = 0; i < comps.Length; i++)
            {
                var c = comps[i];
                var t = c.GetType();

                // Fields
                sb.Append("{\"type\":\"").Append(t.Name).Append("\",\"fields\":{");
                foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var val = Safe(() => f.GetValue(c)).Replace("\n", " ").Replace("\r", " ").Replace("\t", " ").Trim();
                    sb.Append("\"").Append(f.Name).Append("\":\"").Append(val).Append("\",");
                }

                // Properties
                Trim(sb); sb.Append("},\"properties\":{");
                var seen = new HashSet<string>();
                foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    if (p.CanRead && p.GetIndexParameters().Length == 0 && seen.Add(p.Name))
                        sb.Append("\"").Append(p.Name).Append("\":\"").Append(Safe(() => p.GetValue(c))).Append("\",");

                // Methods (use full signatures as keys to prevent duplication)
                Trim(sb); sb.Append("},\"methods\":{");
                foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    string sig = m.ToString().Replace("\n", " ").Replace("\r", " ").Replace("\t", " ").Trim();
                    sb.Append("\"").Append(Safe(() => sig)).Append("\":\"method\",");
                }

                Trim(sb); sb.Append("}}");
                if (i < comps.Length - 1) sb.Append(",");
            }

            sb.Append("]}");
            dumpJson = Regex.Replace(sb.ToString(), "[\r\n\t]", "");
        }

        public static void CursorToggle(KeyCode k)
        {
            if (!cursorFixToggle && Input.GetKey(k))
            {
                cursorLockStateBackup = Cursor.lockState;
                cursorVisibleBackup = Cursor.visible;
                cursorFixToggle = true;
            }
            else if (cursorFixToggle && Input.GetKey(k))
            {
                Cursor.lockState = cursorLockStateBackup;
                Cursor.visible = cursorVisibleBackup;
                cursorFixToggle = false;
            }

            if (cursorFixToggle)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = cursorLockStateBackup;
                Cursor.visible = cursorVisibleBackup;
            }
        }

        public static string menuLabel = "MOD MENU";
        public static bool menuVisible = false;
        public static bool cursorFixToggle = false;
        public static CursorLockMode cursorLockStateBackup;
        public static bool cursorVisibleBackup;
        public static bool cheatActiveInfiniteOxygen = false;
        public static bool cheatActiveInfiniteTemperature = false;
        public static bool cheatActiveMovementSpeed = false;
        public static string dumpJson;
        public static string dumpDir = "D:/Modding/";
    }
}