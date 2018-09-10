# unishell: Interactive C# Shell for the Unity3d editor

## Installation
Copy Assets/Plugins/Unishell into your project

### Binaries
Binary unitypackage files can be downloaded here: https://github.com/rje/unishell/downloads

## Use

### Basics
Select Window -> Unishell to open the window.

Type "help" to see a list of available commands, otherwise type any valid C# statement or expression.  Some examples of things you can do:

    1 + 2;

    var go = GameObject.FindGameObjectsWithTag("MainCamera");
    go.transform.position = Vector3.zero;

    for(int i = 0; i < 10; i++) { Debug.Log("hello from the shell!"); }


### Scripts
You can add custom scripts that define helper functions by creating a script directory (default: Resources/Unishell) with the suffix .csx.
The scripts will all be evaluated by the shell on first load or when you type "reload" in the shell.  This is handy for creating small functions that you plan on using often. For example, you could create a file called "scripts.csx" with the following contents:

    Func<int, int, int> add = (a, b) => {
	    return a + b;
    };

Then in the config object add that text asset to the Init Scripts list. Type reload in the shell and then you should be able to:

    > add(3, 4);
    7

### Useful types to know
#### Action\<T\>, Action\<T, T\>, etc
These are types for any function you want to create in a script that does not have a return value. e.g.

    Action<string, int> lotsOfDebugStatements = (text, count) => {
	    for(int i = 0; i < count; i++) {
		    Debug.Log(text);
	    }
    };

#### Func\<T, TResult\>, Func\<T, T, TResult\>, etc
These are types for any function you want to create that does have a return value. e.g.

    Func<int, int, int> add = (a, b) => {
	    return a + b;
    };

Note that the first type parameters are the function argument types and the last type parameter is the return value type.

## Shell Commands

### reload
Reload all scripts specified in the unishell config. This will currently overwrite any previous version of a script function and add any new functions, but will not remove functions that have been loaded into the context but are no longer in the script.

### list vars
Lists all variables created within the shell content

### list using
Lists all assemblies currently referenced by a 'using' statement

### help
Print out all available shell commands

### clear
Clear the output text area

## Notes & Bugs
* Don't forget to end your statements and expressions with a semicolon
* Playing and stopping the game can reset the shell variables
* The basic editor mechanics still apply -- e.g. changes made in play mode are not persisted after you stop gameplay, but changes made in editing mode are permanent
* Bug: Autocomplete doesn't work reliably yet
* Bug: Autocomplete doesn't let you cycle through available matches
