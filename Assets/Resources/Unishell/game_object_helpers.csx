
Func<string, float, float, float, GameObject> new_go = (string name, float x, float y, float z) => {
    var go = new GameObject(name);
    go.transform.position = new Vector3(x, y, z);
    return go;
}