using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using TG_Bot.DAL;
using TG_Bot.Helpers;
using File = System.IO.File;
using Monitor = TG_Bot.monitoring.Monitor;

namespace TG_Bot.BusinessLayer
{
    public class BotService : IBotService, IHostedService, IDisposable
    {
        private readonly ILogger<BotService> _logger;
        private readonly IStateService _stateService;
        private readonly ICamService _camService;
        private readonly IConfiguration _configuration;
        private ITelegramBotClient _botClient;
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts =
            new CancellationTokenSource();

        /// <summary>
        /// Основная клавиатура
        /// </summary>
        private InlineKeyboardMarkup _keyboard =>
            new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Все параметры", "state"),
                    InlineKeyboardButton.WithCallbackData("Электричество", "electricity"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Нагрев", "heating"),
                    InlineKeyboardButton.WithCallbackData("Температура", "temperature"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Камеры","cameras"),
                }
            });

        /// <summary>
        /// Клавиатура с камерами
        /// </summary>
        private InlineKeyboardMarkup _camerasKeyboard =>
            new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Въезд", "entrance"),
                InlineKeyboardButton.WithCallbackData("Двор", "yard"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData("Назад", "back")
            }
        });

        private string BotToken
        {
            get
            {
                IEnumerable<IConfigurationSection> sections = _configuration.GetSection("BotConfiguration").GetChildren();
                var token = sections.FirstOrDefault(_ => _.Key == "BotToken");
                if (token == null)
                {
                    _logger.LogError($"Не найден токен для бота, выход");
                    return string.Empty;
                }

                return token.Value;
            }
        }

        public BotService(ILogger<BotService> logger, IStateService stateService, ICamService camService, IConfiguration configuration)
        {
            _logger = logger;
            _stateService = stateService;
            _camService = camService;
            _configuration = configuration;
            //if (string.IsNullOrEmpty(BotToken))
            //{
            //    _stoppingCts.Cancel();
            //}
            //else
            //{
            //    _botClient = new TelegramBotClient(BotToken);
            //}
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(update.Message),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
                UpdateType.InlineQuery => BotOnInlineQueryReceived(update.InlineQuery),
                UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult),
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private async Task BotOnMessageReceived(Message message)
        {
            _logger.LogInformation($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            //var action = (message.Text.Split(' ').First()) switch
            //{
            //    "/state" => SendInlineKeyboard(message),
            //    "/temperature" => SendInlineKeyboard(message),
            //    //"/keyboard" => SendReplyKeyboard(message),
            //    //"/photo" => SendFile(message),
            //    //"/request" => RequestContactAndLocation(message),
            //    _ => Usage(message)
            //};
            var action = SendInlineKeyboard(message);
            await action;


        }

        // Send inline keyboard
        // You can process responses in BotOnCallbackQueryReceived handler
        async Task SendInlineKeyboard(Message message)
        {
            await _botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            InlineKeyboardMarkup inlineKeyboard;
            switch (message.Text)
            {
                case "/start":
                    inlineKeyboard = _keyboard;
                    break;

                default:
                    inlineKeyboard = _keyboard;
                    break;
            }


            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Выберите запрос",
                replyMarkup: inlineKeyboard
            );
        }

        //async Task SendInlineKeyboard(long ChatId)
        //{
        //    await _botClient.SendChatActionAsync(ChatId, ChatAction.Typing);
        //    await _botClient.SendTextMessageAsync(
        //        chatId: ChatId,
        //        text: "Выберите запрос",
        //        replyMarkup: _keyboard
        //    );
        //}

        async Task SendFile(Message message)
        {
            await _botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            const string filePath = @"Files/tux.png";
            using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

            await _botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: new InputOnlineFile(fileStream, fileName),
                caption: "Nice Picture"
            );
        }

        async Task RequestContactAndLocation(Message message)
        {
            var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact"),
                });
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Who or Where are you?",
                replyMarkup: RequestReplyKeyboard
            );
        }

        async Task Usage(Message message)
        {
            //const string usage = "Usage:\n" +
            //                        "/inline   - send inline keyboard\n" +
            //                        "/keyboard - send custom keyboard\n" +
            //                        "/photo    - send a photo\n" +
            //                        "/request  - request location or contact";
            const string usage = "Использование: \n" +
                                 "/state          - текущее состояние системы\n" +
                                 "/temperature    - температура объектов";
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove()
            );
        }

        // Process Inline Keyboard callback data
        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            string userName = callbackQuery.Message.Chat.Username;
            switch (callbackQuery.Data)
            {
                case "state":
                    var state = await _stateService.LastState();
                    await Answer(callbackQuery, state);
                    _logger.LogInformation(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                        ? $"Запрос состояния"
                        : $"Запрос состояния от {callbackQuery.From.FirstName}");
                    break;

                case "electricity":
                    var electricity = await _stateService.Electricity();
                    await Answer(callbackQuery, electricity);
                    _logger.LogInformation(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                        ? $"Запрос показаний по электричеству"
                        : $"Запрос показаний по электричеству от {callbackQuery.From.FirstName}");
                    break;

                case "heating":
                    var heating = await _stateService.Heating();
                    await Answer(callbackQuery, heating);
                    _logger.LogInformation(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                        ? $"Запрос состояния нагревательных элементов"
                        : $"Запрос состояния нагревательных элементов от {callbackQuery.From.FirstName}");
                    break;

                case "temperature":
                    var temperature = await _stateService.Temperature();
                    await Answer(callbackQuery, temperature);
                    _logger.LogInformation(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                        ? $"Запрос показаний температуры"
                        : $"Запрос показаний температуры от {callbackQuery.From.FirstName}");
                    break;

                case "cameras":
                    await _botClient.AnswerCallbackQueryAsync(
                        callbackQuery.Id
                    );

                    //удаление главной клавиатуры
                    await _botClient.EditMessageReplyMarkupAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        messageId: callbackQuery.Message.MessageId,
                        replyMarkup: _camerasKeyboard);
                    break;

                case "entrance":
                    //ответ на кнопку
                    await _botClient.AnswerCallbackQueryAsync(
                        callbackQuery.Id
                    );
                    // Показываем статус отправки фото
                    await _botClient.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.UploadPhoto);
                    string filePath = string.Empty;
                    //отправка фото
                    try
                    {
                        filePath = _camService.GetEntranceCam(out var fileName);
                        await using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        await _botClient.SendPhotoAsync(
                            chatId: callbackQuery.Message.Chat.Id,
                            photo: new InputOnlineFile(fileStream, fileName),
                            replyMarkup: _camerasKeyboard
                        );

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Ошибка получения изображения с камеры въезда - {ex.Message}");
                        await _botClient.SendTextMessageAsync(
                            chatId: callbackQuery.Message.Chat.Id,
                            text: "Невозможно получить изображение с камеры въезда",
                            replyMarkup: _camerasKeyboard);
                    }

                    _logger.LogInformation(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                        ? $"Запрос изображения с камеры въезда"
                        : $"Запрос изображения с камеры въезда от {callbackQuery.From.FirstName}");
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Не удалось удалить изображение с камеры из временной директории - {ex.Message}");
                    }
                    break;

                case "back":
                    await _botClient.AnswerCallbackQueryAsync(
                        callbackQuery.Id
                    );
                    await _botClient.EditMessageReplyMarkupAsync(
                         chatId: callbackQuery.Message.Chat.Id,
                         messageId: callbackQuery.Message.MessageId,
                         replyMarkup: _keyboard);
                    break;

                default:

                    break;
            }

        }

        /// <summary>
        /// Ответ
        /// </summary>
        /// <param name="callbackQuery">Callback запрос</param>
        /// <param name="result">Строка для ответа</param>
        /// <returns>Инстанс таски для возврата</returns>
        private async Task Answer(CallbackQuery callbackQuery, string result)
        {
            //ответ
            await _botClient.AnswerCallbackQueryAsync(
                callbackQuery.Id
            );

            //удаление главной клавиатуры
            await _botClient.EditMessageReplyMarkupAsync(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                replyMarkup: null);

            //отправка результата + кнопка "Назад"
            await _botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: result,
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "back"),
                }));
        }

        #region Inline Mode

        private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery)
        {
            Console.WriteLine($"Received inline query from: {inlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                // displayed result
                new InlineQueryResultArticle(
                    id: "3",
                    title: "TgBots",
                    inputMessageContent: new InputTextMessageContent(
                        "hello"
                    )
                )
            };

            await _botClient.AnswerInlineQueryAsync(
                inlineQuery.Id,
                results,
                isPersonal: true,
                cacheTime: 0
            );
        }

        private async Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
        }

        #endregion

        private async Task UnknownUpdateHandlerAsync(Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Bot service is starting.");
            // TODO разобраться с отменой задач
            CancellationTokenSource.CreateLinkedTokenSource(_stoppingCts.Token, cancellationToken);
            //_stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _stoppingCts.Token.Register(() =>
                _logger.LogDebug($"Bot service stopping"));

            if (string.IsNullOrEmpty(BotToken))
            {
                _stoppingCts.Cancel();
            }
            else
            {
                _botClient = new TelegramBotClient(BotToken);
            }

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            _executingTask = new Task(() =>
            {
                _botClient.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                        _stoppingCts.Token);
                _logger.LogInformation($"Telegram bot started receiveing");
            }, _stoppingCts.Token);

            try
            {
                _executingTask.Start();
                _logger.LogInformation("Telegram bot started, waiting for messages");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Работа бота отменена, не найден токен");
            }
            //_stoppingCts.Token.ThrowIfCancellationRequested();
            await _executingTask;
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
            {
                return;
            }

            //_botClient.StopReceiving();
            _stoppingCts.Cancel();

            await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));

            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("Bot service stoppped");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _logger.LogInformation("Dispose");
            _executingTask.Dispose();
            try
            {
                _botClient.StopReceiving();
            }
            catch (Exception ex)
            {
            }
            Interlocked.Exchange(ref _botClient, null);
            _stoppingCts.Dispose();
            //Dispose
        }
    }
}
