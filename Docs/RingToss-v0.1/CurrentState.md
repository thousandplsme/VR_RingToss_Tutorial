# VR輪投げ v0.1 — 教材化の基準となる最新状態

整理日：2026-09-27 JST。今後の教材初稿はこの文書の設定・検証範囲を基準にする。過去の作業記録は履歴として残し、古い値を現行手順へ混ぜない。
今回の作業は読み取り調査と文書更新のみ。シーン・スクリプト・パッケージ・入力設定は変更していない。Notion未反映。

## 1. 現在の到達点

「輪が前方に出る → コントローラーで掴む → 離して投げる → 右Aで戻す → 再び投げる」まで。
ユーザー本人からQuest 2で基本操作と再投球の成功報告あり。的・得点・UI・ゲームの終了条件はまだない。

対象シーン：`Assets/RingTossTutorial/Scenes/RingToss_v0_1.unity`。
2026-09-27のEditor読み取りでは当該シーンがアクティブ、Play停止中、未保存変更なし。
現在のファイルが実機で試したAPKと完全一致するかは、APK識別子・ビルド時点が未記録のため照合できていない。

## 2. 実機結果と証拠の区別

|項目|状態|根拠・限界|
|---|---|---|
|前方への初期配置・掴み・投擲|Quest 2で成功|本人報告「配置・掴み・投擲は問題なく可能」|
|投げる→右Aで戻す→再び投げる|Quest 2で成功|本人報告「問題なく実行ができました」|
|左右それぞれの近接・取得・追従・解放・投擲|Editor疑似入力で成功|実機報告では左右を個別に特定していない|
|保持中・解放直後の復帰拒否|Editorで成功|疑似Gripと復帰メソッドの直接呼び出し|
|180度向き変更後の復帰・速度消去・繰り返し復帰|Editorで成功|復帰メソッド直接呼び出し。実ボタンの連打試験とは異なる|
|実機でA長押し・連打・10回連続投球|未確認|個別の結果報告なし|
|追跡喪失時の操作、再追跡時の挙動|実機未確認|コードに追跡チェックあり。動作検証とは区別|
|Quest Pro／Quest 3S|未確認|Quest 2成功を転用しない|
|Starterと教材だけで部員が再現|未確認|教材初稿・Starterは未整備|

実機結果は2026-09-27の整理時点までに受領した報告。実施時刻・試行回数・APK識別子は未記録。
実行方式は今回の追加回答により、**Build And RunでQuest 2へ入れた単体アプリ**と確認。配置・掴み・投擲・右Aによる再投球の成功報告に適用する。Quest Link＋Unity Playでの実機成功とは記載しない。
以前のテストログが「未確認」と記載しているのはその時点の履歴であり、最新の判定は上表を参照する。

## 3. 環境（2026-09-27の実Editorで取得）

|項目|版|
|---|---|
|Unity|6000.3.9f1|
|XR Interaction Toolkit|3.3.1|
|OpenXR Plugin|1.16.1|
|XR Plug-in Management|4.5.3|
|Input System|1.18.0|
|XR Core Utilities|2.5.3|
|XR Hands|1.7.3|
|Android XR OpenXR|1.1.0|
|AR Foundation|6.3.2|
|XR Legacy Input Helpers|2.1.13|
|URP|17.3.0|

入力には取り込み済みXRI **3.3.0**サンプルの`Starter Assets/XRI Default Input Actions.inputactions`を使用。導入パッケージ本体3.3.1とは区別する。上表の全パッケージがこの教材の必須依存であるという意味ではない。
根拠：`current-editor-readback-2026-09-27.json`。Unity CLIからPipeline経由でコンポーネント実値を取得した。Inspector画面の目視確認ではない。

## 4. 教材に採用する設定

### 輪

コンポーネント：Transform、MeshFilter、MeshRenderer、Rigidbody、XR Grab Interactable、XR General Grab Transformer、RingInitialPlacement、RingReturnToPlayer。
子のCollider_00〜15にCapsuleCollider。輪の穴を残した複合Colliderとして使い、子にRigidbodyは追加しない。

|対象|項目|現在値|
|---|---|---|
|Transform|保存時の位置|(0, 1.25, 0.45)。起動後は頭基準で配置|
|Rigidbody|Mass|0.15kg|
|同上|Use Gravity|**OFF（取得待ち・復帰時）**|
|同上|Is Kinematic|OFF|
|同上|Interpolate|Interpolate|
|同上|Collision Detection|Continuous Dynamic|
|XR Grab Interactable|Select Mode|Single|
|同上|Movement Type|Velocity Tracking|
|同上|Track Position / Rotation|ON / ON|
|同上|Use Dynamic Attach|ON|
|同上|Attach Ease In Time|0.15秒|
|同上|Throw On Detach|ON|
|同上|Throw Smoothing Duration|0.25秒|
|同上|Throw Velocity / Angular Velocity Scale|1 / 1|
|同上|Force Gravity On Detach|**ON（投擲後の重力）**|
|同上|Colliders|16個|
|同上|Starting Single Grab Transformers|XR General Grab Transformer 1件|
|同上|Add Default Grab Transformers|OFF|

XR General Grab Transformerの拡縮禁止は前回Editor記録・保存シーン設定に基づく。輪の大きさを変える操作は教材に含めない。

### プレイヤー・入力

- XR Origin：Tracking Origin Mode=Floor、Camera Y Offset=1.6。実追跡時の目線高さを常に1.6mへ固定する設定ではない。
- 左右XR Direct Interactor：Select Action Trigger=State Change。近接してからGripを押して掴み、離して投げる。
- Select Input：Input Action Reference。`XRI Left Interaction/Select`と`Select Value`、右は対応するRight側。
- SphereCollider半径0.08m、Improve Accuracy With Sphere Collider=ON。
- 再投球：RingReturnToPlayerが専用InputActionを所有。`<XRController>{RightHand}/{PrimaryButton}`（右A）の押し始めを検出。既存Input Actionsアセットを書き換えない。
- 現シーンでは移動・ジャンプ機能なし。既存サンプルのJumpと右Aの割り当てが重なるため、将来移動を追加する際は見直す。

### 配置と復帰（現行コード・保存シーンから確認）

- Head参照はMain Camera。水平前方0.45m、目線から下0.35m。
- 初期配置は頭のisTrackedが0.25秒続いた後に一度だけ行う。姿勢が静止したことを測定する処理ではない。
- 頭が追跡されない間は初期配置／右A操作を行わない。初期追跡待ち中は保存位置に輪が存在する。
- 右Aで現在の視点前方へ戻し、姿勢を水平、速度・角速度を0、重力をOFFにする。
- 掴み中、解放フレームと次フレーム、水平前方が決まらないほど上／下を向いた場合は復帰しない。
- 無視されたボタン操作は予約しない。条件が整った後に押し直す。
- 輪は1個を再利用し、復帰後も頭へ追従させない。

## 5. Consoleと検証日時

2026-09-27 JSTの今回の読み取り：実Console Error 0、Warning 0、Compilation Failed=false、コンパイル中ではない。
今回はPlay・実機テストを新規実施していない。過去のPipelineタイムアウトや途中の警告は作業履歴として別文書に残す。今回のConsole状態を実機端末ログの代わりにしない。

## 6. 過去の値からの変更点

|初期の記録|教材で採用する現在状態|
|---|---|
|輪の位置(0,0.03,0.65)、床上から取得|保存時(0,1.25,0.45)、追跡後は視点前方へ配置|
|Use Gravity ON、Force Gravity On Detach OFF|Use Gravity OFF、Force Gravity On Detach ON|
|Grab Transformerを実行時に自動追加|明示登録、自動追加OFF|
|再投球なし／実機テスト待ち|右Aで復帰。基本の再投球ループはQuest 2で本人成功報告|
|独自ランタイムスクリプトなし|配置用と再投球用の2スクリプトあり|

## 7. 教材化に使う範囲

- 01_輪を掴もう：環境・入力・物理輪・前方配置。
- 02_輪を投げよう：追従、解放時速度、重力、実機確認。
- 05_ゲームとして完成させようの一部：右Aによる再投球。章全体が完成したとはしない。

この文書は最新状態の基準記録であり、学習者向け操作手順の完成版ではない。Starter／完成点の固定と再現手順の作成が次の作業。

追記：2026-09-27に[教材初稿と保存点](../Lessons/README.md)を作成。01開始・01完成シーンは保存／参照検証済み、部員再現と新シーン実機確認は未実施。元の完成シーンは変更していない。
現在の完成シーンを出発点なしに「最初から作れる教材」とは扱わない。過去のCreateScene.csなどは作業用で、現行機能を全て再現する学習者向け手順ではない。
