// Play-mode-only synthetic XRI input test. Never saves runtime changes.
if(!EditorApplication.isPlaying) throw new System.Exception("Play mode required.");
var ring=GameObject.Find("Ring");
var grab=ring.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
var body=ring.GetComponent<Rigidbody>();
var rig=GameObject.Find("XR Origin");
var hands=rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>();
var placement=ring.GetComponent(AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/RingTossTutorial/RingInitialPlacement.cs").GetClass());
var returnMethod=placement.GetType().GetMethod("TryPlaceInFront");
var head=rig.GetComponentInChildren<Camera>().transform;
bool rejectedOnRelease=false; grab.selectExited.AddListener(args=>{ rejectedOnRelease=!(bool)returnMethod.Invoke(placement,null); }); var results=new List<object>();
var oldBackground=Application.runInBackground;
Application.runInBackground=true;
foreach(var driver in rig.GetComponentsInChildren<UnityEngine.InputSystem.XR.TrackedPoseDriver>()) driver.enabled=false;
foreach(var hand in hands) {
    hand.selectInput.inputSourceMode=UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.ManualValue;
    hand.selectInput.QueueManualState(false,0);
    hand.transform.position=new Vector3(10,1,0);
}
System.Collections.IEnumerator Wait(float seconds) {
    float start=Time.time;
    double deadline=EditorApplication.timeSinceStartup+20;
    while(EditorApplication.isPlaying && Time.time-start<seconds && EditorApplication.timeSinceStartup<deadline) { EditorApplication.QueuePlayerLoopUpdate(); yield return null; }
}
System.Collections.IEnumerator Run() {
    foreach(var hand in hands.OrderBy(h=>h.name)) {
        body.useGravity=true; body.linearVelocity=Vector3.zero; body.angularVelocity=Vector3.zero;
        body.position=new Vector3(0,0.03f,0.65f); body.rotation=Quaternion.identity;
        yield return Wait(0.4f);
        var floorY=body.position.y;
        var start=body.position+new Vector3(0.16f,0,0);
        hand.transform.position=start; Physics.SyncTransforms();
        yield return Wait(0.4f);
        bool hovered=hand.interactablesHovered.Contains(grab);
        hand.selectInput.QueueManualState(true,1);
        yield return Wait(0.35f);
        bool selected=grab.isSelected && grab.interactorsSelecting.Contains(hand);
        bool heldReturnRejected=!(bool)returnMethod.Invoke(placement,null); float t0=Time.time;
        while(Time.time-t0<0.7f) { float t=Mathf.Clamp01((Time.time-t0)/0.7f); hand.transform.position=start+new Vector3(0,t,0); yield return null; }
        yield return Wait(0.2f);
        float liftedY=body.position.y;
        var launchStart=hand.transform.position;
        t0=Time.time;
        while(Time.time-t0<0.4f) {
            float t=Time.time-t0;
            hand.transform.position=launchStart+new Vector3(0,0.5f*t,1.5f*t);
            hand.transform.rotation=Quaternion.Euler(0,120f*t,0);
            yield return null;
        }
        var releasePosition=body.position;
        hand.selectInput.QueueManualState(false,0);
        yield return Wait(0.08f);
        bool released=!grab.isSelected;
        var velocity=body.linearVelocity; var angular=body.angularVelocity;
        hand.transform.position=new Vector3(10,1,0);
        yield return Wait(0.3f);
        var displacement=body.position-releasePosition;
        var gravityAfterThrow=body.useGravity;
head.position=new Vector3(0,1.6f,0); head.rotation=Quaternion.Euler(0,180,0);
bool returned=(bool)returnMethod.Invoke(placement,null);
var expected=head.position+Vector3.back*0.45f-Vector3.up*0.35f;
bool positionCorrect=Vector3.Distance(body.position,expected)<0.001f;
bool stopped=body.linearVelocity.sqrMagnitude<0.001f && body.angularVelocity.sqrMagnitude<0.001f && !body.useGravity;
bool repeated=(bool)returnMethod.Invoke(placement,null) && Vector3.Distance(body.position,expected)<0.001f;
head.rotation=Quaternion.Euler(90,0,0);
bool verticalRejected=!(bool)returnMethod.Invoke(placement,null);
head.rotation=Quaternion.identity;
bool passed=rejectedOnRelease && heldReturnRejected && returned && positionCorrect && stopped && repeated && verticalRejected && gravityAfterThrow && hovered && selected && liftedY>0.7f && released && velocity.z>0.3f && angular.magnitude>0.2f && displacement.z>0.1f;
        results.Add(new {hand=hand.name,passed,rejectedOnRelease,heldReturnRejected,returned,positionCorrect,stopped,repeated,verticalRejected,gravityAfterThrow,hovered,selected,liftedY,released,floorY,releaseVelocity=new[]{velocity.x,velocity.y,velocity.z},releaseAngularVelocity=new[]{angular.x,angular.y,angular.z},postReleaseDisplacement=new[]{displacement.x,displacement.y,displacement.z},gravity=body.useGravity});
        hand.transform.rotation=Quaternion.identity;
        yield return Wait(1f);
    }
    Application.runInBackground=oldBackground;
    var report=Newtonsoft.Json.JsonConvert.SerializeObject(new {method="Play-mode ManualValue XRI reader; tracked pose disabled temporarily; return method invoked directly; no physical A button or head-tracking validation",results},Newtonsoft.Json.Formatting.Indented);
    System.IO.File.WriteAllText("Docs/RingToss-v0.1/return-play-results.json",report);
    SessionState.SetString("RingTossReturnProbe",report);
}
SessionState.SetString("RingTossReturnProbe","running");
Unity.EditorCoroutines.Editor.EditorCoroutineUtility.StartCoroutineOwnerless(Run());
return "Started left/right synthetic grab, lift, release and throw probe.";


