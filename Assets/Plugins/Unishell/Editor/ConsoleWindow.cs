
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Mono.CSharp;
using System.IO;
using System.Text;

public class ConsoleWindow : EditorWindow {
	
	const string CONFIG_PATH = "Assets/Plugins/Unishell/config.asset";
	CommandEvaluator cmdEval;
	ConsoleWindowConfig cfg;
	Vector2 scrollPos;

	[MenuItem("Window/Interactive Shell")]
	static void CreateWindow() {
        ConsoleWindow window = EditorWindow.GetWindow<ConsoleWindow>();
		window.Init ();
	}
	
	void Init() {
		cfg = FindConfig();
		cmdEval = new CommandEvaluator(cfg);
		titleContent = new GUIContent("Unishell Console");
		cmdEval.ClearEval();
		cmdEval.InitEval();
		cmdEval.LoadScripts();
	}
	
	ConsoleWindowConfig FindConfig() {
        var configs = AssetDatabase.FindAssets("t:ConsoleWindowConfig");

		if(configs == null || configs.Length == 0) {
			Debug.Log ("Creating config");
			var cfg = ScriptableObject.CreateInstance<ConsoleWindowConfig>();
			cfg.foreground = Color.white;
			cfg.background = Color.white;
			AssetDatabase.CreateAsset(cfg, CONFIG_PATH);
			return cfg;
		}
        var path = AssetDatabase.GUIDToAssetPath(configs[0]);
        var asset = AssetDatabase.LoadAssetAtPath<ConsoleWindowConfig>(path);
		return asset;
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
		GUI.color = cfg.foreground;
		GUI.backgroundColor = cfg.background;
		
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