# YamaPlayer Queue Manager

YamaPlayerのイベントを利用してVRCUrlをランタイム中にキュートに追加するUdonSharpスクリプトです。

## 概要

このスクリプトは、YamaPlayerのプレイヤーイベント（動画終了、エラー、停止など）を利用して、事前に定義されたVRCUrlを自動的にキュートに追加する機能を提供します。

## 含まれるスクリプト

### 1. YamaPlayerQueueManager.cs
基本的なキュート管理機能を提供するスクリプトです。

**主な機能:**
- 事前定義されたURLの自動追加
- プレイヤーイベントに基づく自動キュート追加
- キュートの手動管理
- キュート情報の表示

### 2. YamaPlayerDynamicQueueManager.cs
高度なキュート管理機能を提供するスクリプトです。

**主な機能:**
- ランタイムでの動的URL追加
- ネットワーク同期対応
- シャッフル再生
- ループ再生
- 自動再生設定
- キュート状態の定期チェック

## セットアップ方法

### 1. 基本的なセットアップ

1. **YamaPlayerQueueManager**をGameObjectに追加
2. **Controller**フィールドにYamaPlayerのControllerを割り当て
3. 必要に応じて**Predefined URLs**にURLを設定
4. **Use Predefined URLs**を有効にする

### 2. 高度なセットアップ

1. **YamaPlayerDynamicQueueManager**をGameObjectに追加
2. **Controller**フィールドにYamaPlayerのControllerを割り当て
3. 各種設定を調整

## 設定項目

### YamaPlayerQueueManager

| 項目 | 説明 |
|------|------|
| Controller | YamaPlayerのControllerコンポーネント |
| Auto Add To Queue | 自動キュート追加の有効/無効 |
| Notify On Add | 追加時の通知表示 |
| Predefined URLs | 事前定義されたURL配列 |
| Use Predefined URLs | 事前定義URLの使用有効/無効 |
| Add On Video End | 動画終了時の追加 |
| Add On Video Error | 動画エラー時の追加 |
| Add On Video Stop | 動画停止時の追加 |

### YamaPlayerDynamicQueueManager

| 項目 | 説明 |
|------|------|
| Controller | YamaPlayerのControllerコンポーネント |
| Dynamic URLs | 動的URL配列（ネットワーク同期） |
| Max Dynamic URLs | 最大動的URL数 |
| Auto Play Next | 自動次曲再生 |
| Loop Queue | キュートループ |
| Shuffle Queue | シャッフル再生 |
| Add On Video End | 動画終了時の追加 |
| Add On Video Error | 動画エラー時の追加 |
| Add On Video Stop | 動画停止時の追加 |
| Enable Notifications | 通知表示 |
| Queue Check Interval | キュートチェック間隔 |
| Enable Queue Validation | キュート検証有効/無効 |

## 使用方法

### 基本的な使用方法

1. **自動追加**: プレイヤーイベントに基づいて自動的にURLがキュートに追加されます
2. **手動追加**: `AddUrlToQueue(VRCUrl url)`メソッドを使用して手動でURLを追加
3. **キュート管理**: `ClearQueue()`、`ShowQueueInfo()`メソッドでキュートを管理

### 高度な使用方法

1. **動的URL追加**: `AddDynamicUrl(VRCUrl url)`でランタイム中にURLを追加
2. **一括追加**: `AddDynamicUrls(VRCUrl[] urls)`で複数のURLを一括追加
3. **URL削除**: `RemoveDynamicUrl(int index)`で特定のURLを削除
4. **ランダム追加**: `AddRandomDynamicUrlToQueue()`でランダムにURLを追加

## イベントハンドラー

### 利用可能なイベント

- `OnVideoEnd()`: 動画終了時
- `OnVideoError(VideoError videoError)`: 動画エラー時
- `OnVideoStop()`: 動画停止時
- `OnQueueUpdated()`: キュート更新時
- `OnTrackUpdated()`: トラック更新時

### イベントのカスタマイズ

各イベントハンドラーは、設定項目に基づいて動作します。必要に応じて、スクリプトを編集してカスタムロジックを追加できます。

## 注意事項

1. **ネットワーク同期**: YamaPlayerDynamicQueueManagerは`BehaviourSyncMode.Manual`を使用しているため、URLの追加/削除はオーナーのみが実行できます
2. **URL検証**: 追加されるURLは自動的に検証されます
3. **パフォーマンス**: 大量のURLを追加する場合は、`Max Dynamic URLs`を適切に設定してください
4. **エラーハンドリング**: 無効なURLは自動的にスキップされ、エラーログに記録されます

## サンプルコード

### 基本的なURL追加
```csharp
// YamaPlayerQueueManagerのインスタンスを取得
YamaPlayerQueueManager queueManager = GetComponent<YamaPlayerQueueManager>();

// URLを追加
VRCUrl url = new VRCUrl("https://example.com/video.mp4");
queueManager.AddUrlToQueue(url);
```

### 動的URL追加
```csharp
// YamaPlayerDynamicQueueManagerのインスタンスを取得
YamaPlayerDynamicQueueManager dynamicQueueManager = GetComponent<YamaPlayerDynamicQueueManager>();

// 動的URLを追加
VRCUrl url = new VRCUrl("https://example.com/video.mp4");
dynamicQueueManager.AddDynamicUrl(url);
```

## トラブルシューティング

### よくある問題

1. **Controllerが割り当てられていない**
   - エラー: "Controller is not assigned!"
   - 解決: ControllerフィールドにYamaPlayerのControllerを割り当ててください

2. **URLが追加されない**
   - 原因: 無効なURLまたはネットワーク権限の問題
   - 解決: URLの形式を確認し、オーナー権限があることを確認してください

3. **イベントが発火しない**
   - 原因: Controllerにリスナーとして登録されていない
   - 解決: Start()メソッドで`_controller.AddListener(this)`が実行されていることを確認してください

## ライセンス

このスクリプトはYamaPlayerプロジェクトの一部として提供されています。