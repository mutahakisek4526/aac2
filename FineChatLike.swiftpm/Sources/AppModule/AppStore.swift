import AVFoundation
import Foundation
import SwiftUI
import UIKit

@MainActor
final class AppStore: ObservableObject {
    @Published var inputText: String = ""
    @Published private(set) var state: AppState

    private let defaultsKey = "finechatlike.state.json"
    private let defaults = UserDefaults.standard
    private let encoder = JSONEncoder()
    private let decoder = JSONDecoder()
    private let speaker = Speaker()

    init() {
        if let raw = defaults.string(forKey: defaultsKey),
           let data = raw.data(using: .utf8),
           let decoded = try? decoder.decode(AppState.self, from: data) {
            var normalized = decoded
            normalized.settings.normalize()
            state = normalized
        } else {
            state = AppState.seed()
        }
    }

    var favorites: [PhraseItem] {
        Array(state.phrases.filter(\.isFavorite).prefix(12))
    }

    func phrases(in categoryID: UUID) -> [PhraseItem] {
        state.phrases.filter { $0.categoryID == categoryID }
    }

    func speakCurrent() {
        speak(text: inputText)
    }

    func speak(text: String) {
        let trimmed = text.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmed.isEmpty else { return }
        speaker.speak(trimmed, rate: Float(state.settings.speechRate), pitch: Float(state.settings.speechPitch))
        addHistory(trimmed)
    }

    func stopSpeak() {
        speaker.stop()
    }

    func undoInput() {
        guard !inputText.isEmpty else { return }
        inputText.removeLast()
    }

    func deleteLastWord() {
        guard !inputText.isEmpty else { return }
        var chars = Array(inputText)
        while let last = chars.last, last.isWhitespace || last.isNewline {
            chars.removeLast()
        }
        while let last = chars.last, !last.isWhitespace && !last.isNewline {
            chars.removeLast()
        }
        inputText = String(chars)
    }

    func clearInput() {
        inputText = ""
    }

    func applyHistory(_ text: String, speakNow: Bool) {
        inputText = text
        if speakNow { speak(text: text) }
    }

    func insertText(_ text: String) {
        inputText += text
    }

    func copyInputToClipboard() {
        let trimmed = inputText.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmed.isEmpty else { return }
        UIPasteboard.general.string = trimmed
    }

    func lineSendURL() -> URL? {
        let trimmed = inputText.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmed.isEmpty else { return nil }
        let encoded = trimmed.addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed) ?? ""
        guard !encoded.isEmpty else { return nil }
        return URL(string: "line://msg/text/\(encoded)")
    }

    func addCategory(name: String) {
        let trimmed = name.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmed.isEmpty else { return }
        state.categories.append(PhraseCategory(id: UUID(), name: trimmed))
        save()
    }

    func addPhrase(categoryID: UUID, text: String) {
        let trimmed = text.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmed.isEmpty else { return }
        state.phrases.append(PhraseItem(id: UUID(), categoryID: categoryID, text: trimmed, isFavorite: false))
        save()
    }

    func updatePhrase(_ phrase: PhraseItem, text: String) {
        let trimmed = text.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmed.isEmpty else { return }
        if let i = state.phrases.firstIndex(where: { $0.id == phrase.id }) {
            state.phrases[i].text = trimmed
            save()
        }
    }

    func toggleFavorite(_ phrase: PhraseItem) {
        if let i = state.phrases.firstIndex(where: { $0.id == phrase.id }) {
            state.phrases[i].isFavorite.toggle()
            save()
        }
    }

    func deletePhrase(_ phrase: PhraseItem) {
        state.phrases.removeAll { $0.id == phrase.id }
        save()
    }

    func updateSettings(_ block: (inout SettingsState) -> Void) {
        block(&state.settings)
        state.settings.normalize()
        save()
    }

    func changePIN(newPIN: String) -> Bool {
        guard SettingsState.isValidPin(newPIN) else { return false }
        updateSettings { $0.caregiverPIN = newPIN }
        return true
    }

    func predictions() -> [String] {
        let base = [
            "おはようございます", "こんにちは", "こんばんは", "ありがとうございます",
            "お願いします", "すみません", "大丈夫です", "はい", "いいえ",
            "いたいです", "ねむいです", "のみものをください"
        ] + state.history

        let prefix = inputText.hiraganaSuffixPrefix()
        guard !prefix.isEmpty else {
            return Array(Array(Set(base)).prefix(6))
        }
        let unique = Array(Set(base))
        return Array(unique.filter { $0.contains(prefix) }.prefix(6))
    }

    private func addHistory(_ text: String) {
        state.history.removeAll { $0 == text }
        state.history.insert(text, at: 0)
        let limit = state.settings.historyLimit.clamped(to: 5...50)
        if state.history.count > limit {
            state.history = Array(state.history.prefix(limit))
        }
        save()
    }

    private func save() {
        guard let data = try? encoder.encode(state),
              let raw = String(data: data, encoding: .utf8) else {
            return
        }
        defaults.set(raw, forKey: defaultsKey)
    }
}

final class Speaker {
    private let synth = AVSpeechSynthesizer()

    func speak(_ text: String, rate: Float, pitch: Float) {
        let utterance = AVSpeechUtterance(string: text)
        utterance.voice = AVSpeechSynthesisVoice(language: "ja-JP")
        utterance.rate = rate
        utterance.pitchMultiplier = pitch
        synth.speak(utterance)
    }

    func stop() {
        synth.stopSpeaking(at: .immediate)
    }
}

private extension String {
    func hiraganaSuffixPrefix() -> String {
        let chars = Array(self)
        guard !chars.isEmpty else { return "" }
        var collected: [Character] = []
        for ch in chars.reversed() {
            if ch.isHiragana {
                collected.append(ch)
            } else {
                break
            }
        }
        return String(collected.reversed())
    }
}

private extension Character {
    var isHiragana: Bool {
        unicodeScalars.allSatisfy { (0x3040...0x309F).contains(Int($0.value)) }
    }
}
