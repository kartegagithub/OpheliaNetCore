# Ophelia Integration Notification

A unified messaging hub for sending push notifications across various platforms and services.

## 📂 Core Logic

### [Notifier.cs](./Notifier.cs)
The central entry point for dispatching notifications. It abstracts away the complexity of target platforms.

### [PushNotification.cs](./PushNotification.cs)
The base model representing a notification payload, including title, body, and custom data.

## 📱 Supported Platforms

- **[Firebase](./Firebase)**: Google's Firebase Cloud Messaging (FCM) for Android and Web.
- **[OneSignal](./OneSignal)**: High-level messaging service for cross-platform notifications.
- **[Expo](./Expo)**: Notifications for React Native applications using the Expo managed workflow.
- **[WebPush](./WebPush)**: Standard browser-based web push notifications.
- **[iOS](./iOS)** & **[Android](./Android)**: Platform-specific low-level implementations.

## 🛠 Usage

```csharp
var notifier = new Notifier();
notifier.Send(new PushNotification {
    Title = "Hello",
    Body = "You have a new message"
}, platform: OperatingSystemEnum.Android);
```
