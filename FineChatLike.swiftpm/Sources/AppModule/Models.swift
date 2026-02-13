import Foundation

struct PhraseCategory: Codable, Identifiable, Hashable {
    var id: UUID
    var name: String
}

struct PhraseItem: Codable, Identifiable, Hashable {
    var id: UUID
    var categoryID: UUID
    var text: String
    var isFavorite: Bool
}

struct SettingsState: Codable {
    var textScale: Double
    var highContrast: Bool
    var historyLimit: Int
    var speechRate: Double
    var speechPitch: Double
    var caregiverPIN: String

    static let `default` = SettingsState(
        textScale: 1.0,
        highContrast: false,
        historyLimit: 20,
        speechRate: 0.5,
        speechPitch: 1.0,
        caregiverPIN: "0000"
    )

    mutating func normalize() {
        textScale = textScale.clamped(to: 0.8...1.8)
        speechRate = speechRate.clamped(to: 0.3...0.65)
        speechPitch = speechPitch.clamped(to: 0.5...1.8)
        historyLimit = historyLimit.clamped(to: 5...50)
        if !Self.isValidPin(caregiverPIN) {
            caregiverPIN = "0000"
        }
    }

    static func isValidPin(_ value: String) -> Bool {
        value.count == 4 && value.allSatisfy(\.isNumber)
    }
}

struct AppState: Codable {
    var categories: [PhraseCategory]
    var phrases: [PhraseItem]
    var history: [String]
    var settings: SettingsState

    static func seed() -> AppState {
        let daily = PhraseCategory(id: UUID(), name: "日常")
        let care = PhraseCategory(id: UUID(), name: "介助")
        return AppState(
            categories: [daily, care],
            phrases: [
                PhraseItem(id: UUID(), categoryID: daily.id, text: "おはようございます", isFavorite: true),
                PhraseItem(id: UUID(), categoryID: daily.id, text: "ありがとうございます", isFavorite: true),
                PhraseItem(id: UUID(), categoryID: daily.id, text: "少し待ってください", isFavorite: false),
                PhraseItem(id: UUID(), categoryID: care.id, text: "水をください", isFavorite: true),
                PhraseItem(id: UUID(), categoryID: care.id, text: "体勢を直してください", isFavorite: false),
                PhraseItem(id: UUID(), categoryID: care.id, text: "トイレに行きたいです", isFavorite: false)
            ],
            history: [],
            settings: .default
        )
    }
}

extension Comparable {
    func clamped(to range: ClosedRange<Self>) -> Self {
        min(max(self, range.lowerBound), range.upperBound)
    }
}
