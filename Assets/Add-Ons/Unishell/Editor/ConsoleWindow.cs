
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Mono.CSharp;
using System.IO;
using System.Text;

public class ConsoleWindow : EditorWindow {
	public string consoleText = "";
	public string commandText = "";
	public string partialCommand = null;
	public List<String> previousCommands = new List<String>();
	int commandScrollIdx = -1;
	const int MAX_CMD_BUFFER = 100;
	
	Vector2 scrollPos;
	StringBuilder errInfo = new StringBuilder("");

	[MenuItem("Window/Interactive Shell")]
	static void Init() {
        ConsoleWindow window = EditorWindow.GetWindow<ConsoleWindow>();
		window.title = "Shell";
		window.InitEval();
	}
	
	void InitEval() {
		foreach(System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
			Mono.CSharp.Evaluator.ReferenceAssembly(assembly);
		}
		Evaluator.Run ("using UnityEngine; using UnityEditor; using System.Collections.Generic;");
		Evaluator.MessageOutput = new StringWriter(errInfo);
	}
	
	void OnGUI() {
		bool didComplete = false;
		if (Event.current.isKey && Event.current.type == EventType.KeyDown) {
			switch(Event.current.keyCode) {
			case KeyCode.Return:
				RunCommand(commandText);
				commandText = "";
				commandScrollIdx = -1;
				Event.current.Use();
				break;
			case KeyCode.Space:
				if(Event.current.control) {
					commandText = TryTabComplete(commandText);
					didComplete = true;
				}
				break;
			case KeyCode.UpArrow:
				if(previousCommands.Count > 0) {
					if(commandScrollIdx == -1) {
						commandScrollIdx = previousCommands.Count - 1;
						didComplete = true;
					}
					else {
						commandScrollIdx--;
						if(commandScrollIdx < 0) {
							commandScrollIdx = 0;
						}
					}
					commandText = previousCommands[commandScrollIdx];
					didComplete = true;
				}
				break;
			case KeyCode.DownArrow:
				if(commandScrollIdx != -1) {
					commandScrollIdx++;
					if(commandScrollIdx < previousCommands.Count) {
						commandText = previousCommands[commandScrollIdx];
						didComplete = true;
					}
					else {
						commandText = "";
						commandScrollIdx = -1;
						didComplete = true;
					}
				}
				break;
			}
		}
		EditorGUILayout.BeginVertical();
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width (position.width), GUILayout.Height (position.height - 30));
		EditorGUILayout.TextArea(consoleText);
		EditorGUILayout.EndScrollView();
		commandText = GUILayout.TextField(commandText, GUILayout.Height (30));
		if(didComplete) {
			TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl); 
			if (te != null) { 
				te.MoveCursorToPosition(new Vector2(5555, 5555));
			}
		}
		EditorGUILayout.EndVertical();
	}
	
	string TryTabComplete(string cmdText) {
		string prefix;
		var results = Evaluator.GetCompletions(cmdText, out prefix);
		if(results != null && results.Length > 1) {
			cmdText = cmdText + results[0];
		}
		return cmdText;
	}
	
	void ListVars() {
		string vars = null;
		try {
			vars = Evaluator.GetVars();
		}
		catch(Exception e) {
			e.ToString();
		}
		if(vars != null && vars.Length > 0) {
			consoleText += "Current vars: \n";
			consoleText += vars;
			consoleText += "\n";
		}
		else {
			consoleText += "No current vars\n";
		}
	}
	
	void Clear() {
		consoleText = "";
	}
	
	void ListUsing() {
		var val = Evaluator.GetUsing();
		if(val.Length > 0) {
			consoleText += String.Format ("Current using: \n{0}\n", val);
		}
		else {
			consoleText += "No current using\n";
		}
	}
	
	void Help() {
		consoleText += "Current shell commands:\n";
		consoleText += "\tlist vars: List all local variables in the shell\n";
		consoleText += "\tlist using: List all using statements in the shell\n";
		consoleText += "\tclear: Clear the contents of the shell window (does not clear the variables)\n";
		consoleText += "\thelp: Prints this message\n";
		consoleText += "\n";
		consoleText += "Current shortcuts:\n";
		consoleText += "ctrl-space: autocomplete current buffer";
		consoleText += "up arrow/down arrow: go through previous commands";
		consoleText += "\n";
	}
	
	bool CheckLocal(string toCheck) {
		switch(toCheck) {
		case "list vars":
			ListVars();
			return true;
		case "list using":
			ListUsing();
			return true;
		case "clear":
			Clear();
			return true;
		case "help":
			Help();
			return true;
		}
		return false;
	}
	
	void RunCommand(string commandText) {
		if(commandText == null || commandText.Trim ().Equals ("")) {
			return;
		}
		InitEval ();
		if(!CheckLocal (commandText)) {
			object obj;
			bool result_set;
			var command = commandText;
			bool dots = false;
			if(partialCommand != null) {
				command = partialCommand + " " + commandText;
				dots = true;
			}
			partialCommand = null;
			string retval = Evaluator.Evaluate(command, out obj, out result_set);
			consoleText += string.Format ("{0} {1}\n", dots ? "... " : "> ", commandText);
			if(retval == null) {
				if(result_set) {
					consoleText += string.Format ("{0}\n", obj);
				}
				string toStore = command.Replace ("\n", " ");
				AddCommandToBuffer(toStore);
			}
			else {
				partialCommand = retval;
			}
			Evaluator.MessageOutput.Flush();
			if(errInfo.Length > 0) {
				consoleText += string.Format ("{0}\n", errInfo.ToString());
				errInfo.Remove(0, errInfo.Length);
			}
		}
		else {
			AddCommandToBuffer(commandText);
		}
	}
	
	void AddCommandToBuffer(string toAdd) {
		previousCommands.Add(toAdd);
		if(previousCommands.Count > MAX_CMD_BUFFER) {
			previousCommands.RemoveAt (0);
		}
	}
}