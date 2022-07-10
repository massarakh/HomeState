using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using TG_Bot.BusinessLayer.Abstract;
using TG_Bot.BusinessLayer.CCUModels;
using NLog;
using TG_Bot.Helpers;
using static TG_Bot.Helpers.Additions;

namespace TG_Bot.BusinessLayer.Concrete
{
    public partial class BotService : IBotService, IHostedService, IDisposable
    {
        private readonly IStateService _stateService;
        private readonly ICamService _camService;
        private readonly IRestService _restService;
        private ITelegramBotClient _botClient;
        private readonly BotHelper _botHelper;
        private Task _executingTask;

        private readonly CancellationTokenSource _stoppingCts =
            new CancellationTokenSource();

        private CancellationToken Token => _stoppingCts.Token;

        //private readonly Outputs _outputs = new Outputs();
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

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
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Управление", "control"),
                    InlineKeyboardButton.WithCallbackData("Камеры", "cameras"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Статистика", "stats"),
                    InlineKeyboardButton.WithCallbackData("Погода", "weather"),
                }
            });

        /// <summary>
        /// Клавиатура для статистики
        /// </summary>
        private InlineKeyboardMarkup _statsKeyboard =>
            new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Сегодня", "today"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Выходные", "weekend"),
                    InlineKeyboardButton.WithCallbackData("Прошл. выходные", "lastweekend"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Неделя", "week"),
                    InlineKeyboardButton.WithCallbackData("Прошл. неделя", "lastweek"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Месяц", "month"),
                    InlineKeyboardButton.WithCallbackData("Прошл. месяц", "lastmonth"),
                },
                //new[]
                //{
                //    InlineKeyboardButton.WithCallbackData("Время года", "season"),
                //},
                //new[]
                //{
                //    InlineKeyboardButton.WithCallbackData("Год", "year"),
                //},
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "back")
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
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Обзор", "overview"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "back")
                }
            });

        /// <summary>
        /// Клавиатура с управлением
        /// </summary>
        private InlineKeyboardMarkup _controlKeyboard =>
            new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Состояния выходов", "output_states")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Всё вкл.", "switches_enable"),
                    InlineKeyboardButton.WithCallbackData("Всё выкл.", "switches_disable"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Конв. вкл.", "relay1_enable"),
                    InlineKeyboardButton.WithCallbackData("Конв. выкл.", "relay1_disable"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Бойлер вкл.", "output1_enable"),
                    InlineKeyboardButton.WithCallbackData("Бойлер выкл.", "output1_disable")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Полы (с/у) вкл.", "output2_enable"),
                    InlineKeyboardButton.WithCallbackData("Полы (с/у) выкл.", "output2_disable")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Спал. №4 вкл.", "output3_enable"),
                    InlineKeyboardButton.WithCallbackData("Спал. №4 выкл.", "output3_disable")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Кухня вкл.", "output4_enable"),
                    InlineKeyboardButton.WithCallbackData("Кухня выкл.", "output4_disable")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", "back")
                }
            });

        public BotService(IStateService stateService, ICamService camService, IConfiguration configuration,
            IRestService restService)
        {
            _stateService = stateService;
            _camService = camService;
            _restService = restService;
            _botHelper = new BotHelper(configuration);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(update.Message),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
                //UpdateType.InlineQuery => BotOnInlineQueryReceived(update.InlineQuery),
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

        /// <summary>
        /// Проверка пользователя
        /// </summary>
        /// <param name="query">Запрос</param>
        /// <returns>Результат проверки</returns>
        private async Task<bool> Authenticate(CallbackQuery query)
        {
            if (!_botHelper.IsAuthorized(Convert.ToInt32(query.From.Id)))
            {
                await _botClient.AnswerCallbackQueryAsync(
                    query.Id, cancellationToken: Token);
                await _botClient.SendTextMessageAsync(
                    chatId: query.Message.Chat.Id,
                    text: "Not authorized", cancellationToken: Token);
                _logger.Error($"Not authorized user - {query.From.Id} ({query.From.FirstName})");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка пользователя
        /// </summary>
        /// <param name="message">Запрос</param>
        /// <returns>Результат проверки</returns>
        private async Task<bool> Authenticate(Message message)
        {
            if (!_botHelper.IsAuthorized(Convert.ToInt32(message.From.Id)))
            {
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Not authorized", cancellationToken: Token);
                _logger.Error($"Not authorized user - {message.From.Id} ({message.From.FirstName})");
                return false;
            }

            return true;
        }

        private async Task BotOnMessageReceived(Message message)
        {
            if (!await Authenticate(message))
                return;
            _logger.Info($"Старт работы с ботом");
            if (message.Type != MessageType.Text)
                return;

            var action = (message.Text.Split(' ').First()) switch
            {
                "/start" => SendInlineKeyboard(message),
                "/info" => SendInlineKeyboard(message),
                _ => SendInlineKeyboard(message)
            };

            //var action = (message.Text.Split(' ').First()) switch
            //{
            //    "/state" => SendInlineKeyboard(message),
            //    "/temperature" => SendInlineKeyboard(message),
            //    //"/keyboard" => SendReplyKeyboard(message),
            //    //"/photo" => SendFile(message),
            //    //"/request" => RequestContactAndLocation(message),
            //    _ => Usage(message)
            //};
            //var action = SendInlineKeyboard(message);
            await action;
        }

        // Send inline keyboard
        // You can process responses in BotOnCallbackQueryReceived handler
        async Task SendInlineKeyboard(Message message)
        {
            await _botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing, Token);
            InlineKeyboardMarkup inlineKeyboard;
            string text;
            switch (message.Text)
            {
                case "/start":
                    text = "Выберите запрос";
                    inlineKeyboard = _keyboard;
                    break;

                case "/info":
                    text = "Статус системы:\n";
                    string result = _botHelper.GetSystemInfo();
                    text += result;
                    inlineKeyboard = _keyboard;
                    break;

                default:
                    text = "Выберите запрос";
                    inlineKeyboard = _keyboard;
                    break;
            }


            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                replyMarkup: inlineKeyboard, cancellationToken: Token);
        }

        /// <summary>
        /// Process Inline Keyboard callback data
        /// </summary>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            if (!await Authenticate(callbackQuery))
                return;
            int stateValue;
            string result;
            switch (callbackQuery.Data)
            {
                #region Запрос состояния

                case "state":
                    var state = await _stateService.LastState();
                    await Answer(callbackQuery, state);
                    _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                        ? $"Запрос состояния"
                        : $"Запрос состояния от {callbackQuery.From.FirstName}");
                    break;

                case "electricity":
                    var electricity = await _stateService.Electricity();
                    await Answer(callbackQuery, electricity);
                    _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                        ? $"Запрос показаний по электричеству"
                        : $"Запрос показаний по электричеству от {callbackQuery.From.FirstName}");
                    break;

                case "heating":
                    var heating = await _stateService.Heating();
                    await Answer(callbackQuery, heating);
                    _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                        ? $"Запрос состояния нагревательных элементов"
                        : $"Запрос состояния нагревательных элементов от {callbackQuery.From.FirstName}");
                    break;

                case "temperature":
                    var temperature = await _stateService.Temperature();
                    await Answer(callbackQuery, temperature);
                    _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                        ? $"Запрос показаний температуры"
                        : $"Запрос показаний температуры от {callbackQuery.From.FirstName}");
                    break;

                #endregion

                #region Камеры

                case "cameras":
                    await AnswerAndSendKeyboard(callbackQuery, _camerasKeyboard);
                    break;

                case "entrance":
                    await ReplyEntranceCam(callbackQuery);
                    break;

                case "yard":
                    await ReplyYardCam(callbackQuery);
                    break;

                case "overview":
                    await ReplyOverviewCam(callbackQuery);
                    break;

                #endregion

                #region Управление

                case "control":
                    await AnswerAndSendKeyboard(callbackQuery, _controlKeyboard);
                    break;

                case "relay1_enable":
                case "relay1_disable":
                    await ReplySwitchOutput(callbackQuery, Outputs.Relay1,
                        callbackQuery.Data.Contains("enable") ? 1 : 0);
                    break;

                case "output1_enable":
                case "output1_disable":
                    stateValue = callbackQuery.Data.Contains("enable") ? 1 : 0;
                    await ReplySwitchOutput(callbackQuery, Outputs.Output1, stateValue);
                    break;

                case "output2_enable":
                case "output2_disable":
                    stateValue = callbackQuery.Data.Contains("enable") ? 1 : 0;
                    await ReplySwitchOutput(callbackQuery, Outputs.Output2, stateValue);
                    break;

                case "output3_enable":
                case "output3_disable":
                    stateValue = callbackQuery.Data.Contains("enable") ? 1 : 0;
                    await ReplySwitchOutput(callbackQuery, Outputs.Output3, stateValue);
                    break;

                case "output4_enable":
                case "output4_disable":
                    stateValue = callbackQuery.Data.Contains("enable") ? 1 : 0;
                    await ReplySwitchOutput(callbackQuery, Outputs.Output4, stateValue);
                    break;

                case "output_states":
                    await ReplyControllerState(callbackQuery);
                    break;

                #endregion

                #region Статистика

                case "stats":
                    await AnswerAndSendKeyboard(callbackQuery, _statsKeyboard);
                    break;

                case "today":
                    result = await _stateService.GetStatistics(StatType.Day);

                    await ReplyAndLog(result, callbackQuery, () =>
                    {
                        _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                            ? $"Запрос статистики за день"
                            : $"Запрос статистики за день от {callbackQuery.From.FirstName}");
                    });
                    break;

                case "weekend":
                    result = await _stateService.GetStatistics(StatType.Weekend);

                    await ReplyAndLog(result, callbackQuery, () =>
                    {
                        _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                            ? $"Запрос статистики за выходные"
                            : $"Запрос статистики за выходные от {callbackQuery.From.FirstName}");
                    });
                    break;

                case "lastweekend":
                    result = await _stateService.GetStatistics(StatType.LastWeekend);

                    await ReplyAndLog(result, callbackQuery, () =>
                    {
                        _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                            ? $"Запрос статистики за прошлые выходные"
                            : $"Запрос статистики за прошлые выходные от {callbackQuery.From.FirstName}");
                    });
                    break;

                case "week":
                    result = await _stateService.GetStatistics(StatType.Week);
                    await ReplyAndLog(result, callbackQuery, () =>
                    {
                        _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                            ? $"Запрос статистики за неделю"
                            : $"Запрос статистики за неделю от {callbackQuery.From.FirstName}");
                    });
                    break;

                case "lastweek":
                    result = await _stateService.GetStatistics(StatType.LastWeek);

                    await ReplyAndLog(result, callbackQuery, () =>
                    {
                        _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                            ? $"Запрос статистики за прошлую неделю"
                            : $"Запрос статистики за прошлую неделю от {callbackQuery.From.FirstName}");
                    });
                    break;  

                case "month":
                    result = await _stateService.GetStatistics(StatType.Month);

                    await ReplyAndLog(result, callbackQuery, () =>
                    {
                        _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                            ? $"Запрос статистики за месяц"
                            : $"Запрос статистики за месяц от {callbackQuery.From.FirstName}");
                    });
                    break;

                case "lastmonth":
                    result = await _stateService.GetStatistics(StatType.LastMonth);

                    await ReplyAndLog(result, callbackQuery, () =>
                    {
                        _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                            ? $"Запрос статистики за прошлый месяц"
                            : $"Запрос статистики за прошлый месяц от {callbackQuery.From.FirstName}");
                    });
                    break;

                case "season":
                    result = await _stateService.GetStatistics(StatType.Season);

                    await ReplyAndLog(result, callbackQuery, () =>
                    {
                        _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                            ? $"Запрос статистики за время года"
                            : $"Запрос статистики за время года от {callbackQuery.From.FirstName}");
                    });
                    break;

                case "year":
                    result = await _stateService.GetStatistics(StatType.Year);

                    await ReplyAndLog(result, callbackQuery, () =>
                    {
                        _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                            ? $"Запрос статистики за год"
                            : $"Запрос статистики за год от {callbackQuery.From.FirstName}");
                    });
                    break;

                #endregion

                #region Погода

                case "weather":
                    var weather = await _stateService.GetWeather();
                    await Answer(callbackQuery, weather);
                    break;

                #endregion

                case "back":
                    await AnswerAndSendKeyboard(callbackQuery, _keyboard);
                    break;

                case "switches_enable":
                case "switches_disable":
                    stateValue = callbackQuery.Data.Contains("enable") ? 1 : 0;
                    await ReplySwitchAllOutputs(callbackQuery, stateValue);
                    break;

                default:

                    break;
            }
        }


        //TODO надо переписать и привести к общему виду в версии 2.0

        #region Inline Mode

        private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery)
        {
            _logger.Info($"Received inline query from: {inlineQuery.From.Id}");
            InlineQueryResultBase[] results =
            {
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
            _logger.Warn($"Unknown update type: {update.Type}");
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException =>
                    $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.Error(ErrorMessage);
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Debug($"Bot service is starting");
            CancellationTokenSource.CreateLinkedTokenSource(Token, cancellationToken);
            Token.Register(() => { _logger.Info($"Bot service stopping"); }, true);
            string token = _botHelper.GetBotToken();
            if (string.IsNullOrEmpty(token))
            {
                _stoppingCts.Cancel();
            }
            else
            {
                _botClient = new TelegramBotClient(token);
            }

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            _executingTask = new Task(() =>
            {
                _botClient.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                    Token);
                _logger.Info($"Telegram bot started receiveing");
            }, Token);

            try
            {
                _executingTask.Start();
                _logger.Debug("Telegram bot initiated");
            }
            catch (OperationCanceledException)
            {
                _logger.Debug($"Работа бота отменена");
            }
            catch (Exception)
            {
                _logger.Error($"Работа бота отменена, не найден токен бота");
            }

            await _executingTask;
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                _logger.Debug($"Try to stop telegram bot receiving");
                _stoppingCts.Cancel();
            }
            catch (Exception ex)
            {
                _logger.Debug($"Error while stopping bot receiving - {ex.Message}");
            }

            await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));

            cancellationToken.ThrowIfCancellationRequested();
            _logger.Info("Bot service stoppped");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _executingTask.Dispose();
            Interlocked.Exchange(ref _botClient, null);
            _logger.Info("Dispose");
            _stoppingCts.Dispose();
        }
    }

    
}