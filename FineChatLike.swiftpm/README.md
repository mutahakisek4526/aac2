# FineChatLike for Swift Playgrounds (iPad)

## 目的
Swift Playgrounds（iPad）で安定起動を優先した、AACアプリ風のサンプルです。

## 起動手順（iPad）
1. `FineChatLike.swiftpm` フォルダを iCloud Drive などで iPad に配置します。
2. iPad で **Swift Playgrounds** を開きます。
3. 「ファイルを開く」から `FineChatLike.swiftpm` を選択します。
4. Playgrounds 内でアプリを実行します。

## 安定性のために避けたもの
- `FileDocument` / `fileImporter` / `fileExporter` / `UIDocumentPicker`
- CoreData / SwiftData / CloudKit
- 外部ライブラリ依存

## データ保存
- `UserDefaults` にJSON文字列で保存します。
- デコード失敗時はシードデータへフォールバックします。
