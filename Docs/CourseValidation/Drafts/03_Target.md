**ゴール：棒と土台を作り、輪の穴が棒を通ることを確かめよう。**

前の章：<mention-page url="https://app.notion.com/p/40121a05110d81e78b90db2d33207614"/>

第2章の `Assets/Scenes/02_Throw.unity` から始めます。輪と床のScaleは（1, 1, 1）を基準にします。Floorは第1章で指定した大きさのまま使います。

# STEP 1｜別のシーンとして保存しよう

1. Projectで02_Throwを開き、Playを停止します。
2. **File → Save As**で `Assets/Scenes/03_Target.unity` に保存します。

# STEP 2｜土台を作ろう

1. Hierarchyの空いている場所を右クリック → **Create Empty**。
2. 名前を **Target** にし、Transformを下の値にします。

| 項目 | X | Y | Z |
| --- | --- | --- | --- |
| Position | 0 | 0 | 1.5 |
| Rotation | 0 | 0 | 0 |
| Scale | 1 | 1 | 1 |

3. Targetを右クリック → **3D Object → Cube**。名前を **Base** にします。
4. BaseのTransformを次の値にします。

| 項目 | X | Y | Z |
| --- | --- | --- | --- |
| Position | 0 | 0.05 | 0 |
| Rotation | 0 | 0 | 0 |
| Scale | 0.5 | 0.1 | 0.5 |

**子のPositionは、親のTargetから見た位置です。** BaseのZは1.5ではなく0にします。親を動かすと、土台と棒をまとめて移動できます。

5. Baseの**Box Collider**を残し、Is TriggerはOFFにします。Rigidbodyは追加しません。

# STEP 3｜棒を立てよう

1. HierarchyのTargetを右クリック → **3D Object → Cylinder**。
2. 名前を **Pole** にして、Transformを次の値にします。

| 項目 | X | Y | Z |
| --- | --- | --- | --- |
| Position | 0 | 0.4 | 0 |
| Rotation | 0 | 0 | 0 |
| Scale | 0.06 | 0.3 | 0.06 |

3. Poleの**Capsule Collider**を残し、Is TriggerをOFFにします。Rigidbodyは追加しません。
4. Targetを選択し、Scene画面へマウスを置いてFキーを押します。

**確認：土台の中央に、細い棒が立ちました。**

UnityのCylinderは元の高さが2なので、YのScaleを0.3にすると高さは0.6mです。Capsule Colliderは上下が丸いため、見た目とは先端の形が少し異なります。この教材では標準のColliderで進めます。

Hierarchyは次の形です。

```text
Target
├─ Base
└─ Pole
```

# STEP 4｜輪が棒を通るか確かめよう

まずEditorで、真上から落とした輪が棒を通るか確かめます。

1. Hierarchyの**親のRing**を選びます。
2. Inspectorの**Place Ring At Start**の有効チェックを一時的にOFFにします。
3. RingのPositionを **（0, 1, 1.5）**、Rotationを **（0, 0, 0）** にします。
4. Rigidbodyの**Use GravityをON**にします。Is KinematicはOFFです。
5. Playを押します。SceneタブでTargetを選び、Fキーで見やすくします。
6. 輪が棒を通り、土台の上に落ちるか確認します。
7. Playを停止します。

**できたらOK：棒が輪の穴を通り、輪は土台の上で止まる。**

## 試験用の変更を戻す

1. RingのPositionを **（0, 1.2, 0.5）**、Rotationを **（0, 0, 0）** に戻します。
2. Use Gravityを**OFF**、Place Ring At Startの有効チェックを**ON**に戻します。
3. Ctrl+Sで保存します。

Play中の移動は停止すると戻りますが、**Play前に変えた値は自分で戻します。**

# STEP 5｜Questで的を狙おう

1. File → Build Profiles → AndroidのScene Listで**03_Targetだけを有効**にします。見つからなければAdd Open Scenesで追加します。
2. Run DeviceでQuestを選び、**Build And Run**で `Builds/03_Target.apk` に保存します。
3. 輪を掴み、棒へ向けて小さく投げます。輪が遠くへ行ったらアプリを終了し、起動し直します。

この章では、まだ点数は入りません。**棒にぶつかることと、輪の穴に棒が入ることの違い**を確かめます。

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

**今日の保存点：`Assets/Scenes/03_Target.unity`**

次の章では「輪が入った」をコードで判断します。

