using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Mono.CSharp;
using System.IO;
using System.Text;

public class BuiltinCommands {
	
	public CommandEvaluator cmdEval;
	
	public BuiltinCommands(CommandEvaluator parent) {
		cmdEval = parent;
	}
	
	void ListVars() {
		string vars = null;
		try {
			vars = cmdEval.CsharpEval.GetVars();
		}
		catch(Exception e) {
			e.ToString();
		}
		if(vars != null && vars.Length > 0) {
			cmdEval.AppendOutput("Current vars:");
			cmdEval.AppendOutput(vars);
			cmdEval.AppendOutput("");
		}
		else {
			cmdEval.AppendOutput("No current vars");
		}
	}
	
	void Clear() {
		cmdEval.ClearConsole();
	}
	
	void ListUsing() {
		var val = cmdEval.CsharpEval.GetUsing();
		if(val.Length > 0) {
			cmdEval.AppendOutput("Current using:");
			cmdEval.AppendOutput(val);
			cmdEval.AppendOutput("");
		}
		else {
			cmdEval.AppendOutput("No current using");
			cmdEval.AppendOutput("");
		}
	}
	
	void ReloadScripts() {
		cmdEval.LoadScripts();
	}
	
	void Help() {
		cmdEval.AppendOutput("Current shell commands:");
		cmdEval.AppendOutput("\tlist vars: List all local variables in the shell");
		cmdEval.AppendOutput("\tlist using: List all using statements in the shell");
		cmdEval.AppendOutput("\treload: Reload all scripts");
		cmdEval.AppendOutput("\tclear: Clear the contents of the shell window (does not clear the variables)");
		cmdEval.AppendOutput("\thelp: Prints this message");
		cmdEval.AppendOutput("");
		cmdEval.AppendOutput("Current shortcuts:");
		cmdEval.AppendOutput("\tctrl-space: autocomplete current buffer");
		cmdEval.AppendOutput("\tup arrow/down arrow: go through previous commands");
		cmdEval.AppendOutput("");
	}
	
	public bool CheckBuiltins(string toCheck) {
		switch(toCheck) {
		case "list vars":
		case "list vars;":
			ListVars();
			return true;
		case "list using":
		case "list using;":
			ListUsing();
			return true;
		case "clear":
		case "clear;":
			Clear();
			return true;
		case "help":
		case "help;":
			Help();
			return true;
		case "reload":
		case "reload;":
			ReloadScripts();
			return true;
		}
		return false;
	}
}

