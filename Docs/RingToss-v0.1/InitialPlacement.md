# 初期配置修正 — 2026-09-26 JST

状態：実装・保存済み。その後、ユーザーからQuest 2で配置・掴み・投擲が問題なく可能との報告あり。詳細と未確認範囲は [Quest2UserResult.md](Quest2UserResult.md)。Notion未反映。

## 調査

実Editorの保存状態ではXR Originは位置・回転0、Main Cameraは(0,1.6,0)、forwardは+Z、Ringは(0,0.03,0.65)。この保存姿勢に対しては前方だった。
従来は輪をワールド座標へ固定しており、起動時の実HMDの位置・向きを反映していなかった。実機のトラッキング原点や向きとの差が後方に見える原因候補。実機姿勢を取得していないため、具体的な向きのずれは未確定。

## 変更

- Assets/RingTossTutorial/RingInitialPlacement.cs（および.meta）追加。
- Assets/RingTossTutorial/Scenes/RingToss_v0_1.unityのRingへ上記を追加し、headにMain Cameraを指定。
- XRNode.HeadのisTrackedが連続0.25秒有効になった後、水平前方0.45m、目線から下0.35mへ一度だけ配置。上下を向きすぎて水平前方を決められない場合は待つ。
- 掴み済みなら配置しない。配置後に顔を動かしても輪を追従させず、リセット機能も追加しない。
- 保存時の仮位置は(0,1.25,0.45)。頭が未追跡ならこの位置を保つ。
- Ring RigidbodyのUse GravityをOFFへ。初回取得前の落下を防ぐ。
- XR Grab InteractableのForce Gravity On DetachをONへ。解放後は重力が働く構成に変更。
- パッケージ・Input Actions・他のシーンは編集していない。

再現：RingへRingInitialPlacementを追加し、Head=Main Camera、Forward Distance=0.45、Below Eyes=0.35を指定。RigidbodyのUse GravityをOFF、XR Grab InteractableのForce Gravity On DetachをONにし、教材シーンを保存する。

## 確認範囲

Editorでスクリプトのコンパイル完了、Main Camera参照、0.45／0.35の設定値、重力設定、保存済み（isDirty=false）を確認。
Consoleには今回のPipeline呼び出しタイムアウトが記録された。変更処理のタイムアウト後は状態を読み直し、重複実行せず、save_sceneで保存成功を確認した。
今回の新しい配置処理と重力設定についてPlay／Quest実機の動作確認は未実施。以前の疑似投擲テスト結果を今回の修正に対する動作確認として扱わない。

Quest 2でユーザーに確認いただく項目：起動時に輪が前方へ出ること、手が届く高さ・距離であること、左右Gripで掴めること、解放後に投げられて落下すること。結果を受け取るまでNotionへ「確認済み」と記載しない。
