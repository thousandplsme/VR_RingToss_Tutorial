var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/RingTossMaterials/RingShape.prefab");
if (prefab == null) throw new System.Exception("RingShape missing");
return new {
  scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,
  dirty = UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty,
  dependencies = UnityEditor.AssetDatabase.GetDependencies("Assets/RingTossMaterials/RingShape.prefab", true),
  mesh = UnityEditor.AssetDatabase.GetAssetPath(prefab.GetComponent<UnityEngine.MeshFilter>().sharedMesh),
  material = UnityEditor.AssetDatabase.GetAssetPath(prefab.GetComponent<UnityEngine.MeshRenderer>().sharedMaterial),
  colliders = prefab.GetComponentsInChildren<UnityEngine.Collider>(true).Length,
  scripts = prefab.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true).Length
};
