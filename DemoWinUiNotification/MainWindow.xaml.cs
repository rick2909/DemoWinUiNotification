using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DemoWinUiNotification
{
    public sealed partial class MainWindow : Window
    {
        private readonly ObservableCollection<string> _messages = new();
        private readonly HttpClient _httpClient = new();
        private HubConnection? _hubConnection;

        public MainWindow()
        {
            InitializeComponent();
            MessagesListView.ItemsSource = _messages;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var functionBaseUrl = FunctionBaseUrlTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(functionBaseUrl))
            {
                StatusTextBlock.Text = "Geef eerst een geldige Function URL op.";
                return;
            }

            await ConnectAsync(functionBaseUrl);
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            await DisconnectAsync();
        }

        private async void SendTestButton_Click(object sender, RoutedEventArgs e)
        {
            var message = string.IsNullOrWhiteSpace(TestMessageTextBox.Text)
                ? "Hallo van WinUI"
                : TestMessageTextBox.Text.Trim();

            var senderName = string.IsNullOrWhiteSpace(SenderNameTextBox.Text)
                ? "winui-client"
                : SenderNameTextBox.Text.Trim();

            if (!TryGetFunctionBaseUri(out var baseUri))
            {
                StatusTextBlock.Text = "Ongeldige Function URL.";
                return;
            }

            var apiUri = new Uri(baseUri, "/api/notify");

            try
            {
                var response = await _httpClient.PostAsJsonAsync(apiUri, new { message, sender = senderName });
                response.EnsureSuccessStatusCode();
                StatusTextBlock.Text = "Testbericht verstuurd via Function.";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Versturen mislukt: {ex.Message}";
            }
        }

        private async Task ConnectAsync(string functionBaseUrl)
        {
            if (!Uri.TryCreate(functionBaseUrl, UriKind.Absolute, out var baseUri))
            {
                StatusTextBlock.Text = "Function URL is ongeldig.";
                return;
            }

            try
            {
                await DisconnectAsync();

                var negotiateUri = new Uri(baseUri, "/api/negotiate");
                var negotiate = await _httpClient.GetFromJsonAsync<NegotiateResponse>(negotiateUri);

                if (negotiate is null || string.IsNullOrWhiteSpace(negotiate.Url))
                {
                    StatusTextBlock.Text = "Negotiate response is ongeldig.";
                    return;
                }

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(negotiate.Url, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(negotiate.AccessToken);
                    })
                    .WithAutomaticReconnect()
                    .Build();

                _hubConnection.On<string, string, DateTimeOffset>("ReceiveMessage", (message, sender, at) =>
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        _messages.Insert(0, $"[{at:HH:mm:ss}] {sender}: {message}");
                        ShowNativeNotification(sender, message, at);
                    });
                });

                _hubConnection.Reconnecting += _ =>
                {
                    DispatcherQueue.TryEnqueue(() => StatusTextBlock.Text = "Verbinding kwijt. Opnieuw verbinden...");
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += _ =>
                {
                    DispatcherQueue.TryEnqueue(() => StatusTextBlock.Text = "Opnieuw verbonden.");
                    return Task.CompletedTask;
                };

                _hubConnection.Closed += _ =>
                {
                    DispatcherQueue.TryEnqueue(() => StatusTextBlock.Text = "Verbinding gesloten.");
                    return Task.CompletedTask;
                };

                await _hubConnection.StartAsync();

                StatusTextBlock.Text = "Verbonden via Function negotiate + SignalR.";
                _messages.Insert(0, $"[{DateTimeOffset.Now:HH:mm:ss}] systeem: verbonden");
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Verbinden mislukt: {ex.Message}";
            }
        }

        private async Task DisconnectAsync()
        {
            if (_hubConnection is null)
            {
                return;
            }

            try
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                StatusTextBlock.Text = "Niet verbonden";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Verbreken mislukt: {ex.Message}";
            }
        }

        private bool TryGetFunctionBaseUri(out Uri baseUri)
        {
            return Uri.TryCreate(FunctionBaseUrlTextBox.Text?.Trim(), UriKind.Absolute, out baseUri!);
        }

        private static void ShowNativeNotification(string sender, string message, DateTimeOffset at)
        {
            try
            {
                var notification = new AppNotificationBuilder()
                    .AddText($"Nieuw bericht van {sender}")
                    .AddText($"Ontvangen om: {at:HH:mm:ss}")
                    .AddText(message)
                    .BuildNotification();

                AppNotificationManager.Default.Show(notification);
            }
            catch (COMException)
            {
                // Geen geregistreerde COM server in huidig runprofiel.
            }
        }

        private sealed class NegotiateResponse
        {
            public string Url { get; set; } = string.Empty;
            public string AccessToken { get; set; } = string.Empty;
        }
    }
}
