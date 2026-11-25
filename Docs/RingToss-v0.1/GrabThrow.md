# v0.1 掴む・投げる実装と検証

2026-09-25 JST。Unity Editor 6000.3.9f1、Editor登録済みXR Interaction Toolkit 3.3.1／Input System 1.18.0を確認。
Unity CLI → Pipelineの実Editor上で実装・検証。Inspectorの画面を目視したものではなく、同じコンポーネントの実値をEditor APIから取得した。

## 変更内容

教材シーン `Assets/RingTossTutorial/Scenes/RingToss_v0_1.unity` のRingのみ変更。

- XR General Grab Transformerを追加。
- XR Grab InteractableのStarting Single Grab Transformersにそのコンポーネントを1件登録。
- Add Default Grab TransformersをONからOFFへ変更し、実行時の暗黙追加を明示設定に置換。
- TransformerのAllow One Handed Scaling／Allow Two Handed ScalingをOFFに固定。
- それ以外の既存掴み・投擲・入力設定は維持。独自ランタイムスクリプトなし。
- 床、プレイヤー配置、得点、棒、UI、効果音、リセットは追加・変更していない。

導入済みXRIソースと実Editorでプロパティを確認してから操作。自動追加が壊れていたという判断ではなく、教材でInspectorから追従処理を明示確認できるようにしたもの。

## Inspector項目の実値

|対象|項目|確認値|
|---|---|---|
|Ring / Rigidbody|Mass|0.15 kg|
|同上|Use Gravity / Is Kinematic|ON / OFF|
|同上|Interpolate / Collision Detection|Interpolate / Continuous Dynamic|
|XR Grab Interactable|Select Mode|Single|
|同上|Movement Type|Velocity Tracking|
|同上|Track Position / Track Rotation|ON / ON|
|同上|Use Dynamic Attach|ON|
|同上|Attach Ease In Time|0.15 s|
|同上|Throw On Detach|ON|
|同上|Throw Smoothing Duration|0.25 s|
|同上|Throw Velocity Scale / Throw Angular Velocity Scale|1 / 1|
|同上|Force Gravity On Detach|OFF（元のRigidbodyの重力ONが復元されることをPlayで確認）|
|同上|Colliders|非Trigger CapsuleCollider 16個|
|同上|Interaction Layer Mask|Default（bit値1）|
|同上|Starting Single Grab Transformers|XRGeneralGrabTransformer 1件|
|同上|Add Default Grab Transformers|OFF|
|XR General Grab Transformer|Permitted Displacement Axes|All|
|同上|One Handed / Two Handed Scaling|OFF / OFF|
|左右XR Direct Interactor|Select Action Trigger|State Change|
|同上|Select Input Source|Input Action Reference|
|同上|Select / Select Value|各XRI Left/Right Interactionの同名アクション|
|同上|Interaction Layer Mask|Everything（bit値-1）、輪のDefaultと交差|
|同上|SphereCollider|Is Trigger ON、Radius 0.08m|
|同上|Improve Accuracy With Sphere Collider|ON|
|左右Tracked Pose Driver|Enabled|ON|

State Changeなので、輪に近づけてからGripを押す。押している間は保持し、離すと解放する。Toggle／Stickyではない。
既存Input Actions内のXRController GripButton／Gripを使う。入力アセット、Package設定は変更していない。

## Editorで確認できた動作

Playモードで`ProbeGrab.cs`を実行。TrackedPoseDriverを一時無効化し、XRInputButtonReaderをManualValueに切り替え、疑似GripとコントローラーTransform移動を与えた。
XRInteractionManager.SelectEnterを直接呼ぶ方法ではなく、近接Hover→入力→Select→追従→入力解放→投擲という通常のインタラクション処理を通して確認。
輪への解放速度は手で設定せず、XRI自身に算出させた。輪の初期化はテスト準備のみであり、ゲームのリセット機能は追加していない。

|結果|左|右|
|---|---:|---:|
|床で静止した中心Y|0.025m|0.025m|
|近接Hover|成功|成功|
|疑似GripによるSelect|成功|成功|
|持ち上げ後の中心Y|1.022m|1.025m|
|解放|成功|成功|
|解放約0.08秒後の前方速度Z|1.753m/s|1.758m/s|
|同時点の角速度Y|2.106rad/s|2.111rad/s|
|解放後約0.38秒の前方変位|0.701m|0.668m|
|解放後の重力ON・落下|確認|確認|

測定値はフレーム間隔によって変わる。完全な弾道・衝突耐性の試験ではない。
生の結果：`grab-play-results.json`。構成値：`grab-inspector-values.json`。
後者の`isDirty: true`は最終SaveScene呼び出し前の値。その後保存成功し、list_open_scenesで`isDirty: false`を別途確認済み。

## Console

- 開始前：既存PipelineタイムアウトError 1、既存AndroidXRSettingsInitializer時刻不一致Warning 1。
- Play後・終了後の実Console（groundTruth）：Error 0、Warning 0、Compilation Failed=false。
- Pipelineの履歴バッファには以前のError 1／Warning 1が残る。これは現在のConsole件数とは異なる。
- この作業ではConsole Clearコマンドは実行していない。Playへの遷移後に実Console件数が0になったことを確認した。
- 今回の実装・Playテストで追加エラーなし。

## 保存と影響範囲

Play停止後、左右のSelect InputがInputActionReferenceへ、TrackedPoseDriverがEnabledへ戻ったことを確認。
輪の位置も(0,0.03,0.65)へ復帰。疑似入力・テスト中の移動は保存していない。
シーンを保存し、開いているシーンのisDirty=falseを確認。
作業前1,248ファイルのSHA-256を照合し、変更は教材シーン1ファイルだけ。Packages／ProjectSettings／既存Input Actions／既存シーンは変更なし。
Docs内には実装コード、検証コード、結果、記録を追加した。Assets内へ検証用スクリプトは入れていない。

再現：同じ保存ポイントで`ConfigureGrab.cs`をPipeline eval_fileで実行して保存。Play開始後だけ`ProbeGrab.cs`を実行し、結果ファイルの左右passed=trueを確認。その後Play停止し`InspectGrab.cs`で実入力設定と保存を確認する。出力先Docs/RingToss-v0.1が必要。

## Quest実機で必要な確認（未実施）

- Quest 2のOpenXR起動、HMD／左右Touchコントローラー追跡、床高さ。
- 実コントローラーのGrip押下・保持・解放が入力アセットへ届くこと。
- 左右で掴む位置、届く距離、動的Attachの自然さ、投げる方向・距離・回転。
- 実速度で床への衝突、高速投擲でのすり抜け、フレームレート。
- Quest Pro／Quest 3Sでも同じ基本操作が成立すること。

今回の疑似入力テストは実機Input Systemのバインド解決や追跡品質を検証していない。機種固有機能を必要とするコンポーネントは追加していない。
