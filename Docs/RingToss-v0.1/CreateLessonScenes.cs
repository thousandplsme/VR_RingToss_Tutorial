var source="Assets/RingTossTutorial/Scenes/RingToss_v0_1.unity";
var folder="Assets/RingTossTutorial/Scenes/Lessons";
var starter=folder+"/01_Grab_Starter.unity";
var complete=folder+"/01_Grab_Complete.unity";
var active=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(EditorApplication.isPlaying || active.path!=source || active.isDirty) throw new System.Exception("Open the clean reference scene in Edit mode first.");
if(System.IO.File.Exists(starter)||System.IO.File.Exists(complete)) throw new System.Exception("Lesson scenes already exist; refusing overwrite.");
if(!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/RingTossTutorial/Scenes","Lessons");
if(!AssetDatabase.CopyAsset(source,starter)) throw new System.Exception("Copy failed.");
try {
    var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(starter);
    var grab=GameObject.Find("Ring").GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    grab.enabled=false;
    grab.throwOnDetach=false;
    grab.forceGravityOnDetach=false;
    EditorUtility.SetDirty(grab);
    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
    if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene)) throw new System.Exception("Starter save failed.");
    if(!AssetDatabase.CopyAsset(starter,complete)) throw new System.Exception("Complete copy failed.");
    scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(complete);
    grab=GameObject.Find("Ring").GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    grab.enabled=true;
    EditorUtility.SetDirty(grab);
    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
    if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene)) throw new System.Exception("Complete save failed.");
    var checks=new List<object>();
    foreach(var path in new[]{starter,complete,source}) {
        scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
        var ring=GameObject.Find("Ring");
        grab=ring.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        var missing=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
        bool expectedEnabled=path!=starter,expectedThrow=path==source;
        bool passed=grab.enabled==expectedEnabled && grab.throwOnDetach==expectedThrow && grab.forceGravityOnDetach==expectedThrow && missing==0 && grab.colliders.Count==16 && grab.startingSingleGrabTransformers.Count==1;
        if(!passed) throw new System.Exception("Lesson verification failed: "+path);
        checks.Add(new{path,passed,grab.enabled,grab.throwOnDetach,grab.forceGravityOnDetach,missingScripts=missing,colliders=grab.colliders.Count,saved=!scene.isDirty});
    }
    System.IO.File.WriteAllText("Docs/RingToss-v0.1/lesson-scene-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(checks,Newtonsoft.Json.Formatting.Indented));
    return checks;
} finally {
    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(source);
}
