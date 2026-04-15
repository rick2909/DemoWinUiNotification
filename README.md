# DemoWinUiNotification

WinUI 3 desktop demo that connects through Azure Functions (negotiate) to Azure SignalR and shows real-time notifications in-app and as native Windows notifications.

## Release Asset

- DemoWinUiNotification-win-x64-v1.0.0.zip

## What is included

- WinUI 3 desktop app build (win-x64, self-contained)
- Real-time message receive via SignalR event: ReceiveMessage
- Native Windows app notifications for incoming messages
- Configurable Function base URL in the UI (default currently points to https://winui3demobackend.azurewebsites.net)

## How to open and run

1. Download DemoWinUiNotification-win-x64-v1.0.0.zip from the GitHub Release.
2. Extract the zip to a local folder, for example C:\Apps\DemoWinUiNotification.
3. Open the extracted folder.
4. Start DemoWinUiNotification.exe.
5. In the app, verify Function base URL is set to https://winui3demobackend.azurewebsites.net.
6. Click Verbind.
7. Confirm status changes to connected (Dutch UI text: Verbonden via Function negotiate + SignalR.).

Notes:
- If Windows SmartScreen appears, click More info and then Run anyway.
- This build targets win-x64.

## Quick app test

1. Enter a sender name in Sender (or keep winui-client).
2. Enter text in Testbericht.
3. Click Stuur via Function.
4. Expected result:
   - A new message appears in Ontvangen berichten.
   - A native Windows notification appears.

## API test using Azure Function

Base URL:
- https://winui3demobackend.azurewebsites.net

### 1) Optional connection check (negotiate)

GET https://winui3demobackend.azurewebsites.net/api/negotiate

Expected: JSON containing url and accessToken.

### 2) Send a test message (notify)

POST https://winui3demobackend.azurewebsites.net/api/notify
Content-Type: application/json

{
  "message": "Hello from API test",
  "sender": "github-release-test"
}

Expected:
- HTTP 200 response
- Connected app clients receive the message immediately
- Sender and message appear in the list and trigger a Windows notification

### cURL example

curl -X POST "https://winui3demobackend.azurewebsites.net/api/notify" \
  -H "Content-Type: application/json" \
  -d "{\"message\":\"Hello from cURL\",\"sender\":\"curl\"}"

### PowerShell example

$body = @{
  message = "Hello from PowerShell"
  sender  = "powershell"
} | ConvertTo-Json

Invoke-RestMethod \
  -Method Post \
  -Uri "https://winui3demobackend.azurewebsites.net/api/notify" \
  -ContentType "application/json" \
  -Body $body

## Troubleshooting

- Not connecting:
  - Verify internet connectivity.
  - Verify the Function URL is exactly https://winui3demobackend.azurewebsites.net.
  - Test GET /api/negotiate in a browser or Postman.
- Message not showing:
  - Ensure app is connected before posting to /api/notify.
  - Check that request body includes message and or sender.
