
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Mono.CSharp;
using System.IO;
using System.Text;

public class ConsoleWindow : EditorWindow {
	
	const string CONFIG_PATH = "Assets/Add-Ons/Unishell/config.asset";
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
		title = "Shell";
		cmdEval.ClearEval();
		cmdEval.InitEval();
		cmdEval.LoadScripts();
	}
	
	ConsoleWindowConfig FindConfig() {
		var results = AssetDatabase.LoadAssetAtPath(CONFIG_PATH, typeof(ConsoleWindowConfig)) as ConsoleWindowConfig;
		if(results == null) {
			Debug.Log ("Creating config");
			var cfg = ScriptableObject.CreateInstance<ConsoleWindowConfig>();
			cfg.foreground = Color.white;
			cfg.background = Color.white;
			AssetDatabase.CreateAsset(cfg, CONFIG_PATH);
			return cfg;
		}
		return results;
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