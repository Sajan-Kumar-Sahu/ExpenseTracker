using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

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
            EnsureFirebaseInitialized(configuration, logger);
        }

        private static void EnsureFirebaseInitialized(IConfiguration configuration, ILogger logger)
        {
            if (FirebaseApp.DefaultInstance is not null) return;

            lock (_lock)
            {
                if (FirebaseApp.DefaultInstance is not null) return;

                GoogleCredential credential;

                var base64 = configuration["Firebase:ServiceAccountKeyBase64"];
                if (string.IsNullOrWhiteSpace(base64))
                    base64 = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_BASE64");

                var inlineJson = configuration["Firebase:ServiceAccountKeyJson"];
                if (string.IsNullOrWhiteSpace(inlineJson))
                    inlineJson = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_JSON");

                var keyPath = configuration["Firebase:ServiceAccountKeyPath"];

                if (!string.IsNullOrWhiteSpace(base64))
                {
                    var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64.Trim()));
                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                    credential = GoogleCredential.FromStream(stream);
                    logger.LogInformation("[Firebase] Initialized from Base64 credentials.");
                }
                else if (!string.IsNullOrWhiteSpace(inlineJson))
                {
                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(inlineJson));
                    credential = GoogleCredential.FromStream(stream);
                    logger.LogInformation("[Firebase] Initialized from inline JSON.");
                }
                else if (!string.IsNullOrWhiteSpace(keyPath))
                {
                    using var stream = File.OpenRead(keyPath);
                    credential = GoogleCredential.FromStream(stream);
                    logger.LogInformation("[Firebase] Initialized from key file.");
                }
                else
                {
                    throw new InvalidOperationException(
                        "Firebase credentials not configured. Set Firebase__ServiceAccountKeyJson, " +
                        "Firebase__ServiceAccountKeyBase64, or Firebase__ServiceAccountKeyPath.");
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
                // Data-only message: no Notification payload.
                // Android treats messages with a Notification payload as "notification messages"
                // and batches them during Doze even at Priority.High.
                // Data-only + Priority.High reliably wakes the device from Doze immediately.
                var payload = new Dictionary<string, string>(data ?? new Dictionary<string, string>())
                {
                    ["title"] = title,
                    ["body"] = body,
                };

                var message = new Message
                {
                    Token = deviceToken,
                    Data = payload,
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High,
                    },
                    Apns = new ApnsConfig
                    {
                        Headers = new Dictionary<string, string>
                        {
                            // content-available=1 wakes the iOS app for background processing
                            ["apns-push-type"] = "background",
                            ["apns-priority"] = "5",
                        },
                        Aps = new Aps
                        {
                            ContentAvailable = true,
                            Sound = "default",
                            Badge = 1,
                        },
                    },
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
