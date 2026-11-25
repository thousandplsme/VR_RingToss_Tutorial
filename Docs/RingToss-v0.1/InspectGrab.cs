if(EditorApplication.isPlaying) throw new System.Exception("Edit mode required.");
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(scene.path!="Assets/RingTossTutorial/Scenes/RingToss_v0_1.unity") throw new System.Exception("Wrong scene.");
var ring=GameObject.Find("Ring");
var g=ring.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
var b=ring.GetComponent<Rigidbody>();
var t=ring.GetComponent<UnityEngine.XR.Interaction.Toolkit.Transformers.XRGeneralGrabTransformer>();
var hands=GameObject.Find("XR Origin").GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>();
var report=new {
    scene=scene.path,scene.isDirty,ringPosition=ring.transform.position.ToString(),
    grab=new {g.enabled,selectMode=g.selectMode.ToString(),movementType=g.movementType.ToString(),g.trackPosition,g.trackRotation,g.useDynamicAttach,g.attachEaseInTime,g.throwOnDetach,g.throwSmoothingDuration,g.throwVelocityScale,g.throwAngularVelocityScale,g.forceGravityOnDetach,g.addDefaultGrabTransformers,transformers=g.startingSingleGrabTransformers.Select(x=>x.GetType().Name).ToArray(),colliders=g.colliders.Count,layers=g.interactionLayers.value},
    rigidbody=new {b.mass,b.useGravity,b.isKinematic,interpolation=b.interpolation.ToString(),collision=b.collisionDetectionMode.ToString()},
    transformer=new {t.allowOneHandedScaling,t.allowTwoHandedScaling,axes=t.permittedDisplacementAxes.ToString()},
    hands=hands.Select(h=>new {h.name,h.enabled,trigger=h.selectActionTrigger.ToString(),inputMode=h.selectInput.inputSourceMode.ToString(),select=h.selectInput.inputActionReferencePerformed.action.actionMap.name+"/"+h.selectInput.inputActionReferencePerformed.action.name,value=h.selectInput.inputActionReferenceValue.action.name,layers=h.interactionLayers.value,h.improveAccuracyWithSphereCollider,radius=h.GetComponent<SphereCollider>().radius,isTrigger=h.GetComponent<SphereCollider>().isTrigger,trackingEnabled=h.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>().enabled}).ToArray()
};
if(hands.Any(h=>h.selectInput.inputSourceMode!=UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.InputActionReference || !h.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>().enabled)) throw new System.Exception("Runtime test state leaked into edit mode.");
if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene)) throw new System.Exception("Save failed.");
System.IO.File.WriteAllText("Docs/RingToss-v0.1/grab-inspector-values.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
return report;
