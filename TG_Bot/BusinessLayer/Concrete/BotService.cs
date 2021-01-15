using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using TG_Bot.BusinessLayer.Abstract;
using File = System.IO.File;

namespace TG_Bot.BusinessLayer.Concrete
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
        private CancellationToken Token => _stoppingCts.Token;

        private string _botToken = string.Empty;

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

        /// <summary>
        /// Получение токена телеграм бота
        /// </summary>
        private string BotToken
        {
            get
            {
                if (!string.IsNullOrEmpty(_botToken))
                {
                    return _botToken;
                }
                IEnumerable<IConfigurationSection> sections = _configuration.GetSection("BotConfiguration").GetChildren();
                var tokenSection = sections.FirstOrDefault(_ => _.Key == "BotToken");
                if (tokenSection == null)
                {
                    _logger.LogError($"Не найден токен для бота, выход");
                    return string.Empty;
                }

                switch (tokenSection.Value)
                {
                    case "<BotToken>":
                    {
                        string token = GetBotTokenFromFile();
                        _botToken = token;
                        return string.IsNullOrEmpty(token) ? string.Empty : token;
                    }

                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Получение токена из файла
        /// </summary>
        /// <returns></returns>
        private string GetBotTokenFromFile()
        {
            //string codeBase = Assembly.GetExecutingAssembly().Location;
            var path = Directory.GetParent(AppContext.BaseDirectory).FullName;
            //UriBuilder uri = new UriBuilder(codeBase);
            //string tmp = Uri.UnescapeDataString(uri.Path);
            var PathToken = Path.Combine(path, "BotToken.txt");
            if (!File.Exists(PathToken))
                return null;

            return File.ReadAllText(PathToken);
        }

        public BotService(ILogger<BotService> logger, IStateService stateService, ICamService camService, IConfiguration configuration)
        {
            _logger = logger;
            _stateService = stateService;
            _camService = camService;
            _configuration = configuration;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(update.Message),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
                UpdateType.InlineQuery => BotOnInlineQueryReceived(update.InlineQuery),
                //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult),
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
            _logger.LogInformation($"Старт работы с ботом");
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
            await _botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing, Token);
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
                replyMarkup: inlineKeyboard, cancellationToken: Token);
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

        //async Task SendFile(Message message)
        //{
        //    await _botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto, Token);

        //    const string filePath = @"Files/tux.png";
        //    using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        //    var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

        //    await _botClient.SendPhotoAsync(
        //        chatId: message.Chat.Id,
        //        photo: new InputOnlineFile(fileStream, fileName),
        //        caption: "Nice Picture", cancellationToken: Token);
        //}

        //async Task RequestContactAndLocation(Message message)
        //{
        //    var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
        //    {
        //            KeyboardButton.WithRequestLocation("Location"),
        //            KeyboardButton.WithRequestContact("Contact"),
        //        });
        //    await _botClient.SendTextMessageAsync(
        //        chatId: message.Chat.Id,
        //        text: "Who or Where are you?",
        //        replyMarkup: RequestReplyKeyboard, cancellationToken: Token);
        //}

        //async Task Usage(Message message)
        //{
        //    //const string usage = "Usage:\n" +
        //    //                        "/inline   - send inline keyboard\n" +
        //    //                        "/keyboard - send custom keyboard\n" +
        //    //                        "/photo    - send a photo\n" +
        //    //                        "/request  - request location or contact";
        //    const string usage = "Использование: \n" +
        //                         "/state          - текущее состояние системы\n" +
        //                         "/temperature    - температура объектов";
        //    await _botClient.SendTextMessageAsync(
        //        chatId: message.Chat.Id,
        //        text: usage,
        //        replyMarkup: new ReplyKeyboardRemove(), cancellationToken: Token);
        //}

        // Process Inline Keyboard callback data
        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
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
                        callbackQuery.Id, cancellationToken: Token);

                    //удаление главной клавиатуры
                    await _botClient.EditMessageReplyMarkupAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        messageId: callbackQuery.Message.MessageId,
                        replyMarkup: _camerasKeyboard, cancellationToken: Token);
                    break;

                case "entrance":
                    await ReplyEntranceCam(callbackQuery);
                    break;

                case "yard":
                    await ReplyYardCam(callbackQuery);
                    break;

                case "back":
                    await _botClient.AnswerCallbackQueryAsync(
                        callbackQuery.Id, cancellationToken: Token);
                    await _botClient.EditMessageReplyMarkupAsync(
                         chatId: callbackQuery.Message.Chat.Id,
                         messageId: callbackQuery.Message.MessageId,
                         replyMarkup: _keyboard, cancellationToken: Token);
                    break;

                default:

                    break;
            }

        }

        /// <summary>
        /// Ответ на запрос изображения с камеры входа
        /// </summary>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private async Task ReplyEntranceCam(CallbackQuery callbackQuery)
        {
            //ответ на кнопку
            await _botClient.AnswerCallbackQueryAsync(
                callbackQuery.Id, cancellationToken: Token);
            // Показываем статус отправки фото
            await _botClient.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.UploadPhoto, Token);
            string filePath = string.Empty;
            //отправка фото
            try
            {
                filePath = _camService.GetEntranceCam(out var fileName);
                await using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                await _botClient.SendPhotoAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    photo: new InputOnlineFile(fileStream, fileName),
                    replyMarkup: _camerasKeyboard, cancellationToken: Token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка получения изображения с камеры въезда - {ex.Message}");
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "Невозможно получить изображение с камеры въезда",
                    replyMarkup: _camerasKeyboard, cancellationToken: Token);
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
        }

        /// <summary>
        /// Ответ на запрос изображения с камеры двора
        /// </summary>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private async Task ReplyYardCam(CallbackQuery callbackQuery)
        {
            //ответ на кнопку
            await _botClient.AnswerCallbackQueryAsync(
                callbackQuery.Id, cancellationToken: Token);
            string filePathToDelete = string.Empty;
            // Показываем статус отправки фото
            await _botClient.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.UploadPhoto, Token);
            //отправка фото
            try
            {
                var task = _camService.GetYardCam(Token);
                while (!task.IsCompleted)
                {
                    if (_stoppingCts.Token.IsCancellationRequested)
                    {
                        Token.ThrowIfCancellationRequested();
                    }

                    await _botClient.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.UploadPhoto,
                        Token);
                    await Task.Delay(100, Token);
                }

                var (fileP, fileName) = task.Result;
                //var (fileP, fileName) = await _camService.GetYardCam();
                filePathToDelete = fileP;
                await using FileStream fileStream =
                    new FileStream(fileP, FileMode.Open, FileAccess.Read, FileShare.Read);
                await _botClient.SendPhotoAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    photo: new InputOnlineFile(fileStream, fileName),
                    replyMarkup: _camerasKeyboard, cancellationToken: Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Загрузка фотографии отменена");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "Невозможно получить изображение с камеры двора",
                    replyMarkup: _camerasKeyboard, cancellationToken: Token);
            }

            _logger.LogInformation(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                ? $"Запрос изображения с камеры двора"
                : $"Запрос изображения с камеры двора от {callbackQuery.From.FirstName}");
            try
            {
                File.Delete(filePathToDelete);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Не удалось удалить изображение с камеры из временной директории - {ex.Message}");
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
                callbackQuery.Id, cancellationToken: Token);

            //удаление главной клавиатуры
            await _botClient.EditMessageReplyMarkupAsync(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                replyMarkup: null, cancellationToken: Token);

            //отправка результата + кнопка "Назад"
            await _botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: result,
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "back"),
                }), cancellationToken: Token);
        }

        #region Inline Mode

        private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery)
        {
            _logger.LogInformation($"Received inline query from: {inlineQuery.From.Id}");

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
                cacheTime: 0, cancellationToken: Token);
        }

        //private async Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult)
        //{
        //    Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
        //}

        #endregion

        private async Task UnknownUpdateHandlerAsync(Update update)
        {
            _logger.LogWarning($"Unknown update type: {update.Type}");
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(ErrorMessage);
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Bot service is starting");
            CancellationTokenSource.CreateLinkedTokenSource(Token, cancellationToken);
            Token.Register(() =>
            {
                _logger.LogInformation($"Bot service stopping");
            }, true);

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
                        Token);
                _logger.LogInformation($"Telegram bot started receiveing");
            }, Token);

            try
            {
                _executingTask.Start();
                _logger.LogDebug("Telegram bot initiated");
            }
            catch (Exception)
            {
                _logger.LogError($"Работа бота отменена, не найден токен бота");
            }
            //TODO
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
            _stoppingCts.Cancel();

            try
            {
                _logger.LogDebug($"Try to stop telegram bot receiving");
                _botClient.StopReceiving();
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Error while stopping bot receiving - {ex.Message}");
            }

            await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));

            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("Bot service stoppped");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _executingTask.Dispose();
            Interlocked.Exchange(ref _botClient, null);
            _logger.LogInformation("Dispose");
            _stoppingCts.Dispose();
        }
    }
}
