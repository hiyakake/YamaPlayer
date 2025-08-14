# プレイリスト表示制御機能

この機能により、ランタイム中にプレイリスト一覧UIに表示されるプレイリストを動的に制御できます。

## 機能概要

- 特定の名前のプレイリストを非表示/表示に切り替え
- インデックス指定によるプレイリストの表示制御
- すべてのプレイリストの一括表示/非表示
- イベントシステムによる動的な制御

## 使用方法

### 1. PlaylistVisibilityControllerコンポーネントの追加

1. シーン内で右クリック
2. `GameObject > YamaPlayer > Playlist Visibility Controller` を選択
3. 作成されたGameObjectにControllerを設定

### 2. 基本的な使用方法

#### 名前指定による制御

```csharp
// 特定のプレイリストを非表示にする
playlistVisibilityController.HidePlaylist("My Playlist");

// 特定のプレイリストを表示する
playlistVisibilityController.ShowPlaylist("My Playlist");

// 特定のプレイリストの表示状態を切り替える
playlistVisibilityController.TogglePlaylist("My Playlist");
```

#### インデックス指定による制御

```csharp
// インデックス0のプレイリストを非表示にする
playlistVisibilityController.HidePlaylistByIndex(0);

// インデックス1のプレイリストを表示する
playlistVisibilityController.ShowPlaylistByIndex(1);

// インデックス2のプレイリストの表示状態を切り替える
playlistVisibilityController.TogglePlaylistByIndex(2);
```

#### 一括制御

```csharp
// すべてのプレイリストを非表示にする
playlistVisibilityController.HideAllPlaylists();

// すべてのプレイリストを表示する
playlistVisibilityController.ShowAllPlaylists();
```

### 3. イベントシステムの使用

#### UdonSharpイベントの設定

1. PlaylistVisibilityControllerコンポーネントの `Enable Custom Events` を有効にする
2. 各イベント名を設定（デフォルト値が提供されています）
3. 他のUdonSharpコンポーネントからイベントを呼び出し

#### イベント呼び出し例

```csharp
// カスタムイベント名でプレイリストを非表示にする
playlistVisibilityController.CustomHidePlaylist();

// カスタムイベント名でプレイリストを表示する
playlistVisibilityController.CustomShowPlaylist();

// カスタムイベント名でプレイリストの表示状態を切り替える
playlistVisibilityController.CustomTogglePlaylist();
```

### 4. 直接Controllerからの制御

```csharp
// Controllerコンポーネントから直接制御
controller.HidePlaylist("Playlist Name");
controller.ShowPlaylist("Playlist Name");
controller.SetPlaylistVisibility("Playlist Name", false);
controller.HideAllPlaylists();
controller.ShowAllPlaylists();
```

## 設定項目

### PlaylistVisibilityController

- **Controller**: YamaPlayerのControllerコンポーネント
- **Target Playlist Name**: 制御対象のプレイリスト名（名前指定の場合）
- **Target Playlist Index**: 制御対象のプレイリストインデックス（インデックス指定の場合）
- **Enable Custom Events**: カスタムイベント機能の有効/無効
- **Hide Playlist Event Name**: 非表示イベントの名前
- **Show Playlist Event Name**: 表示イベントの名前
- **Toggle Playlist Event Name**: 切り替えイベントの名前
- **Hide All Playlists Event Name**: 全非表示イベントの名前
- **Show All Playlists Event Name**: 全表示イベントの名前

## 使用例

### 例1: 特定のプレイリストを一時的に非表示にする

```csharp
// プレイリストを非表示にする
playlistVisibilityController.HidePlaylist("Private Playlist");

// 後で再表示する
playlistVisibilityController.ShowPlaylist("Private Playlist");
```

### 例2: プレイリストの表示状態を切り替える

```csharp
// ボタンクリック時にプレイリストの表示/非表示を切り替え
public void OnTogglePlaylistButton()
{
    playlistVisibilityController.TogglePlaylist("My Playlist");
}
```

### 例3: すべてのプレイリストを非表示にしてから特定のものだけ表示

```csharp
public void ShowOnlySpecificPlaylist()
{
    // すべてを非表示にする
    playlistVisibilityController.HideAllPlaylists();
    
    // 特定のプレイリストのみ表示する
    playlistVisibilityController.ShowPlaylist("Main Playlist");
}
```

## 注意事項

- プレイリストの表示状態は同期されます（UdonSynced）
- 非表示にしたプレイリストは、プレイリスト一覧UIに表示されませんが、実際のプレイリストデータは保持されます
- 動的プレイリスト（YouTubeプレイリストなど）も制御可能です
- キューと履歴は表示制御の対象外です

## トラブルシューティング

### プレイリストが制御されない場合

1. Controllerが正しく設定されているか確認
2. プレイリスト名が正確に一致しているか確認
3. プレイリストインデックスが有効な範囲内か確認

### イベントが動作しない場合

1. Enable Custom Eventsが有効になっているか確認
2. イベント名が正しく設定されているか確認
3. 呼び出し元のUdonSharpコンポーネントが正しく設定されているか確認