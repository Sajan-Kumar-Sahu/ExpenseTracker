using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FcmNotification = FirebaseAdmin.Messaging.Notification;

namespace ExpenseTracker.Infrastructure.Services
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly ILogger<PushNotificationService> _logger;
        private readonly IUserRepository _userRepository;

        private static readonly object _lock = new();

        public PushNotificationService(
            ILogger<PushNotificationService> logger,
            IConfiguration configuration,
            IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
            EnsureFirebaseInitialized(configuration);
        }

        private static void EnsureFirebaseInitialized(IConfiguration configuration)
        {
            if (FirebaseApp.DefaultInstance is not null) return;

            lock (_lock)
            {
                if (FirebaseApp.DefaultInstance is not null) return;

                GoogleCredential credential;

                var inlineJson = configuration["Firebase:ServiceAccountKeyJson"];

                Console.WriteLine("========== FIREBASE DEBUG ==========");
                Console.WriteLine($"Is null: {inlineJson == null}");
                Console.WriteLine($"Is empty: {inlineJson == ""}");
                Console.WriteLine($"Length: {inlineJson?.Length ?? 0}");

                if (inlineJson != null)
                {
                    Console.WriteLine($"First 30 chars: {inlineJson.Substring(0, Math.Min(30, inlineJson.Length))}");
                }
                Console.WriteLine("====================================");
                if (!string.IsNullOrWhiteSpace(inlineJson))
                {
                    using var jsonStream = new System.IO.MemoryStream(
                        System.Text.Encoding.UTF8.GetBytes(inlineJson));
                    credential = GoogleCredential.FromStream(jsonStream);
                }
                else
                {
                    var keyPath = configuration["Firebase:ServiceAccountKeyPath"];
                    if (string.IsNullOrWhiteSpace(keyPath))
                        throw new InvalidOperationException(
                            "Firebase credentials not configured. Set Firebase:ServiceAccountKeyJson or Firebase:ServiceAccountKeyPath.");

                    using var fileStream = System.IO.File.OpenRead(keyPath);
                    credential = GoogleCredential.FromStream(fileStream);
                }

                FirebaseApp.Create(new AppOptions { Credential = credential });
            }
        }

        public async Task<bool> SendAsync(
            string deviceToken,
            string title,
            string body,
            Dictionary<string, string>? data = null)
        {
            try
            {
                var message = new Message
                {
                    Token = deviceToken,
                    Notification = new FcmNotification { Title = title, Body = body },
                    Data = data ?? new Dictionary<string, string>(),
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification
                        {
                            ChannelId = "reminders_channel",
                            Sound = "default"
                        }
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps { Sound = "default", Badge = 1 }
                    }
                };

                var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation("FCM message sent. MessageId: {MessageId}", messageId);
                return true;
            }
            catch (FirebaseMessagingException ex)
            {
                _logger.LogError(ex,
                    "FCM error sending to token {Token}: {ErrorCode}",
                    deviceToken[..Math.Min(8, deviceToken.Length)] + "...",
                    ex.MessagingErrorCode);
                return false;
            }
        }

        public async Task<bool> SendToUserAsync(
            Guid userId,
            string title,
            string body,
            Dictionary<string, string>? data = null)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user?.DeviceToken is null)
            {
                _logger.LogWarning("No device token registered for user {UserId}", userId);
                return false;
            }

            return await SendAsync(user.DeviceToken, title, body, data);
        }

        public async Task<bool> SendReminderAsync(Reminder reminder)
        {
            var user = await _userRepository.GetByIdAsync(reminder.UserId);
            if (user?.DeviceToken is null)
            {
                _logger.LogWarning("No device token registered for user {UserId}", reminder.UserId);
                return false;
            }

            var data = new Dictionary<string, string>
            {
                ["reminderId"] = reminder.Id.ToString(),
                ["type"] = "reminder"
            };

            return await SendAsync(user.DeviceToken, reminder.Title, reminder.Message, data);
        }
    }
}
