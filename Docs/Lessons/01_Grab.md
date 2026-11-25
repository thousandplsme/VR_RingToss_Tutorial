# 01_輪を掴もう

## 目的と前提

コントローラーを輪に近づけ、Gripを押して保持し、手を動かすと輪がついてくる状態を作ります。今回の中心は「掴める対象」「掴む側」「両者を管理するもの」の関係です。
接続教材を終え、配布プロジェクトを開けることが前提です。環境と保存点は[教材入口](README.md)を確認してください。

## 1. 開始シーンを自分用に保存する

Playを停止し、Projectで`Assets/RingTossTutorial/Scenes/Lessons/01_Grab_Starter.unity`を開きます。
File > Save Asで`My_01_Grab.unity`などの自分用シーンに保存します。参考完成シーンを上書きしません。

HierarchyにはFloor、Directional Light、XR Interaction Manager、XR Origin、Ringがあります。
XR Origin > Camera Offsetの下にはMain Camera、Left Controller、Right Controllerがあります。
開始時のRingには必要な部品が入っていますが、XR Grab InteractableのチェックがOFFなので掴めません。故障ではなく、ここから有効化する練習です。

## 2. 輪の形と物理を確認する

HierarchyでRingを選びます。子のCollider_00〜15は輪の円周を近似するCapsuleColliderです。輪全体を大きな一個の凸Colliderで塞がないことで穴を残しています。
MeshFilter／MeshRendererは見た目、Colliderは当たり判定、Rigidbodyは物理運動を担当します。

Rigidbodyを次の値にします。配布状態では設定済みなので、違う場合だけ直してください。

|Inspector項目|値|理由|
|---|---|---|
|Mass|0.15|この教材の輪の基準質量|
|Use Gravity|OFF|前方で掴むまで浮かせて待つ|
|Is Kinematic|OFF|物理による運動を使う|
|Interpolate|Interpolate|描画時の動きを補間する|
|Collision Detection|Continuous Dynamic|高速移動時の衝突検出を補助する。すり抜け防止を保証するものではない|

子のColliderに別のRigidbodyを追加しないでください。輪全体を一つの物体として扱います。

## 3. 掴む側を確認する

Left ControllerとRight Controllerを順に選び、XR Direct Interactorを確認します。

|項目|値|
|---|---|
|Interaction Manager|HierarchyのXR Interaction Manager|
|Select Action Trigger|State Change|
|Select Inputの入力方式|Input Action Reference|
|左のSelect／Select Value|XRI Left Interactionの同名アクション|
|右のSelect／Select Value|XRI Right Interactionの同名アクション|
|SphereCollider|Is Trigger ON、Radius 0.08|
|Improve Accuracy With Sphere Collider|ON|

参照は配布済みです。空になっていた場合は参考01完成シーンと照合し、アクションを適当に新規作成しないでください。
State Changeでは、先に輪へ近づけてからGripを押します。離れた場所で押し始めたまま近づけても掴めない場合があります。

## 4. 掴める輪にする

RingのXR Grab Interactableを確認します。

|項目|この章の値|
|---|---|
|Interaction Manager|XR Interaction Manager|
|Interaction Layer Mask|Default（左右InteractorはEverythingで含む）|
|Select Mode|Single|
|Movement Type|Velocity Tracking|
|Track Position／Track Rotation|ON／ON|
|Use Dynamic Attach|ON|
|Attach Ease In Time|0.15|
|Colliders|16個。子のCapsuleCollider参照がMissingでない|
|Starting Single Grab Transformers|RingのXR General Grab Transformer 1件|
|Add Default Grab Transformers|OFF|
|Throw On Detach|**OFF：投擲は次の章**|
|Force Gravity On Detach|**OFF：次の章で変更**|

XR General Grab TransformerのPermitted Displacement AxesはAll、Allow One Handed Scaling／Allow Two Handed ScalingはOFFです。
最後に、**XR Grab Interactableのコンポーネント名の左にある有効チェックをON**にします。GameObjectのActiveチェックと取り違えないでください。Ctrl+Sで保存します。

## 5. 動作を確かめる

EditorのPlayだけでは実コントローラーの操作は確認できません。接続教材の手順でQuestへ実行します。
Build Profilesの実際に使用するScene Listに自分のシーンが含まれ、最初に起動することを確認してください。開始／参考完成シーンを全部ビルドに入れる必要はありません。開いているシーンが自動的に起動シーンになるとは限りません。

1. 輪が視点の前方に出ることを確認する。
2. 輪の縁にコントローラーを近づけ、Gripを押す。
3. Gripを押したまま手をゆっくり移動・回転し、輪が追従することを確かめる。
4. Gripを離し、保持が終了することを確かめる。
5. 左右で試し、輪を回収したいときは離してから右Aを押す。

Throw On DetachがOFFでも、Velocity Tracking中の物理速度などで解放後に動く可能性があります。「必ず空中で静止する」はこの章の合格条件にしません。投擲速度の引き継ぎと重力は次章で扱います。

## 完成チェック・困ったとき

- [ ] 自分用シーンを保存した。
- [ ] 輪を近接Gripで掴み、保持中の位置・回転追従を確認した。
- [ ] Gripを離すと保持が終わる。
- [ ] 機種とLink／単体アプリの別をチェック票へ記録した。

掴めない：XR Grab Interactableの有効チェック、Gripを押す順序、Manager参照、Collider参照、Interaction Layerの重なりを順に確認します。
輪がない：別シーンをビルドしていないか、頭の追跡とRingInitialPlacementのHead参照を確認します。頭へRingを親子付けする修正はしません。
片手だけ動かない：左右のTracked Pose Driverと入力参照を参考シーンと比較します。

Challenge：自分用シーンでUse Dynamic AttachをOFFにした場合の掴む位置を比較し、確認後ONへ戻す。基本の完成チェックが終わってから行います。
完成例：`Assets/RingTossTutorial/Scenes/Lessons/01_Grab_Complete.unity`。この教材から別の部員が再現できるかは未検証です。
