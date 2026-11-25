var rootPath="Assets/RingTossTutorial";
var scenePath=rootPath+"/Scenes/RingToss_v0_1.unity";
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.path != "" || scene.GetRootGameObjects().Length != 4) throw new System.Exception("Unexpected scene; stop.");
var inputPath="Assets/Samples/XR Interaction Toolkit/3.3.0/Starter Assets/XRI Default Input Actions.inputactions";
UnityEngine.InputSystem.InputActionReference Ref(string map,string action) => AssetDatabase.LoadAllAssetsAtPath(inputPath).OfType<UnityEngine.InputSystem.InputActionReference>().First(r=>r.action!=null && r.action.actionMap.name==map && r.action.name==action);
var rig=GameObject.Find("XR Origin");
var origin=rig.GetComponent<Unity.XR.CoreUtils.XROrigin>();
var offset=rig.transform.Find("Camera Offset").gameObject;
var manager=GameObject.Find("XR Interaction Manager").GetComponent<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();
rig.GetComponent<UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager>().actionAssets=new List<UnityEngine.InputSystem.InputActionAsset>{AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(inputPath)};
var ringMat=AssetDatabase.LoadAssetAtPath<Material>(rootPath+"/Materials/Ring.mat");
var handMat=AssetDatabase.LoadAssetAtPath<Material>(rootPath+"/Materials/ControllerMarker.mat");
void Tracking(GameObject go, string map) {
    var driver = go.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>() ?? go.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
    driver.positionInput = new UnityEngine.InputSystem.InputActionProperty(Ref(map, "Position"));
    driver.rotationInput = new UnityEngine.InputSystem.InputActionProperty(Ref(map, "Rotation"));
    driver.trackingStateInput = new UnityEngine.InputSystem.InputActionProperty(Ref(map, "Tracking State"));
}
var cameraGo = offset.transform.Find("Main Camera").gameObject; cameraGo.tag = "MainCamera";
cameraGo.transform.SetParent(offset.transform, false); cameraGo.transform.localPosition = new Vector3(0, 1.6f, 0);
var camera = cameraGo.GetComponent<Camera>(); camera.nearClipPlane = 0.05f; camera.farClipPlane = 50;
camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.12f, 0.17f, 0.23f);
origin.Camera = camera; Tracking(cameraGo, "XRI Head");
foreach (var side in new[]{"Left", "Right"}) {
    var controller = new GameObject(side + " Controller"); controller.transform.SetParent(offset.transform, false);
    Tracking(controller, "XRI " + side);
    var sphere = controller.AddComponent<SphereCollider>(); sphere.isTrigger = true; sphere.radius = 0.08f;
    var direct = controller.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>(); direct.interactionManager = manager;
    direct.improveAccuracyWithSphereCollider = true;
    direct.selectInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.InputActionReference;
    direct.selectInput.inputActionReferencePerformed = Ref("XRI " + side + " Interaction", "Select");
    direct.selectInput.inputActionReferenceValue = Ref("XRI " + side + " Interaction", "Select Value");
    var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere); marker.name = "Controller Marker";
    UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
    marker.transform.SetParent(controller.transform, false); marker.transform.localScale = Vector3.one * 0.05f;
    marker.GetComponent<Renderer>().sharedMaterial = handMat;
}
// A torus visual and 16 capsule colliders preserve the hole without a concave dynamic MeshCollider.
const int segments = 32, sides = 8, colliderCount = 16;
const float radius = 0.16f, tube = 0.025f;
var vertices = new Vector3[segments * sides]; var triangles = new int[segments * sides * 6];
for (int i = 0; i < segments; i++) for (int j = 0; j < sides; j++) {
    float a = 2 * Mathf.PI * i / segments, b = 2 * Mathf.PI * j / sides;
    vertices[i*sides+j] = new Vector3((radius+tube*Mathf.Cos(b))*Mathf.Cos(a), tube*Mathf.Sin(b), (radius+tube*Mathf.Cos(b))*Mathf.Sin(a));
    int v = i*sides+j, next = ((i+1)%segments)*sides+j, jn = i*sides+(j+1)%sides, nn = ((i+1)%segments)*sides+(j+1)%sides;
    int t = v*6; triangles[t]=v; triangles[t+1]=jn; triangles[t+2]=next; triangles[t+3]=jn; triangles[t+4]=nn; triangles[t+5]=next;
}
var mesh = new Mesh {name="RingPlaceholder", vertices=vertices, triangles=triangles}; mesh.RecalculateNormals(); mesh.RecalculateBounds();
AssetDatabase.CreateAsset(mesh, rootPath + "/Meshes/RingPlaceholder.asset");
var ring = new GameObject("Ring"); ring.transform.position = new Vector3(0, 0.03f, 0.65f);
ring.AddComponent<MeshFilter>().sharedMesh = mesh; ring.AddComponent<MeshRenderer>().sharedMaterial = ringMat;
var body = ring.AddComponent<Rigidbody>(); body.mass = 0.15f; body.useGravity = true;
body.interpolation = RigidbodyInterpolation.Interpolate; body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
var colliders = new List<Collider>();
for (int i=0; i<colliderCount; i++) {
    float a=2*Mathf.PI*i/colliderCount, b=2*Mathf.PI*(i+1)/colliderCount;
    var p=new Vector3(radius*Mathf.Cos(a),0,radius*Mathf.Sin(a)); var q=new Vector3(radius*Mathf.Cos(b),0,radius*Mathf.Sin(b));
    var part=new GameObject("Collider_" + i.ToString("00")); part.transform.SetParent(ring.transform,false);
    part.transform.localPosition=(p+q)*0.5f; part.transform.localRotation=Quaternion.FromToRotation(Vector3.up,(q-p).normalized);
    var capsule=part.AddComponent<CapsuleCollider>(); capsule.direction=1; capsule.radius=tube; capsule.height=(q-p).magnitude+2*tube;
    colliders.Add(capsule);
}
var grab=ring.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>(); grab.interactionManager=manager;
grab.colliders.Clear(); grab.colliders.AddRange(colliders); grab.useDynamicAttach=true;
grab.movementType=UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.VelocityTracking; grab.throwOnDetach=true;
grab.throwVelocityScale=1f; grab.throwAngularVelocityScale=1f;
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
if (!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene,scenePath)) throw new System.Exception("Scene save failed.");
return new { scene=scene.path, roots=scene.GetRootGameObjects().Select(g=>g.name).ToArray(), colliders=colliders.Count, saved=!scene.isDirty };



