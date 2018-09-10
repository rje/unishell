
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ConsoleWindow : EditorWindow
{
    private const string PROPERTY_SCRIPT_DIR = "unishell_script_dir";
    private const string PROPERTY_COLOR_FG = "unishell_fg";
    private const string PROPERTY_COLOR_BG = "unishell_bg";
    CommandEvaluator cmdEval;
    Vector2 scrollPos;

    [MenuItem("Window/Unishell")]
    static void CreateWindow()
    {
        ConsoleWindow window = EditorWindow.GetWindow<ConsoleWindow>();
        window.Init();
    }

    [PreferenceItem("Unishell")]
    public static void PreferencesGUI()
    {
        var bgColor = LoadEditorPrefsColor(PROPERTY_COLOR_BG);
        var newColor = EditorGUILayout.ColorField("Background Color", bgColor);
        if (bgColor != newColor)
        {
            SaveEditorPrefsColor(PROPERTY_COLOR_BG, newColor);
        }

        var fgColor = LoadEditorPrefsColor(PROPERTY_COLOR_FG);
        newColor = EditorGUILayout.ColorField("Foreground Color", fgColor);
        if (fgColor != newColor)
        {
            SaveEditorPrefsColor(PROPERTY_COLOR_FG, newColor);
        }

        var scriptFolder = EditorPrefs.GetString(PROPERTY_SCRIPT_DIR, "/Resources/Unishell");
        GUI.enabled = false;
        EditorGUILayout.TextField("Script Directory", scriptFolder);
        GUI.enabled = true;
        if (GUILayout.Button("Change Script Directory"))
        {
            var newPath = EditorUtility.OpenFolderPanel("Select script folder", "", "");
            if (newPath.StartsWith(Application.dataPath))
            {
                newPath = newPath.Substring(Application.dataPath.Length);
                if (scriptFolder != newPath)
                {
                    scriptFolder = newPath;
                    EditorPrefs.SetString(PROPERTY_SCRIPT_DIR, scriptFolder);
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid script folder", "Please select a folder that is somewhere in your project folder structure", "Ok");
            }

        }

    }

    public static List<string> GetScriptContents()
    {
        var toReturn = new List<string>();
        var scriptFolder = EditorPrefs.GetString(PROPERTY_SCRIPT_DIR, "/Resources/Unishell");
        var fullFolder = Application.dataPath + scriptFolder;
        if(!Directory.Exists(fullFolder))
        {
            return toReturn;
        }
        var files = Directory.EnumerateFiles(fullFolder, "*.csx", SearchOption.AllDirectories);
        foreach(var file in files)
        {
            toReturn.Add(File.ReadAllText(file));
        }

        return toReturn;
    }

    static void SaveEditorPrefsColor(string prefix, Color toSave)
    {
        EditorPrefs.SetFloat($"{prefix}_r", toSave.r);
        EditorPrefs.SetFloat($"{prefix}_g", toSave.g);
        EditorPrefs.SetFloat($"{prefix}_b", toSave.b);
        EditorPrefs.SetFloat($"{prefix}_a", toSave.a);
    }

    static Color LoadEditorPrefsColor(string prefix)
    {
        var toReturn = Color.white;
        toReturn.r = EditorPrefs.GetFloat($"{prefix}_r", toReturn.r);
        toReturn.g = EditorPrefs.GetFloat($"{prefix}_g", toReturn.g);
        toReturn.b = EditorPrefs.GetFloat($"{prefix}_b", toReturn.b);
        toReturn.a = EditorPrefs.GetFloat($"{prefix}_a", toReturn.a);

        return toReturn;
    }

    void Init()
    {
        cmdEval = new CommandEvaluator();
        titleContent = new GUIContent("Unishell");
        cmdEval.ClearEval();
        cmdEval.InitEval();
        cmdEval.LoadScripts();
    }

    void OnGUI()
    {
        if (cmdEval == null)
        {
            Init();
        }

        var oldFG = GUI.color;
        var oldBG = GUI.backgroundColor;
        GUI.color = LoadEditorPrefsColor(PROPERTY_COLOR_FG);
        GUI.backgroundColor = LoadEditorPrefsColor(PROPERTY_COLOR_BG);

        bool didComplete = false;
        bool ranCommand = false;
        if (Event.current.isKey && Event.current.type == EventType.KeyDown)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Return:
                    cmdEval.Eval();
                    ranCommand = true;
                    Event.current.Use();
                    break;
                case KeyCode.Space:
                    if (Event.current.control)
                    {
                        didComplete = cmdEval.AutocompleteBuffer();
                    }
                    break;
                case KeyCode.UpArrow:
                    didComplete = cmdEval.UpHistory();
                    break;
                case KeyCode.DownArrow:
                    didComplete = cmdEval.DownHistory();
                    break;
            }
        }

        EditorGUILayout.BeginVertical();
        if (ranCommand)
        {
            scrollPos = new Vector2(0, float.MaxValue);
        }
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 30));


        EditorGUILayout.TextArea(cmdEval.consoleText);
        EditorGUILayout.EndScrollView();
        cmdEval.commandText = GUILayout.TextField(cmdEval.commandText, GUILayout.Height(30));

        GUI.color = oldFG;
        GUI.backgroundColor = oldBG;

        if (didComplete)
        {
            TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            if (te != null)
            {
                te.MoveCursorToPosition(new Vector2(5555, 5555));
            }
        }
        EditorGUILayout.EndVertical();
    }
}