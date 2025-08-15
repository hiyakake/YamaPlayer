# YamaPlayer Integration Scripts

YamaStreamと連携して動画を同期再生するためのUDONスクリプトです。

## 概要

このスクリプトは、VRChat内でYamaPlayerと連携して動画を再生するための統合スクリプトです。インタラクト可能なオブジェクトにアタッチすることで、簡単に動画再生機能を追加できます。

## ファイル構成

### 1. YamaPlayerIntegration.cs
基本的な動画再生機能を提供するシンプルなスクリプトです。

### 2. YamaPlayerAdvancedIntegration.cs
高度な機能を持つ拡張版スクリプトです。複数動画、プレイリスト、イベントリスナー機能をサポートします。

## 基本的な使用方法

### セットアップ

1. **YamaPlayerControllerの設定**
   - YamaPlayerのControllerコンポーネントを`_yamaPlayerController`フィールドに設定
   - 権限システムが有効になっていることを確認

2. **動画URLの設定**
   - `_videoUrl`フィールドに再生したい動画のURLを設定
   - サポート形式: YouTube, ニコニコ動画, その他のVRChat対応動画URL

3. **権限設定**
   - `_requirePermission`: 権限チェックの有効/無効
   - `_minimumPermission`: 必要な最小権限レベル

### 基本的な動作

```csharp
// インタラクト時に実行される処理
public override void Interact()
{
    // 1. 権限チェック
    if (!CheckPermission()) return;
    
    // 2. 所有権取得
    _yamaPlayerController.TakeOwnership();
    
    // 3. Trackオブジェクト作成
    Track track = Track.New(_videoPlayerType, _videoTitle, _videoUrl);
    
    // 4. 動画再生
    _yamaPlayerController.PlayTrack(track);
}
```

## 高度な機能 (YamaPlayerAdvancedIntegration)

### インタラクションモード

1. **SingleVideo**: 単一動画を再生
2. **MultipleVideos**: 複数動画を順次再生
3. **Playlist**: プレイリストを読み込み
4. **RandomVideo**: ランダムに動画を選択して再生

### VideoConfig設定

```csharp
[System.Serializable]
public class VideoConfig
{
    public string name = "動画名";
    public VRCUrl url = VRCUrl.Empty;
    public string title = "動画タイトル";
    public VideoPlayerType playerType = VideoPlayerType.AVProVideoPlayer;
    public bool addToQueue = false;
    public bool autoPlay = true;
    public string successMessage = "動画を再生しました";
    public string errorMessage = "再生に失敗しました";
}
```

### イベントリスナー

他のUDONスクリプトをイベントリスナーとして登録できます：

```csharp
// イベントリスナーに通知されるイベント
public void OnVideoPlaySuccess() { }
public void OnVideoPlayError() { }
public void OnPlaylistLoadSuccess() { }
public void OnPlaylistLoadError() { }
```

## 設定項目

### YamaPlayerIntegration

| 項目 | 説明 |
|------|------|
| `_yamaPlayerController` | YamaPlayerのControllerコンポーネント |
| `_videoPlayerType` | 使用するビデオプレイヤータイプ |
| `_videoUrl` | 再生する動画のURL |
| `_videoTitle` | 動画のタイトル |
| `_addToQueue` | キューに追加するかどうか |
| `_requirePermission` | 権限チェックの有効/無効 |
| `_minimumPermission` | 必要な最小権限レベル |

### YamaPlayerAdvancedIntegration

| 項目 | 説明 |
|------|------|
| `_videoConfigs` | 動画設定の配列 |
| `_interactionMode` | インタラクションモード |
| `_cycleThroughVideos` | 動画を順次切り替えるかどうか |
| `_interactionCooldown` | インタラクション間のクールダウン時間 |
| `_eventListeners` | イベントリスナーの配列 |

## 使用例

### 例1: 基本的な動画再生ボタン

```csharp
// YamaPlayerIntegrationを使用
// 1. オブジェクトにYamaPlayerIntegrationをアタッチ
// 2. YamaPlayerControllerを設定
// 3. 動画URLを設定
// 4. インタラクト可能にする
```

### 例2: 複数動画の順次再生

```csharp
// YamaPlayerAdvancedIntegrationを使用
// 1. VideoConfig配列に複数の動画を設定
// 2. InteractionModeをMultipleVideosに設定
// 3. CycleThroughVideosを有効にする
```

### 例3: プレイリスト再生

```csharp
// YamaPlayerAdvancedIntegrationを使用
// 1. InteractionModeをPlaylistに設定
// 2. プレイリストURLを設定
// 3. インタラクトでプレイリストを読み込み
```

## トラブルシューティング

### よくある問題

1. **動画が再生されない**
   - YamaPlayerControllerが正しく設定されているか確認
   - 権限が不足していないか確認
   - URLが有効か確認

2. **同期されない**
   - TakeOwnership()が呼ばれているか確認
   - RequestSerialization()が呼ばれているか確認
   - ネットワーク設定を確認

3. **エラーが発生する**
   - デバッグログを有効にして詳細を確認
   - 例外処理が適切に実装されているか確認

### デバッグ

```csharp
// デバッグログを有効にする
_enableDebugLogs = true;

// ログ出力例
PrintLog("動画再生開始");
PrintError("エラーが発生しました");
```

## 注意事項

1. **権限管理**: 適切な権限設定を行い、不正な操作を防ぐ
2. **ネットワーク同期**: 所有権とシリアライゼーションを適切に処理
3. **エラーハンドリング**: 例外処理を実装して安定性を確保
4. **パフォーマンス**: 大量の動画設定は避け、適切なクールダウンを設定

## ライセンス

このスクリプトはYamaStreamプロジェクトの一部として提供されています。