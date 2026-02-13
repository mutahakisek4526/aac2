import SwiftUI

struct RootTabView: View {
    @EnvironmentObject private var store: AppStore

    var body: some View {
        TabView {
            ConversationView()
                .tabItem { Label("会話", systemImage: "message") }
            KanaBoardView()
                .tabItem { Label("文字盤", systemImage: "square.grid.3x3") }
            PhraseView()
                .tabItem { Label("定型文", systemImage: "text.bubble") }
            SettingsView()
                .tabItem { Label("設定", systemImage: "gearshape") }
        }
        .preferredColorScheme(store.state.settings.highContrast ? .dark : nil)
    }
}

struct ConversationView: View {
    @EnvironmentObject private var store: AppStore
    @Environment(\.openURL) private var openURL
    @State private var lineStatusMessage: String = ""

    var body: some View {
        ScrollView {
            VStack(alignment: .leading, spacing: 16) {
                Text("入力")
                    .font(.headline)
                TextEditor(text: $store.inputText)
                    .frame(minHeight: 220)
                    .padding(8)
                    .background(Color(uiColor: .secondarySystemBackground))
                    .cornerRadius(10)

                ScrollView(.horizontal, showsIndicators: false) {
                    HStack {
                        ActionButton(title: "発話", color: .blue) { store.speakCurrent() }
                        ActionButton(title: "停止", color: .orange) { store.stopSpeak() }
                        ActionButton(title: "Undo", color: .gray) { store.undoInput() }
                        ActionButton(title: "削除", color: .pink) { store.deleteLastWord() }
                        ActionButton(title: "クリア", color: .red) { store.clearInput() }
                        ActionButton(title: "line送信", color: .green) {
                            store.copyInputToClipboard()
                            guard let url = store.lineSendURL() else {
                                lineStatusMessage = "送信する入力がありません"
                                return
                            }
                            openURL(url) { accepted in
                                lineStatusMessage = accepted ? "LINEを開きました（入力をクリップボードへコピー済み）" : "LINEを開けませんでした"
                            }
                        }
                    }
                }

                if !lineStatusMessage.isEmpty {
                    Text(lineStatusMessage)
                        .font(.footnote)
                        .foregroundColor(.secondary)
                }

                Text("予測")
                    .font(.headline)
                LazyVGrid(columns: [GridItem(.adaptive(minimum: 150), spacing: 8)], spacing: 8) {
                    ForEach(store.predictions(), id: \.self) { item in
                        Button(item) {
                            store.inputText = item
                        }
                        .buttonStyle(.bordered)
                    }
                }

                Text("履歴")
                    .font(.headline)
                VStack(alignment: .leading, spacing: 8) {
                    ForEach(store.state.history, id: \.self) { item in
                        HStack {
                            Button(item) { store.applyHistory(item, speakNow: false) }
                                .buttonStyle(.bordered)
                            Button("再発話") { store.applyHistory(item, speakNow: true) }
                                .buttonStyle(.borderedProminent)
                        }
                    }
                }
            }
            .padding()
            .dynamicTypeSize(.large ... .accessibility3)
            .environment(\.sizeCategory, sizeCategoryFromScale(store.state.settings.textScale))
        }
    }
}

struct KanaBoardView: View {
    @EnvironmentObject private var store: AppStore
    @State private var mode: BoardMode = .gojuon

    var body: some View {
        let keys = mode.keys
        ScrollView {
            VStack(spacing: 14) {
                Picker("種別", selection: $mode) {
                    ForEach(BoardMode.allCases, id: \.self) { m in
                        Text(m.title).tag(m)
                    }
                }
                .pickerStyle(.segmented)

                LazyVGrid(columns: Array(repeating: GridItem(.flexible(), spacing: 8), count: 5), spacing: 8) {
                    ForEach(keys, id: \.self) { key in
                        Button(key) { store.insertText(key) }
                            .frame(maxWidth: .infinity, minHeight: 54)
                            .background(Color(uiColor: .secondarySystemBackground))
                            .cornerRadius(10)
                    }
                }

                HStack {
                    ActionButton(title: "空白", color: .gray) { store.insertText(" ") }
                    ActionButton(title: "改行", color: .gray) { store.insertText("\n") }
                    ActionButton(title: "削除", color: .red) { store.undoInput() }
                }
            }
            .padding()
        }
    }
}

enum BoardMode: CaseIterable {
    case gojuon, dakuon, youon, symbols

    var title: String {
        switch self {
        case .gojuon: return "50音"
        case .dakuon: return "濁音"
        case .youon: return "拗音"
        case .symbols: return "記号"
        }
    }

    var keys: [String] {
        switch self {
        case .gojuon:
            return "あ い う え お か き く け こ さ し す せ そ た ち つ て と な に ぬ ね の は ひ ふ へ ほ ま み む め も や ゆ よ ら り る れ ろ わ を ん".split(separator: " ").map(String.init)
        case .dakuon:
            return "が ぎ ぐ げ ご ざ じ ず ぜ ぞ だ ぢ づ で ど ば び ぶ べ ぼ ぱ ぴ ぷ ぺ ぽ".split(separator: " ").map(String.init)
        case .youon:
            return "きゃ きゅ きょ しゃ しゅ しょ ちゃ ちゅ ちょ にゃ にゅ にょ ひゃ ひゅ ひょ みゃ みゅ みょ りゃ りゅ りょ ぎゃ ぎゅ ぎょ じゃ じゅ じょ".split(separator: " ").map(String.init)
        case .symbols:
            return ["。", "、", "?", "!", "ー", "〜", "（", "）", "・", "…", "♪", "♡", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0"]
        }
    }
}

struct PhraseView: View {
    @EnvironmentObject private var store: AppStore
    @State private var selectedCategoryID: UUID?
    @State private var newCategoryName: String = ""
    @State private var newPhraseText: String = ""
    @State private var editingPhraseID: UUID?
    @State private var editingText: String = ""

    var body: some View {
        ScrollView {
            VStack(alignment: .leading, spacing: 16) {
                Text("お気に入り")
                    .font(.headline)
                ScrollView(.horizontal, showsIndicators: false) {
                    HStack {
                        ForEach(store.favorites) { phrase in
                            Button(phrase.text) { store.insertText(phrase.text) }
                                .buttonStyle(.borderedProminent)
                        }
                    }
                }

                categoryMenu

                VStack(alignment: .leading, spacing: 8) {
                    ForEach(activePhrases) { phrase in
                        phraseRow(phrase)
                    }
                }

                Divider()

                Text("カテゴリ追加")
                HStack {
                    TextField("例: 症状", text: $newCategoryName)
                        .textFieldStyle(.roundedBorder)
                    Button("追加") {
                        store.addCategory(name: newCategoryName)
                        newCategoryName = ""
                        if selectedCategoryID == nil {
                            selectedCategoryID = store.state.categories.first?.id
                        }
                    }
                }

                Text("定型文追加")
                HStack {
                    TextField("定型文", text: $newPhraseText)
                        .textFieldStyle(.roundedBorder)
                    Button("追加") {
                        if let id = selectedCategoryID ?? store.state.categories.first?.id {
                            store.addPhrase(categoryID: id, text: newPhraseText)
                            newPhraseText = ""
                        }
                    }
                }
            }
            .padding()
            .onAppear {
                if selectedCategoryID == nil {
                    selectedCategoryID = store.state.categories.first?.id
                }
            }
        }
    }

    private var categoryMenu: some View {
        HStack {
            Text("カテゴリ")
            Spacer()
            Menu {
                ForEach(store.state.categories) { category in
                    Button(category.name) {
                        selectedCategoryID = category.id
                    }
                }
            } label: {
                Text(currentCategoryName)
                    .padding(8)
                    .background(Color(uiColor: .secondarySystemBackground))
                    .cornerRadius(8)
            }
        }
    }

    private var activePhrases: [PhraseItem] {
        guard let selectedCategoryID else { return [] }
        return store.phrases(in: selectedCategoryID)
    }

    private var currentCategoryName: String {
        if let id = selectedCategoryID,
           let name = store.state.categories.first(where: { $0.id == id })?.name {
            return name
        }
        return "未選択"
    }

    @ViewBuilder
    private func phraseRow(_ phrase: PhraseItem) -> some View {
        VStack(alignment: .leading, spacing: 6) {
            if editingPhraseID == phrase.id {
                HStack {
                    TextField("編集", text: $editingText)
                        .textFieldStyle(.roundedBorder)
                    Button("保存") {
                        store.updatePhrase(phrase, text: editingText)
                        editingPhraseID = nil
                        editingText = ""
                    }
                    Button("取消") {
                        editingPhraseID = nil
                        editingText = ""
                    }
                }
            } else {
                HStack {
                    Text(phrase.text)
                    Spacer()
                    Button("挿入") { store.insertText(phrase.text) }
                    Button("発話") { store.speak(text: phrase.text) }
                    Button(phrase.isFavorite ? "★" : "☆") { store.toggleFavorite(phrase) }
                    Button("編集") {
                        editingPhraseID = phrase.id
                        editingText = phrase.text
                    }
                    Button("削除", role: .destructive) { store.deletePhrase(phrase) }
                }
            }
        }
        .padding(8)
        .background(Color(uiColor: .secondarySystemBackground))
        .cornerRadius(8)
    }
}

struct SettingsView: View {
    @EnvironmentObject private var store: AppStore
    @State private var unlockPIN: String = ""
    @State private var newPIN: String = ""
    @State private var isUnlocked = false
    @State private var pinError = ""

    var body: some View {
        ScrollView {
            VStack(alignment: .leading, spacing: 16) {
                Text("介助者PINロック")
                    .font(.headline)

                if !isUnlocked {
                    HStack {
                        TextField("4桁PIN", text: $unlockPIN)
                            .keyboardType(.numberPad)
                            .textFieldStyle(.roundedBorder)
                        Button("解除") {
                            if unlockPIN == store.state.settings.caregiverPIN {
                                isUnlocked = true
                                pinError = ""
                            } else {
                                pinError = "PINが違います"
                            }
                        }
                    }
                    if !pinError.isEmpty {
                        Text(pinError).foregroundColor(.red)
                    }
                } else {
                    Text("設定変更が可能です")

                    VStack(alignment: .leading) {
                        Text("文字サイズ")
                        Slider(value: Binding(
                            get: { store.state.settings.textScale },
                            set: { newValue in store.updateSettings { $0.textScale = newValue } }
                        ), in: 0.8...1.8)
                    }

                    Toggle("ハイコントラスト", isOn: Binding(
                        get: { store.state.settings.highContrast },
                        set: { newValue in store.updateSettings { $0.highContrast = newValue } }
                    ))

                    VStack(alignment: .leading) {
                        Text("履歴件数: \(store.state.settings.historyLimit)")
                        Slider(value: Binding(
                            get: { Double(store.state.settings.historyLimit) },
                            set: { newValue in store.updateSettings { $0.historyLimit = Int(newValue.rounded()) } }
                        ), in: 5...50, step: 1)
                    }

                    VStack(alignment: .leading) {
                        Text("音声 rate")
                        Slider(value: Binding(
                            get: { store.state.settings.speechRate },
                            set: { newValue in store.updateSettings { $0.speechRate = newValue } }
                        ), in: 0.3...0.65)
                        Text("音声 pitch")
                        Slider(value: Binding(
                            get: { store.state.settings.speechPitch },
                            set: { newValue in store.updateSettings { $0.speechPitch = newValue } }
                        ), in: 0.5...1.8)
                        Button("テスト発話") { store.speak(text: "設定テストです") }
                            .buttonStyle(.borderedProminent)
                    }

                    Divider()
                    Text("PIN変更")
                    HStack {
                        TextField("新しい4桁PIN", text: $newPIN)
                            .keyboardType(.numberPad)
                            .textFieldStyle(.roundedBorder)
                        Button("変更") {
                            if store.changePIN(newPIN: newPIN) {
                                pinError = ""
                                newPIN = ""
                            } else {
                                pinError = "PINは数字4桁のみ"
                            }
                        }
                    }
                    if !pinError.isEmpty {
                        Text(pinError).foregroundColor(.red)
                    }
                }
            }
            .padding()
        }
    }
}

struct ActionButton: View {
    let title: String
    let color: Color
    let action: () -> Void

    var body: some View {
        Button(title, action: action)
            .buttonStyle(.borderedProminent)
            .tint(color)
    }
}

func sizeCategoryFromScale(_ scale: Double) -> ContentSizeCategory {
    switch scale {
    case ..<0.95: return .medium
    case ..<1.05: return .large
    case ..<1.2: return .extraLarge
    case ..<1.4: return .extraExtraLarge
    case ..<1.6: return .extraExtraExtraLarge
    default: return .accessibilityMedium
    }
}
