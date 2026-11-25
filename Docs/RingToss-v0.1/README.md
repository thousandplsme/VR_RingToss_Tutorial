# VR輪投げ v0.1 — 教材用保存ポイント

学習者向けの[教材初稿・開始シーン案内](../Lessons/README.md)を追加しました。新しい教材シーンは部員再現・実機テスト待ちです。

**教材化に使う最新結果・設定は [CurrentState.md](CurrentState.md) に集約。** Quest 2で配置・掴み・投擲・右Aによる再投球は本人から成功報告あり。実行方式や特殊操作の確認範囲は同文書を参照。

再投球機能の実装履歴は [ReturnToPlayer.md](ReturnToPlayer.md)。以下の記録は各作業時点の履歴で、最新値はCurrentState.mdを優先する。

最新の初期配置修正は [InitialPlacement.md](InitialPlacement.md)、その後のQuest 2実機成功報告は [Quest2UserResult.md](Quest2UserResult.md) を参照。

後続の掴む・投げる実装とPlay検証は [GrabThrow.md](GrabThrow.md) を参照。以下は初回保存ポイント時点の記録。

作業日：2026-09-25（JST）。この段階はシーンとコンポーネントの保存・再読込確認まで。
Quest 2での実機操作、Quest Pro／Quest 3S互換性、ビルド、Playモードは未検証。

## 環境と接続

- Editorから確認したUnity：6000.3.9f1。プロジェクト：VR_RingToss_Tutorial。
- 開始時のシーン：Assets/Scenes/SampleScene.unity。未保存変更なし。
- Pipeline 0.7.0-exp.1。サンドボックス内では接続不可、外からUnity CLIでreadyを確認。
- 本作業はUnity CLIからPipelineのEditor操作を使用。会話に公開されたUnity MCPツールによる操作ではない。
- manifest/lock確認値：XRI 3.3.1、OpenXR 1.16.1、XR Management 4.5.3、Input System 1.18.0、XR Core Utilities 2.5.3、XR Hands 1.7.3、Android XR OpenXR 1.1.0、AR Foundation 6.3.2、XR Legacy Input Helpers 2.1.13、URP 17.3.0。
- 使用した既存入力サンプルのフォルダーはXRI 3.3.0。パッケージ本体の版とは区別する。
- OpenXRのOculus Touch Controller ProfileはAndroid／Standaloneで有効とファイル上で確認。既存OpenXR設定は編集していない。

## 追加ファイル・既存設定への影響

- Assets/RingTossTutorial/Scenes/RingToss_v0_1.unity
- Assets/RingTossTutorial/Meshes/RingPlaceholder.asset
- Assets/RingTossTutorial/Materials/{Floor,Ring,ControllerMarker}.mat
- 上記とフォルダーに対応する.meta。
- Docs/RingToss-v0.1/：この記録、作成・検証用コード、作業前ハッシュ一覧。

Assets、Packages、ProjectSettingsに作業前から存在した1,234ファイルをSHA-256で照合し、変更・削除0件。
既存シーン、Input Actions、Project Settings、manifest、lock、ビルドシーン一覧は変更していない。
新規シーンは現在Editorで開いている。ビルド一覧への登録は未実施。
得点・的・UI・効果音は追加していない。ランタイム独自スクリプトも追加していない。

## Hierarchy

```text
RingToss_v0_1
├─ Floor
├─ Directional Light
├─ XR Interaction Manager
├─ XR Origin
│  └─ Camera Offset
│     ├─ Main Camera
│     ├─ Left Controller
│     │  └─ Controller Marker
│     └─ Right Controller
│        └─ Controller Marker
└─ Ring
   └─ Collider_00 ～ Collider_15
```

## 再現する設定

1. 既存シーンに未保存変更がないことを確認し、空の新規シーンを別名で作る。
2. Floor：Cube、位置(0,-0.05,1)、スケール(4,0.1,4)。床上面はY=0、BoxColliderあり。
3. Directional Light：回転(50,-30,0)、強度1.2。環境光はFlat、RGB各0.55。
4. 空オブジェクトにXR Interaction Managerを追加。
5. XR Origin：XROriginとInputActionManager。原点(0,0,0)、Tracking Origin Mode=Floor、Camera Y Offset=1.6。Camera Floor Offset Objectに子のCamera Offset、CameraにMain Cameraを設定。
6. InputActionManagerに既存の `Assets/Samples/XR Interaction Toolkit/3.3.0/Starter Assets/XRI Default Input Actions.inputactions` を登録。入力アセット自体を編集しない。
7. Main Camera：Tag=MainCamera、ローカル位置(0,1.6,0)、Near=0.05、Far=50。TrackedPoseDriverのPosition／Rotation／Tracking StateにXRI Headの同名アクション参照を設定。
8. 左右Controller：TrackedPoseDriverでXRI Left／XRI RightのPosition／Rotation／Tracking Stateを参照。SphereColliderはIs Trigger、Radius=0.08。XRDirectInteractorにManagerを明示指定し、Improve Accuracy With Sphere Colliderを有効化。Select InputはInput Action Reference、Performedに各InteractionマップのSelect、ValueにSelect Valueを指定。子のMarkerは直径0.05の球、Colliderなし。
9. Ring：位置(0,0.03,0.65)。床上で開始するため、近接して拾う。中心線半径0.16、断面半径0.025（外径0.37m）の仮トーラス。メッシュは周方向32×断面8分割。生成方法はCreateScene.csに記録。
10. RingのRigidbody：Mass=0.15kg、Use Gravity=ON、Is Kinematic=OFF、Interpolate、Continuous Dynamic。
11. 穴を保つため、輪の円周を16本のCapsuleColliderで近似。各子を隣接する円周点の中間へ置き、ローカルY軸を両点の方向へ向ける。Radius=0.025、Height=両点間距離+0.05、Is Trigger=OFF。子にRigidbodyを追加しない。
12. RingのXRGrabInteractable：Managerを明示指定、Collidersに上記16個を登録、Use Dynamic Attach=ON、Movement Type=Velocity Tracking、Throw On Detach=ON、Throw Velocity Scale=1、Throw Angular Velocity Scale=1。その他は導入済みXRIの既定値。
13. 新規シーンだけを保存して再読込し、参照とコンポーネントを確認する。

左右Selectには既存の共通XRController GripButton、Select ValueにはGripのバインドが存在する。位置・回転も既存コントローラー追跡を参照する。
入力アセットには元から手・視線などの追加バインドも含まれるが、新規シーンには手や視線のInteractor、機種固有コンポーネントは追加していない。
スティック移動・テレポート・リセット・遠隔掴みは組み込んでいない。床外へ投げた輪の回収はこの段階では未対応。

## 作成コードの使い方

`CreateScene.cs`はPipeline `eval_file`向けのコード本文であり、Assets内へ置くランタイム／Editorクラスではない。
出力フォルダーが存在する場合、Play中、コンパイル中、シーンに未保存変更がある場合は作成を拒否する。
現プロジェクトは既に出力が存在するため、再実行しない。再現時は同じ依存アセットを持つ別の作業コピーで使う。

```powershell
unity command eval_file --file 'Docs/RingToss-v0.1/CreateScene.cs' --caller plugin --skill unity-cli --project-path '<プロジェクトの絶対パス>' --format json
unity command eval_file --file 'Docs/RingToss-v0.1/VerifyScene.cs' --caller plugin --skill unity-cli --project-path '<プロジェクトの絶対パス>' --format json
```

初回作成ではコード評価形式の違いを修正後、シーン切替前に保持した入力参照の検索が途中で失敗した。
シーン切替後にAssetDatabaseから入力アセット・参照を再取得する形に修正し、`ResumeScene.cs`で作成途中の新規シーンのみを完成した。
`ResumeScene.cs`はその復旧記録であり、通常の再現手順では実行しない。完成版CreateScene.csを初めから再実行する試験は未実施。

## 確認結果

- 保存成功。保存したシーンをEditorで再読込し、未保存変更なし。
- ルート5個。Ring：Transform、MeshFilter、MeshRenderer、Rigidbody、XRGrabInteractable。
- 頭・左右のTrackedPoseDriver合計3個、位置・回転・Tracking Stateの参照が有効。
- XRDirectInteractor合計2個、左右のSelect／Select Value参照とManager参照が有効。
- InputActionManagerの入力アセット参照が有効。
- RingのMesh参照と16個の非Trigger Collider参照が有効。
- 欠落スクリプト0、Canvas 0、AudioSource 0。
- Console：開始時Error 0／Warning 1。終了時Error 1／Warning 1。ログは消去していない。
- Errorの内容：調査中のPipeline初回読み取りが5,000msでタイムアウト（`Failed to handle /api/exec request: Main thread operation timed out after 5000ms`）。再試行成功。その後の保存・再読込検証で追加Consoleエラーなし。
- 既存Warning：AndroidXRSettingsInitializerのSourceAssetDBとディスク上の更新時刻不一致。
- コンパイル失敗フラグはfalse。コード評価の初期失敗はツール応答に記録されたもので、保存したシーンに独自スクリプトは含めていない。

## 次段階で確認すること（未実施）

Quest 2で頭・左右コントローラーの追跡、床高さ、輪への近接、左右Gripによる取得と解放、投擲速度・回転、床衝突を確認する。
Quest Pro／Quest 3Sは展示候補として同じ操作を実機確認する。現時点ではいずれも動作保証・検証済みとはしない。
