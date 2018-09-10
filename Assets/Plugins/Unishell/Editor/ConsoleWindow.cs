
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Mono.CSharp;
using System.IO;
using System.Text;

public class ConsoleWindow : EditorWindow {
	
	CommandEvaluator cmdEval;
	ConsoleWindowConfig cfg;
	Vector2 scrollPos;

	[MenuItem("Window/Interactive Shell")]
	static void CreateWindow() {
        ConsoleWindow window = EditorWindow.GetWindow<ConsoleWindow>();
		window.Init ();
	}

    [PreferenceItem("Unishell")]
    public static void PreferencesGUI()
    {
        var bgColor = LoadEditorPrefsColor("unishell_bg");
        var newColor = EditorGUILayout.ColorField("Background Color", bgColor);
        if (bgColor != newColor)
        {
            SaveEditorPrefsColor("unishell_bg", newColor);
        }

        var fgColor = LoadEditorPrefsColor("unishell_fg");
        newColor = EditorGUILayout.ColorField("Foreground Color", fgColor);
        if (fgColor != newColor)
        {
            SaveEditorPrefsColor("unishell_fg", newColor);
        }

    }

    static List<TextAsset> LoadScriptAssets()
    {
        var guids = EditorPrefs.HasKey("unishell_scripts")
            ? EditorPrefs.GetString("unishell_scripts").Split('|')
            : new string[] { };
        var toReturn = new List<TextAsset>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset != null)
            {
                toReturn.Add(asset);
            }
        }

        return toReturn;
    }

    static void SaveEditorPrefsColor(string prefix, Color toSave)
    {
        EditorPrefs.SetFloat(prefix + "_r", toSave.r);
        EditorPrefs.SetFloat(prefix + "_g", toSave.g);
        EditorPrefs.SetFloat(prefix + "_b", toSave.b);
        EditorPrefs.SetFloat(prefix + "_a", toSave.a);
    }

    static Color LoadEditorPrefsColor(string prefix)
    {
        var toReturn = Color.white;
        var key = string.Format("{0}_r", prefix);
        toReturn.r = EditorPrefs.HasKey(key) ? EditorPrefs.GetFloat(key) : toReturn.r;
        key = string.Format("{0}_g", prefix);
        toReturn.g = EditorPrefs.HasKey(key) ? EditorPrefs.GetFloat(key) : toReturn.g;
        key = string.Format("{0}_b", prefix);
        toReturn.b = EditorPrefs.HasKey(key) ? EditorPrefs.GetFloat(key) : toReturn.b;
        key = string.Format("{0}_a", prefix);
        toReturn.a = EditorPrefs.HasKey(key) ? EditorPrefs.GetFloat(key) : toReturn.a;

        return toReturn;
    }
	
	void Init() {
		cfg = FindConfig();
		cmdEval = new CommandEvaluator(cfg);
		titleContent = new GUIContent("Unishell Console");
		cmdEval.ClearEval();
		cmdEval.InitEval();
		cmdEval.LoadScripts();
	}
	
	ConsoleWindowConfig FindConfig()
	{
	    var config = new ConsoleWindowConfig();
	    config.foreground = Color.white;
	    config.background = Color.grey;
        config.initScripts = new List<TextAsset>();
	    config.DebugAssemblyLoading = false;

	    return config;
	}
	
	void OnGUI() {
		if(cmdEval == null) {
			Init ();
		}
		bool didComplete = false;
		bool ranCommand = false;
		if (Event.current.isKey && Event.current.type == EventType.KeyDown) {
			switch(Event.current.keyCode) {
			case KeyCode.Return:
				cmdEval.Eval();
				ranCommand = true;
				Event.current.Use();
				break;
			case KeyCode.Space:
				if(Event.current.control) {
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
		if(ranCommand) {
			scrollPos = new Vector2(0, float.MaxValue);
		}
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width (position.width), GUILayout.Height (position.height - 30));
		
		var oldFG = GUI.color;
		var oldBG = GUI.backgroundColor;
	    GUI.color = LoadEditorPrefsColor("unishell_fg");
	    GUI.backgroundColor = LoadEditorPrefsColor("unishell_bg");
		
		EditorGUILayout.TextArea(cmdEval.consoleText);
		EditorGUILayout.EndScrollView();
		cmdEval.commandText = GUILayout.TextField(cmdEval.commandText, GUILayout.Height (30));
		
		GUI.color = oldFG;
		GUI.backgroundColor = oldBG;
		
		if(didComplete) {
			TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl); 
			if (te != null) { 
				te.MoveCursorToPosition(new Vector2(5555, 5555));
			}
		}
		EditorGUILayout.EndVertical();
	}
}