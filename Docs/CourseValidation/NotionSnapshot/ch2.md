<b>ゴール：コントローラーを動かして手を離すと、輪が飛んで床へ落ちるようにしよう。</b>

前の章：<mention url="https://app.notion.com/p/3eb21a05110d81eab68dd57811014617" />

新規作成コースの完成シーンから進めます。Questで「輪を掴む・動かす・離す」ができてから始めましょう。Unity 6000.3.9f1／XR Interaction Toolkit 3.3.1を使います。

この章では設定を変えて動きを比べます。第1章で作ったPlaceRingAtStartはそのまま使います。

# STEP 1｜第2章のシーンを保存しよう

1. Projectで <code>Assets/Scenes/01_Grab_Complete.unity</code> をダブルクリックします。
2. Unity上部のPlayボタンが停止していることを確認します。
3. <b>File → Save As</b>を選び、<code>Assets/Scenes/02_Throw.unity</code> として保存します。

<b>確認：Hierarchy上部のシーン名が02\_Throwになりました。</b> 以後はこのシーンで作業します。

# STEP 2｜離したら飛ぶ設定にしよう

## ① 設定する輪を選ぶ

1. <b>Hierarchyの親のRing</b>をクリックします。Collider\_00などの子は選びません。
2. Inspectorで<b>Rigidbody</b>を開き、次の値を確認します。

| 項目 | 値 |
| --- | --- |
| Mass | 0.15 |
| Use Gravity | OFF |
| Is Kinematic | OFF |
| Interpolate | Interpolate |
| Collision Detection | Continuous Dynamic |

<b>Use GravityをOFFにしておくと、最初は輪を空中で待たせられます。</b>

## ② 手を離すときの設定を変える

1. 同じRingのInspectorを下へスクロールし、<b>XR Grab Interactable</b>を開きます。
2. Movement Typeが<b>Velocity Tracking</b>であることを確認します。
3. <b>Throw On DetachをON</b>にします。ONにすると、その下に投げ方の設定が表示されます。
4. 次の値にします。

| 項目 | 設定 | 意味 |
| --- | --- | --- |
| Throw On Detach | ON | 離すときに、手の動く速さを輪へ伝える |
| Throw Smoothing Duration | 0.25 | 直前の動きを使って、投げる速さを求める範囲（秒） |
| Throw Velocity Scale | 1 | 投げる速さの倍率 |
| Throw Angular Velocity Scale | 1 | 回転する速さの倍率 |
| Force Gravity On Detach | ON | 離した輪に重力を付ける |

Throw Smoothing Curveは変更しません。第1章のColliders、Grab Transformer、Headの登録もそのまま使います。

<b>「Use GravityはOFFなのに、離した輪が落ちる」のは、Force Gravity On Detachが重力をONにするためです。</b> Throw Smoothing Durationは「手を離してから発射するまでの待ち時間」ではありません。

5. Ctrl+Sで保存します。

<b>確認：Throw On DetachとForce Gravity On Detachの両方にチェックが入りました。</b>

# STEP 3｜第2章をQuestへ入れよう

シーンを別名で保存したので、ビルドするシーンも変更します。

1. <b>File → Build Profiles</b>を開き、前の章で使ったAndroidプロファイルを選びます。
2. <b>Scene List</b>を開きます。Global Scene Listを使う設定なら、その一覧を編集します。
3. <b>Add Open Scenes</b>で、今開いている <code>Assets/Scenes/02_Throw.unity</code> を追加します。既にあれば追加しません。
4. <b>02\_ThrowだけをチェックON</b>にし、01\_Grabなど、ほかのシーンはOFFにします。
5. QuestをUSBで接続し、<b>Run Device</b>で接続したQuestを選びます。見つからなければRefreshし、Quest内のUSBデバッグの許可を確認します。
6. <b>Build And Run</b>を押し、<code>Builds/02_Throw.apk</code> として保存します。

<b>確認：Questでアプリが起動し、前を向いて待つと輪が現れます。</b>

# STEP 4｜掴んで投げてみよう

コントローラーのストラップを付け、周囲に人や物がない場所で、小さく動かして試します。

1. 輪の縁にコントローラーを近づけます。
2. <b>Grip（持ち手側のボタン）</b>を押し続けて、輪を掴みます。
3. 手を前へゆっくり動かし、<b>動かしている途中でGripを離します。</b>
4. 輪が前へ進み、その後に床へ落ちるか確認します。コントローラー本体は手放しません。

<b>できたらOK：手から離れた後も輪が進む／重力で落ちる／床を通り抜けない。</b>

手を止めてからGripを離すと、輪があまり飛ばないことがあります。「離す直前の手の動き」が輪へ伝わるためです。

## もう一度試すには

Questのシステムメニューからこのアプリを終了し、同じアプリを起動し直します。システムメニューを閉じて戻るだけでは、起動時の配置処理はやり直されません。アプリを見つけられない場合は、PCからもう一度Build And Runします。

このコースで右Aによる再投球を作るのは、第5章です。今はアプリを再起動して、反対の手でも試しましょう。

# STEP 5｜投げる速さを変えて比べよう

<b>予想：Throw Velocity Scaleを1から0.5にすると、どうなるでしょうか。</b>

1. UnityでPlayを停止し、HierarchyのRingを選びます。
2. XR Grab Interactableの<b>Throw Velocity Scaleだけを0.5</b>にします。
3. Ctrl+Sで保存し、同じ02\_ThrowシーンをBuild And Runします。
4. できるだけ同じ手の動きで投げ、1のときと比べます。

| 設定 | 比べること |
| --- | --- |
| 1 | 基準の速さ |
| 0.5 | 離れた直後の速さと、飛んだ距離 |

0.5は「輪に伝える速度を半分にする」という意味です。手の動きや離す高さも影響するので、飛ぶ距離が必ず半分になるわけではありません。

5. 確認後は<b>Throw Velocity Scaleを1へ戻し、保存して再びBuild And Run</b>します。

<b>確認：最後は倍率1のシーンとアプリが残りました。</b>

# 困ったとき

| 症状 | 確認するところ |
| --- | --- |
| 設定項目が見つからない | Hierarchyの親のRingを選び、XR Grab Interactableを開く。投げる倍率はThrow On DetachをONにすると表示される |
| 掴めなくなった | 第1章の掴む動作を確認。Manager、16個のColliders、Grab Transformerの登録を見直す |
| 離すと真下に落ちる | Throw On DetachがONか、Is KinematicがOFFか。手を動かしている途中でGripを離す |
| 輪が落ちずに進み続ける | Force Gravity On DetachがONか |
| 輪が床を抜ける | 輪のCapsule Colliderと床のBox Colliderが有効か、Is TriggerがOFFか。Collision Detectionも確認 |
| Questで変更が反映されない | 保存したか、Scene Listで02\_Throwだけが有効か。変更後にBuild And Runしたか |

## 完成チェック

- [ ] 左右の手で輪を掴み、離した後に飛ばせた。
- [ ] 投げた輪が床へ落ち、通り抜けなかった。
- [ ] Throw Velocity Scaleの1と0.5を比べた。
- [ ] 倍率を1へ戻して保存し、Questへ入れ直した。
- [ ] 「投げる速さを伝える設定」と「離した後に重力を付ける設定」を説明できる。

<b>今日の保存点：</b><b><code>Assets/Scenes/02_Throw.unity</code></b>

次の章：<mention url="https://app.notion.com/p/40121a05110d819597c5d5ec4370c872" />。輪を通す棒を作ります。
