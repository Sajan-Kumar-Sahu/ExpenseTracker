using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
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
            if (FirebaseApp.DefaultInstance is not null)
            {
                Console.WriteLine("[Firebase] Already initialized.");
                return;
            }

            lock (_lock)
            {
                if (FirebaseApp.DefaultInstance is not null)
                {
                    Console.WriteLine("[Firebase] Already initialized inside lock.");
                    return;
                }

                Console.WriteLine("========== FIREBASE DEBUG ==========");

                var base64 = configuration["Firebase:ServiceAccountKeyBase64"];
                Console.WriteLine($"Base64 Exists : {!string.IsNullOrWhiteSpace(base64)}");
                Console.WriteLine($"Base64 Length : {base64?.Length ?? 0}");

                var inlineJson = configuration["Firebase:ServiceAccountKeyJson"];
                Console.WriteLine($"Inline JSON Exists : {!string.IsNullOrWhiteSpace(inlineJson)}");
                Console.WriteLine($"Inline JSON Length : {inlineJson?.Length ?? 0}");

                var keyPath = configuration["Firebase:ServiceAccountKeyPath"];
                Console.WriteLine($"KeyPath : {keyPath}");

                GoogleCredential credential;

                if (!string.IsNullOrWhiteSpace(base64))
                {
                    Console.WriteLine("[Firebase] Using Base64 credentials.");

                    try
                    {
                        var json = Encoding.UTF8.GetString(
                            Convert.FromBase64String(base64.Trim()));

                        Console.WriteLine($"Decoded JSON Length : {json.Length}");
                        Console.WriteLine($"Decoded JSON Starts With : {json.Substring(0, Math.Min(50, json.Length))}");

                        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

                        credential = GoogleCredential.FromStream(stream);

                        Console.WriteLine("[Firebase] GoogleCredential created successfully from Base64.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[Firebase] Failed to decode Base64.");
                        Console.WriteLine(ex.ToString());
                        throw;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(inlineJson))
                {
                    Console.WriteLine("[Firebase] Using inline JSON.");

                    try
                    {
                        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(inlineJson));

                        credential = GoogleCredential.FromStream(stream);

                        Console.WriteLine("[Firebase] GoogleCredential created successfully from inline JSON.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[Firebase] Failed to parse inline JSON.");
                        Console.WriteLine(ex.ToString());
                        throw;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(keyPath))
                {
                    Console.WriteLine("[Firebase] Using key path.");

                    Console.WriteLine($"File Exists : {File.Exists(keyPath)}");

                    using var stream = File.OpenRead(keyPath);

                    credential = GoogleCredential.FromStream(stream);

                    Console.WriteLine("[Firebase] GoogleCredential created successfully from file.");
                }
                else
                {
                    Console.WriteLine("[Firebase] No Firebase configuration found.");

                    foreach (var item in configuration.AsEnumerable()
                                                      .Where(x => x.Key.StartsWith("Firebase")))
                    {
                        Console.WriteLine($"{item.Key} = Length({item.Value?.Length ?? 0})");
                    }

                    throw new InvalidOperationException(
                        "Firebase credentials not configured.");
                }

                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential
                });

                Console.WriteLine("[Firebase] FirebaseApp created successfully.");
                Console.WriteLine("====================================");
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
