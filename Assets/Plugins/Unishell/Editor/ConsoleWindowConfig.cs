using UnityEngine;
using System.Collections.Generic;

public class ConsoleWindowConfig : ScriptableObject {
	
	public List<TextAsset> initScripts;
	public Color foreground;
	public Color background;
    [Space]
    [Header("Debugging Tools")]
    public bool DebugAssemblyLoading;

    public List<string> GetScriptContents() {
		var toReturn = new List<string>();
		foreach(var asset in initScripts) {
			toReturn.Add (asset.text);
		}
		
		return toReturn;
	}
}
