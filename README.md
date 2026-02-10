# AacV2

.NET 8 / WPF（`net8.0-windows`）で作成した AAC 完成アプリです。  
視線入力は「マウスカーソル＝視線」方式を採用し、全ボタンに Dwell（滞留クリック）を適用しています。

## 必要環境
- Visual Studio 2022
- .NET 8 SDK
- Windows 10/11

## ビルド手順（Visual Studio 2022）
1. `AacV2.sln` を開く
2. 構成を `Debug | Any CPU` にする
3. `Rebuild Solution` を実行
4. エラー 0 を確認

## 起動手順
1. スタートアッププロジェクトを `AacV2` に設定
2. `F5` または `デバッグ開始` を実行
3. メイン画面（左メニュー＋右コンテンツ）が表示されることを確認

## 機能概要
- ホーム
- 文字盤（50音グループ、確定、読み上げ、予測）
- フレーズ（追加・削除・保存）
- 履歴（再入力）
- 予測
- PC操作（SendInput）
- 環境制御（PCコマンド / URL）
- 設定（InputModeKind / DwellTimeMs / FontScale / ButtonScale 即時保存）
- 支援者モード（説明、履歴・学習語の初期化）

## 保存先
- `%AppData%/AacV2/`
- JSON: `history.json`, `phrases.json`, `learnedWords.json`, `environmentActions.json`, `settings.json`

## Rebuild 成功チェックリスト
- [ ] `AacV2.csproj` の `TargetFramework` が `net8.0-windows`
- [ ] `UseWPF=true`
- [ ] `EnableWindowsTargeting=true`
- [ ] `InputMode` 型名を未使用（`InputModeKind` を利用）
- [ ] `System.IO` 利用箇所で未定義エラーなし
- [ ] `SendInput` の `virtual key=ushort / flags=uint`
- [ ] `IPredictionService` の 3 引数 `GetPredictions` を全呼び出しで遵守
- [ ] `IStorageService` に `LoadEnvironmentActions / SaveEnvironmentActions` を実装済み
