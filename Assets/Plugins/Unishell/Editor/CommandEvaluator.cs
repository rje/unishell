using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Mono.CSharp;
using System.IO;
using System.Text;

public class CommandEvaluator {
	public ConsoleWindowConfig config;
	public string partialCommand = null;
	public List<String> previousCommands = new List<String>();
	int commandScrollIdx = -1;
	const int MAX_CMD_BUFFER = 100;
	public string consoleText = "";
	public string commandText = "";
	StringBuilder errInfo = new StringBuilder("");
	public BuiltinCommands builtins;
	
	public CommandEvaluator(ConsoleWindowConfig cfg) {
		config = cfg;
		builtins = new BuiltinCommands(this);
		Evaluator.MessageOutput = new StringWriter(errInfo);
	}
	
	public void ClearEval() {
		Evaluator.Init (new string[] {});
	}

#pragma warning disable 1635
#pragma warning disable 0168
    public void InitEval() {
		foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if(assembly == null)
            {
                continue;
            }
            try
            {
                Evaluator.ReferenceAssembly(assembly);
            }
            catch(Exception e)
            {
            }
        }
		Evaluator.Run ("using UnityEngine; using System; using System.Collections.Generic;");
	}
#pragma warning restore 1635
#pragma warning restore 0168

    public void LoadScripts() {
		List<string> scripts = config.GetScriptContents();
		foreach(var toParse in scripts) {
			Evaluator.Run(toParse);
		}
	}
	
	public bool AutocompleteBuffer() {
		commandText = TryTabComplete(commandText);
		return true;
	}
	
	public bool UpHistory() {
		if(previousCommands.Count > 0) {
			if(commandScrollIdx == -1) {
				commandScrollIdx = previousCommands.Count - 1;
				return true;
			}
			else {
				commandScrollIdx--;
				if(commandScrollIdx < 0) {
					commandScrollIdx = 0;
				}
			}
			commandText = previousCommands[commandScrollIdx];
			return true;
		}
		return false;
	}
	
	public bool DownHistory() {		
		if(commandScrollIdx != -1) {
			commandScrollIdx++;
			if(commandScrollIdx < previousCommands.Count) {
				commandText = previousCommands[commandScrollIdx];
				return true;
			}
			else {
				commandText = "";
				commandScrollIdx = -1;
				return true;
			}
		}
		return false;
	}
	
	public void Eval() {
		RunCommand(commandText);
		commandText = "";
		commandScrollIdx = -1;
	}
	
	string TryTabComplete(string cmdText) {
		string prefix;
		var results = Evaluator.GetCompletions(cmdText, out prefix);
		if(results != null && results.Length > 1) {
			cmdText = cmdText + results[0];
		}
		return cmdText;
	}
	
	void RunCommand(string commandText) {
		if(commandText == null || commandText.Trim ().Equals ("")) {
			return;
		}
		InitEval ();
		if(!builtins.CheckBuiltins(commandText)) {
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
			AppendOutput(string.Format ("{0} {1}", dots ? "... " : "> ", commandText));
			if(retval == null) {
				if(result_set) {
					AppendOutput(obj);
				}
				string toStore = command.Replace ("\n", " ");
				AddCommandToBuffer(toStore);
			}
			else {
				partialCommand = retval;
			}
			Evaluator.MessageOutput.Flush();
			if(errInfo.Length > 0) {
				AppendOutput(errInfo.ToString());
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
	
	public void AppendOutput(object toWrite) {
		consoleText += toWrite.ToString() + "\n";
	}
	
	public void ClearConsole() {
		consoleText = "";
	}
}

