# 再投球：右Aボタンで輪を戻す

最新の本人確認：Quest 2へBuild And Runで入れた単体アプリで再投球ループ成功（2026-09-27整理時に実行方式も確認）。

2026-09-26 JSTの実装記録。後日追記（2026-09-27整理）：本人からQuest 2で「投げる→右Aで戻す→再び投げる」が問題なく実行できたとの報告あり。長押し・連打などの個別確認とは区別する。最新の結果・設定は [CurrentState.md](CurrentState.md)。以下の検証記述は実装当時の履歴。Notion未反映。

## 操作・仕様

1. 輪を掴んで投げる。
2. 輪を離した後、右コントローラーのAボタンを押す。
3. 現在の頭の水平前方45cm、目線より35cm下へ、同じ輪が戻る。
4. 輪を掴んで再び投げる。

- 頭が未追跡なら操作を無視する。
- 掴んでいる間、および解放フレームとその次のフレームは操作を無視する。無視された操作は予約されず、改めて押す。
- 押し始めだけ処理するため、押し続けても繰り返し戻らない。
- 真上／真下を向き水平前方が決まらない場合は無視する。
- 復帰時は姿勢を水平にし、速度・角速度を0、重力をOFFにする。
- 次の解放では既存XR Grab InteractableのForce Gravity On Detachにより重力がONになる。
- 輪は複製せず1個を再利用。自動回収・UI・得点・的は追加していない。

## 変更ファイルと再現手順

- Assets/RingTossTutorial/RingInitialPlacement.cs：初期配置の計算をTryPlaceInFrontへ共通化。保持中・解放直後の拒否と、復帰時の重力OFFを追加。
- Assets/RingTossTutorial/RingReturnToPlayer.cs（および.meta）：輪専用InputActionを有効／無効化し、右PrimaryButtonの押し始めを検出。
- Assets/RingTossTutorial/Scenes/RingToss_v0_1.unity：RingにRingReturnToPlayerを追加。
- Docs/RingToss-v0.1/ProbeReturn.cs、return-play-results.json：Editorテストと結果。

教材の再現：上記2スクリプトを配置・コンパイルし、RingへRingReturnToPlayerを追加してシーンを保存。RingInitialPlacementのHead=Main Camera、Forward Distance=0.45、Below Eyes=0.35、RigidbodyのUse Gravity=OFF、XR Grab InteractableのForce Gravity On Detach=ONを維持する。

InputActionはこのコンポーネントの実行中だけ所有し、バインドは `<XRController>{RightHand}/{PrimaryButton}`。Questの右Aに対応する基本入力を使用する構成。既存Input Actions／Package設定は編集していない。
既存入力アセットには右PrimaryButtonにJumpのバインドがあるが、実Editorで現シーンに移動・ジャンプを実行するコンポーネントがないことを確認した。将来移動を追加する際は割り当てを再検討する。

## 検証方法と限界

Playで頭・手の追跡を一時無効化し、XRIのManualValue疑似Gripで左右の近接掴み・追従・投擲をテスト。復帰はTryPlaceInFrontを直接呼び、保持中の拒否、解放イベント中の拒否、180度向きを変えた前方配置、速度消去、連続呼び出し、真下を向いた場合の拒否を検証する。
実AボタンのInput Systemバインド解決、長押し、実機のトラッキング喪失はこのテストでは未検証。コード上でWasPressedThisFrameとisTrackedによる制限を実装している。
テスト用の頭・手の変更はPlay停止時に破棄し、保存しない。

## Quest 2でお願いするテスト

最終Editorテスト結果：左右とも全条件passed=true。保持中・解放イベント中の復帰拒否、反対向きの前方配置、速度停止、連続呼び出し、垂直視線の拒否、投擲後重力を確認。実ボタンによる操作は未確認。

終了時：Play停止、実入力参照への復帰を確認。シーン保存成功、isDirty=false、輪の保存位置(0,1.25,0.45)。Consoleの実件数はError 0／Warning 0、Compilation Failed=false。途中のPlay切替では接続リセットが一度あり、実行状態を読み直してからテストを開始した。

- 投げた後に右Aで輪が前方へ戻り、左右それぞれで再取得・再投擲できる。
- 輪を掴んだまま右Aを押しても手元から移動しない。
- Aを押し続けたまま頭を動かしても輪が追いかけてこない。一度離して押し直すと現在の前方に戻る。
- 数回連打しても輪が増えない。少なくとも10回、投げる→戻すを繰り返す。
- 向きを変えてから右Aを押しても、背後でなく前方に戻る。

機種・Linkか単体ビルドか・結果を記録してから実機確認済みへ更新する。
