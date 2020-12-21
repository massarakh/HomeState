﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using TG_Bot.DAL;

namespace TG_Bot.BusinessLayer
{
    public class BotService : IBotService, IHostedService, IDisposable
    {
        private readonly ILogger<BotService> _logger;
        private readonly IStateService _stateService;
        private readonly ITelegramBotClient _botClient;
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts =
            new CancellationTokenSource();

        public BotService(ILogger<BotService> logger, IStateService stateService)
        {
            _logger = logger;
            _stateService = stateService;
            _botClient = new TelegramBotClient("1456907202:AAF50prIIafWzAKJlN2ghWit9ViKZWVhOVM");
            _logger.LogInformation("Telegram bot started, waithing for messages");
            _botClient.OnMessage += Bot_OnMessage;
            _botClient.StartReceiving();
        }

        private async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
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
                            text: "Температура в спальне: "+state.TemperatureLivingRoom.ToString()+ "°C",
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

        //var inlineKeyboard = new InlineKeyboardMarkup(new[]
        //{
        //    // first row
        //    new []
        //    {
        //        InlineKeyboardButton.WithCallbackData("1.1", "11"),
        //        InlineKeyboardButton.WithCallbackData("1.2", "12"),
        //    },
        //    // second row
        //    new []
        //    {
        //        InlineKeyboardButton.WithCallbackData("2.1", "21"),
        //        InlineKeyboardButton.WithCallbackData("2.2", "22"),
        //    }
        //});
        //await Bot.SendTextMessageAsync(
        //    chatId: message.Chat.Id,
        //text: "Choose",
        //replyMarkup: inlineKeyboard

        //);

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Bot service is starting.");

            _stoppingCts.Token.Register(() =>
                _logger.LogDebug($"Bot service stopping"));
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bot service stoppped");
            _botClient.StopReceiving();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _stoppingCts.Cancel();
        }
    }
}
