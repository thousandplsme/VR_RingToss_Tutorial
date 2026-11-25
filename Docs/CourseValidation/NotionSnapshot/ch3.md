<b>ゴール：棒と土台を作り、輪の穴が棒を通ることを確かめよう。</b>

前の章：<mention url="https://app.notion.com/p/40121a05110d81e78b90db2d33207614" />

第2章の <code>Assets/Scenes/02_Throw.unity</code> から始めます。RingのScaleは（1, 1, 1）にします。FloorのScaleは第1章の（4, 0.1, 4）のまま使います。

# STEP 1｜別のシーンとして保存しよう

1. Projectで02\_Throwを開き、Playを停止します。
2. <b>File → Save As</b>で <code>Assets/Scenes/03_Target.unity</code> に保存します。

# STEP 2｜土台を作ろう

1. Hierarchyの空いている場所を右クリック → <b>Create Empty</b>。
2. 名前を <b>Target</b> にし、Transformを下の値にします。

| 項目 | X | Y | Z |
| --- | --- | --- | --- |
| Position | 0 | 0 | 1.5 |
| Rotation | 0 | 0 | 0 |
| Scale | 1 | 1 | 1 |

3. Targetを右クリック → <b>3D Object → Cube</b>。名前を <b>Base</b> にします。
4. BaseのTransformを次の値にします。

| 項目 | X | Y | Z |
| --- | --- | --- | --- |
| Position | 0 | 0.05 | 0 |
| Rotation | 0 | 0 | 0 |
| Scale | 0.5 | 0.1 | 0.5 |

<b>子のPositionは、親のTargetから見た位置です。</b> BaseのZは1.5ではなく0にします。親を動かすと、土台と棒をまとめて移動できます。

5. Baseの<b>Box Collider</b>を残し、Is TriggerはOFFにします。Rigidbodyは追加しません。

# STEP 3｜棒を立てよう

1. HierarchyのTargetを右クリック → <b>3D Object → Cylinder</b>。
2. 名前を <b>Pole</b> にして、Transformを次の値にします。

| 項目 | X | Y | Z |
| --- | --- | --- | --- |
| Position | 0 | 0.4 | 0 |
| Rotation | 0 | 0 | 0 |
| Scale | 0.06 | 0.3 | 0.06 |

3. Poleの<b>Capsule Collider</b>を残し、Is TriggerをOFFにします。Rigidbodyは追加しません。
4. Targetを選択し、Scene画面へマウスを置いてFキーを押します。

<b>確認：土台の中央に、細い棒が立ちました。</b>

UnityのCylinderは元の高さが2なので、YのScaleを0.3にすると高さは0.6mです。Capsule Colliderは上下が丸いため、見た目とは先端の形が少し異なります。この教材では標準のColliderで進めます。

Hierarchyは次の形です。

```txt
Target
├─ Base
└─ Pole
```

# STEP 4｜輪が棒を通るか確かめよう

まずEditorで、真上から落とした輪が棒を通るか確かめます。

1. Hierarchyの<b>親のRing</b>を選びます。
2. Inspectorの<b>Place Ring At Start</b>の有効チェックを一時的にOFFにします。
3. RingのPositionを <b>（0, 1, 1.5）</b>、Rotationを <b>（0, 0, 0）</b> にします。
4. Rigidbodyの<b>Use GravityをON</b>にします。Is KinematicはOFFです。
5. Playを押します。SceneタブでTargetを選び、Fキーで見やすくします。
6. 輪が棒を通り、土台の上に落ちるか確認します。
7. Playを停止します。

<b>できたらOK：棒が輪の穴を通り、輪は土台の上で止まる。</b>

## 試験用の変更を戻す

1. RingのPositionを <b>（0, 1.2, 0.5）</b>、Rotationを <b>（0, 0, 0）</b> に戻します。
2. Use Gravityを<b>OFF</b>、Place Ring At Startの有効チェックを<b>ON</b>に戻します。
3. Ctrl+Sで保存します。

Play中の移動は停止すると戻りますが、<b>Play前に変えた値は自分で戻します。</b>

# STEP 5｜Questで的を狙おう

1. File → Build Profiles → AndroidのScene Listで<b>03\_Targetだけを有効</b>にします。見つからなければAdd Open Scenesで追加します。
2. Run DeviceでQuestを選び、<b>Build And Run</b>で <code>Builds/03_Target.apk</code> に保存します。
3. 輪を掴み、棒へ向けて小さく投げます。輪が遠くへ行ったらアプリを終了し、起動し直します。

この章では、まだ点数は入りません。<b>棒にぶつかることと、輪の穴に棒が入ることの違い</b>を確かめます。

# 困ったとき

| 症状 | 確認するところ |
| --- | --- |
| 棒が土台から離れている | PoleがTargetの子か。PoleのPositionは（0, 0.4, 0）か |
| 的が遠くにずれる | Base・PoleのZは0。TargetだけZ＝1.5 |
| 輪が棒を通れない | RingのScale＝1、PoleのScale＝（0.06, 0.3, 0.06）か。輪の中央をふさぐColliderを追加していないか |
| 棒や土台を通り抜ける | Colliderが有効で、Is TriggerがOFFか |
| Questで輪が落下した状態から始まる | 保存前にUse GravityをOFFへ戻したか |
| 輪が顔の前に現れない | Place Ring At StartをONへ戻したか。Headの参照があるか |

## 余裕があれば

TargetのPositionのZだけを1.5から1.8へ変えて、狙いやすさを比べます。確認後は1.5に戻して保存・再ビルドします。

## 完成チェック

- [ ] Targetの下にBaseとPoleを作れた。
- [ ] Editorで輪を真上から落とし、棒を通せた。
- [ ] 試験用に変えた輪の位置・重力・スクリプトを戻した。
- [ ] Questで的が見え、輪との衝突を確認した。

<b>今日の保存点：</b><b><code>Assets/Scenes/03_Target.unity</code></b>

次の章：<mention url="https://app.notion.com/p/40121a05110d813f86d4f2bb40ab7045" />。「輪が入った」をコードで判断します。
