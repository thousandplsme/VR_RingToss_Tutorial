# 02_輪を投げよう

## 目的・開始状態

01で作った「掴んで手に追従する輪」を、離したときに速度と回転を持って飛び、重力で落ちる輪へ変更します。
01を終えた自分のシーンを開き、File > Save Asで`My_02_Throw.unity`などの別名に保存してください。
途中参加者は`Assets/RingTossTutorial/Scenes/Lessons/01_Grab_Complete.unity`を開いて別名保存します。
Unity／XRIの版、入力、輪の形状は変更しません。

## 1. 投擲を有効にする

Ringを選択し、XR Grab Interactableを次のように設定します。

|項目|01の値|02の値|
|---|---|---|
|Throw On Detach|OFF|**ON**|
|Force Gravity On Detach|OFF|**ON**|
|Throw Smoothing Duration|0.25|0.25のまま|
|Throw Velocity Scale|1|1のまま|
|Throw Angular Velocity Scale|1|1のまま|

Throw On Detachは、解放時にXRIが求めた投擲速度・角速度を物理輪へ渡す設定です。倍率1を基準に始めます。
Smoothing Durationは投擲速度の計算で使う過去の動きの期間です。これを「押してから投げるまでの待ち時間」と解釈しないでください。
Force Gravity On Detachは解放後の重力をONにします。**RigidbodyのUse Gravityは保存時OFFのまま**です。開始時・右Aで戻した直後に輪が落ちるのを防ぎます。

## 2. 物理設定を照合して保存する

RigidbodyはMass=0.15、Is Kinematic=OFF、Interpolate、Continuous Dynamic。
XR Grab Interactableは有効、Movement Type=Velocity Tracking、Track Position／Rotation=ON、Select Mode=Single。
01で確認した16個のColliderとXR General Grab Transformerの参照を維持します。Ctrl+Sで保存してください。

## 3. Questで投げる

接続教材に従い、自分の02シーンを起動対象にしてBuild And Run、またはQuest Link＋Unity Playで実行します。どちらを使ったか記録します。

1. 前方の輪へ手を近づけ、Gripで掴む。
2. 手を前に動かし、動かしている途中でGripを離す。
3. 輪が手から離れて進み、回転や落下が生じることを確認する。
4. 離した後で右Aを押して回収し、もう一度掴んで投げる。
5. 左右で試す。最初から大きく振らず、短い投球で比較する。

右Aは配布済みの練習補助です。現在の頭の水平前方45cm・目線より35cm下へ同じ輪を戻します。
戻した直後は速度・角速度0、重力OFF。次に投げると再び重力ONになります。
掴み中・解放直後は回収操作を無視するため、一度Aを離して押し直してください。長押しで連続回収はしません。

## 完成チェック

- [ ] 掴みと保持中の追従を維持できている。
- [ ] 手を動かして離すと輪が飛ぶ。
- [ ] 解放後に落下する。
- [ ] 右Aで戻し、再投擲できる。
- [ ] 自分用02シーンが保存されている。
- [ ] 使用機種、実行方式、成功／失敗を記録した。

左／右、長押し、連打などは実際に試した項目だけチェックしてください。

## 困ったとき・Challenge

離しても保持されたまま：左右のSelect Action TriggerがState Changeか、Gripの解放入力が来ているか確認します。
離れても投げたように飛ばない：Throw On Detach、Is Kinematic、手を止める前に離しているかを確認します。
落下しない：Force Gravity On DetachがONか確認します。保存時のUse GravityだけをONにして済ませないでください。
開始時に床へ落ちる：Use GravityがOFFか、配布スクリプトを外していないか確認します。
Aで戻らない：右手Aか、掴み中でないか、頭が追跡されているかを確認します。実行中にInput Actionsアセットへ別の割り当てを足す前に相談してください。

Challenge：自分用の実験コピーでThrow Velocity Scaleを0.5と1で比較し、投げる感覚の違いを説明する。参考完成点は1なので、最後に戻します。

## 完成状態と検証範囲

参考完成点：`Assets/RingTossTutorial/Scenes/RingToss_v0_1.unity`。
この既存完成点の配置・掴み・投擲・右A再投球はQuest 2のBuild And Run単体アプリで本人成功報告があります。
新しく教材に沿って作った各自のシーンは、別途確認が必要です。Quest Pro／3Sと部員による再現は未確認です。
この時点では得点・的はありません。右A回収のコード解説は「05_ゲームとして完成させよう」に分けます。
