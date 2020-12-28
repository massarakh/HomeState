using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using Monitor = TG_Bot.monitoring.Monitor;

namespace TG_Bot.BusinessLayer
{
    public class BotService : IBotService, IHostedService, IDisposable
    {
        private readonly ILogger<BotService> _logger;
        private readonly IStateService _stateService;
        private ITelegramBotClient _botClient;
        private Task _executingTask;
        private CancellationTokenSource _stoppingCts =
            new CancellationTokenSource();

        public InlineKeyboardMarkup Keyboard =>
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
                }
            });

        public BotService(ILogger<BotService> logger, IStateService stateService)
        {
            _logger = logger;
            _stateService = stateService;
            _botClient = new TelegramBotClient("1456907202:AAF50prIIafWzAKJlN2ghWit9ViKZWVhOVM");
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
                    inlineKeyboard = Keyboard;
                    break;

                default:
                    inlineKeyboard = Keyboard;
                    break;
            }


            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Выберите запрос",
                replyMarkup: inlineKeyboard
            );
        }

        async Task SendInlineKeyboard(long ChatId)
        {
            await _botClient.SendChatActionAsync(ChatId, ChatAction.Typing);
            await _botClient.SendTextMessageAsync(
                chatId: ChatId,
                text: "Выберите запрос",
                replyMarkup: Keyboard
            );
        }

        async Task SendFile(Message message)
        {
            await _botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            const string filePath = @"Files/tux.png";
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
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
            switch (callbackQuery.Data)
            {
                case "state":
                    Monitor state = await _stateService.LastState();
                    string result = state.ToStat();
                    await _botClient.AnswerCallbackQueryAsync(
                        callbackQuery.Id
                    );

                    await _botClient.EditMessageReplyMarkupAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        messageId: callbackQuery.Message.MessageId,
                        replyMarkup: null);

                    await _botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: result,
                        replyMarkup: new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Назад","back"),
                        }));

                    _logger.LogInformation("Запрос состояния");
                    break;

                case "electricity":
                    // ответ
                    await _botClient.AnswerCallbackQueryAsync(
                        callbackQuery.Id
                    );
                    // результат
                    // кнопка назад от результата
                    // удалить кнопки меню от предыдущего сообщения
                    await _botClient.EditMessageReplyMarkupAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        messageId: callbackQuery.Message.MessageId,
                        replyMarkup: null);

                    await _botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: "Ololo",
                        replyMarkup: new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Назад","back"),
                        }));
                    break;

                case "heating":

                    break;

                case "temperature":

                    break;

                case "back":
                    await _botClient.AnswerCallbackQueryAsync(
                        callbackQuery.Id
                    );
                    await _botClient.EditMessageReplyMarkupAsync(
                         chatId: callbackQuery.Message.Chat.Id,
                         messageId: callbackQuery.Message.MessageId,
                         replyMarkup: Keyboard);
                    break;

                default:

                    break;
            }
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

        private async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            //https://github.com/TelegramBots/Telegram.Bot.Examples/blob/master/Telegram.Bot.Examples.Polling/Program.cs
            if (e.Message.Text != null)
            {
                _logger.LogInformation($"Received a text message in chat {e.Message.Chat.Id}.");
                InlineKeyboardMarkup inlineKeyboard = null;
                switch (e.Message.Text)
                {
                    case "/start":
                        //отправить клавиатуру выбора
                        //старт
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Состояние","state"),
                            InlineKeyboardButton.WithCallbackData("Последнее значение","last"),
                        });
                        await _botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat.Id,
                            text: "Запрос",
                            replyMarkup: inlineKeyboard);
                        break;

                    case "/state":
                        var state = await _stateService.LastState();
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Состояние","state"),
                            InlineKeyboardButton.WithCallbackData("Последнее значение","last"),
                        });
                        await _botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat.Id,
                            text: "Температура в спальне: " + state.TemperatureLivingRoom.ToString() + "°C",
                            replyMarkup: inlineKeyboard);
                        break;

                    case "last":

                        break;

                    default:

                        break;
                }


                //await _botClient.SendTextMessageAsync(
                //    chatId: e.Message.Chat,
                //    text: "You said:\n" + e.Message.Text
                //);
            }
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Bot service is starting.");

            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _stoppingCts.Token.Register(() =>
                _logger.LogDebug($"Bot service stopping"));

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            _executingTask = new Task(() =>
            {
                _botClient.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                        _stoppingCts.Token);
                _logger.LogInformation($"Telegram bot started receiveing");
            }, _stoppingCts.Token);
            _executingTask.Start();
            //_botClient.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), _stoppingCts.Token);
            _logger.LogInformation("Telegram bot started, waiting for messages");
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
            Interlocked.Exchange(ref _botClient, null);
            //Dispose
        }
    }
}
