// swift-tools-version:5.9
import PackageDescription

let package = Package(
    name: "FineChatLike",
    platforms: [.iOS(.v16)],
    products: [
        .iOSApplication(
            name: "FineChatLike",
            targets: ["AppModule"],
            bundleIdentifier: "com.example.finechatlike",
            teamIdentifier: "",
            displayVersion: "1.0",
            bundleVersion: "1",
            appIcon: .placeholder(icon: .message),
            accentColor: .presetColor(.blue),
            supportedDeviceFamilies: [.pad],
            supportedInterfaceOrientations: [
                .portrait,
                .portraitUpsideDown,
                .landscapeLeft,
                .landscapeRight
            ]
        )
    ],
    targets: [
        .executableTarget(
            name: "AppModule",
            path: "Sources/AppModule"
        )
    ]
)
