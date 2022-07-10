using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using TG_Bot.BusinessLayer.CCUModels;

namespace TG_Bot.BusinessLayer.Concrete
{
    public partial class BotService
    {
        /// <summary>
        /// Удаление клавиатуры
        /// Отправка сообщения
        /// Логирование
        /// </summary>
        /// <param name="result">Данные для отправки</param>
        /// <param name="callbackQuery">Запрос</param>
        /// <param name="action">Метод для логирования</param>
        /// <returns></returns>
        private async Task ReplyAndLog(string result, CallbackQuery callbackQuery, Action action)
        {
            //удаление клавиатуры у предыдущего сообщения
            await RemoveKeyboardFromPrevious(callbackQuery);

            await _botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: result,
                replyMarkup: _statsKeyboard, cancellationToken: Token, parseMode: ParseMode.Html);
            action.Invoke();
        }

        /// <summary>
        /// Удаление клавиатуры у предыдущего сообщения
        /// </summary>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private async Task RemoveKeyboardFromPrevious(CallbackQuery callbackQuery)
        {
            await _botClient.EditMessageReplyMarkupAsync(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                replyMarkup: null, cancellationToken: Token);
        }

        /// <summary>
        /// Ответ и отправка нужной клавиатуры
        /// </summary>
        /// <param name="callbackQuery"></param>
        /// <param name="keyboard"></param>
        /// <returns></returns>
        private async Task AnswerAndSendKeyboard(CallbackQuery callbackQuery, InlineKeyboardMarkup keyboard)
        {
            await _botClient.AnswerCallbackQueryAsync(
                callbackQuery.Id, cancellationToken: Token);

            //удаление главной клавиатуры
            await _botClient.EditMessageReplyMarkupAsync(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                replyMarkup: keyboard, cancellationToken: Token);
        }

        /// <summary>
        /// Ответ переключения состояния выхода
        /// </summary>
        /// <param name="callbackQuery">Запрос</param>
        /// <param name="output">Выход для переключения</param>
        /// <param name="stateValue">Новое состояние</param>
        /// <returns>Результат переключения</returns>
        private async Task ReplySwitchOutput(CallbackQuery callbackQuery, Output output, int stateValue)
        {
            try
            {
                //ответ о принятии сообщения
                await _botClient.AnswerCallbackQueryAsync(
                    callbackQuery.Id, cancellationToken: Token);

                // Показываем статус отправки фото
                await _botClient.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.Typing, Token);

                //переключение выхода
                var result = _restService.SwitchOutput(new CommandRequest
                {
                    Command = RestService.SwitchCommand,
                    Output = output,
                    State = stateValue
                });

                //удаление клавиатуры у предыдущего сообщения
                await RemoveKeyboardFromPrevious(callbackQuery);

                //ответ
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: result,
                    replyMarkup: _controlKeyboard, cancellationToken: Token);
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка - {ex.Message}");
                //удаление клавиатуры у предыдущего сообщения
                await RemoveKeyboardFromPrevious(callbackQuery);
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "Ошибка переключения состояния выхода",
                    replyMarkup: _controlKeyboard, cancellationToken: Token);
            }

            _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                ? $"Переключение состояния выхода {output.Name}"
                : $"Переключение состояния выхода {output.Name} от {callbackQuery.From.FirstName}");
        }

        /// <summary>
        /// Ответ переключения состояния всех выходов
        /// </summary>
        /// <param name="callbackQuery">Запрос</param>
        /// <param name="stateValue">Новое состояние</param>
        /// <returns></returns>
        private async Task ReplySwitchAllOutputs(CallbackQuery callbackQuery, int stateValue)
        {
            try
            {
                //ответ о принятии сообщения
                await _botClient.AnswerCallbackQueryAsync(
                    callbackQuery.Id, cancellationToken: Token);

                // Показываем статус отправки фото
                await _botClient.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.Typing, Token);

                var result = _restService.SwitchAll(stateValue);

                //удаление клавиатуры у предыдущего сообщения
                await RemoveKeyboardFromPrevious(callbackQuery);

                //ответ
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: result,
                    replyMarkup: _controlKeyboard, cancellationToken: Token);
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка - {ex.Message}");
                //удаление клавиатуры у предыдущего сообщения
                await RemoveKeyboardFromPrevious(callbackQuery);
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "Ошибка переключения состояний",
                    replyMarkup: _controlKeyboard, cancellationToken: Token);
            }

            _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                ? $"Переключение состояния выходов"
                : $"Переключение состояния выходов от {callbackQuery.From.FirstName}");
        }

        /// <summary>
        /// Ответ на запрос состояния контроллера
        /// </summary>
        /// <param name="callbackQuery">Запрос</param>
        /// <returns>Результат запроса</returns>
        private async Task ReplyControllerState(CallbackQuery callbackQuery)
        {
            try
            {
                //ответ о принятии сообщения
                await _botClient.AnswerCallbackQueryAsync(
                    callbackQuery.Id, cancellationToken: Token);

                //переключение выхода
                var result = _restService.GetState();

                //удаление клавиатуры у предыдущего сообщения
                await RemoveKeyboardFromPrevious(callbackQuery);

                //ответ
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: result,
                    replyMarkup: _controlKeyboard, cancellationToken: Token, parseMode: ParseMode.Html);
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка - {ex.Message}");
                //удаление клавиатуры у предыдущего сообщения
                await RemoveKeyboardFromPrevious(callbackQuery);
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "Ошибка получения состояния контроллера",
                    replyMarkup: _controlKeyboard, cancellationToken: Token);
            }

            _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                ? $"Получение состояния контроллера"
                : $"Получение состояния контроллера от {callbackQuery.From.FirstName}");
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
                await using FileStream fileStream =
                    new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                //удаление клавиатуры у предыдущего сообщения
                await RemoveKeyboardFromPrevious(callbackQuery);
                await _botClient.SendPhotoAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    photo: new InputOnlineFile(fileStream, fileName),
                    replyMarkup: _camerasKeyboard, cancellationToken: Token);
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка получения изображения с камеры въезда - {ex.Message}");
                //удаление клавиатуры у предыдущего сообщения
                await RemoveKeyboardFromPrevious(callbackQuery);
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "Невозможно получить изображение с камеры въезда",
                    replyMarkup: _camerasKeyboard, cancellationToken: Token);
            }

            _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                ? $"Запрос изображения с камеры въезда"
                : $"Запрос изображения с камеры въезда от {callbackQuery.From.FirstName}");
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Не удалось удалить изображение с камеры из временной директории - {ex.Message}");
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
                var task = await _camService.GetFfmpegCam(Token, "YardCam");
                //while (!task.IsCompleted)
                //{
                //    if (_stoppingCts.Token.IsCancellationRequested)
                //    {
                //        Token.ThrowIfCancellationRequested();
                //    }
                //    await Task.Delay(300, Token);
                //}
                await _botClient.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.UploadPhoto,
                    Token);

                var (fileP, fileName) = task;
                filePathToDelete = fileP;
                await using FileStream fileStream =
                    new FileStream(fileP, FileMode.Open, FileAccess.Read, FileShare.Read);

                //удаление клавиатуры у предыдущего сообщения
                await RemoveKeyboardFromPrevious(callbackQuery);
                await _botClient.SendPhotoAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    photo: new InputOnlineFile(fileStream, fileName),
                    replyMarkup: _camerasKeyboard, cancellationToken: Token);
            }
            catch (OperationCanceledException)
            {
                _logger.Warn($"Загрузка фотографии отменена");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                //удаление клавиатуры у предыдущего сообщения
                await RemoveKeyboardFromPrevious(callbackQuery);
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "Невозможно получить изображение с камеры двора",
                    replyMarkup: _camerasKeyboard, cancellationToken: Token);
            }

            _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                ? $"Запрос изображения с камеры двора"
                : $"Запрос изображения с камеры двора от {callbackQuery.From.FirstName}");
            try
            {
                System.IO.File.Delete(filePathToDelete);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Не удалось удалить изображение с камеры из временной директории - {ex.Message}");
            }
        }

        /// <summary>
        /// Ответ на запрос изображения с камеры обзора
        /// </summary>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private async Task ReplyOverviewCam(CallbackQuery callbackQuery)
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
                var task = await _camService.GetFfmpegCam(Token, "OverviewCam");
                //while (!task.IsCompleted)
                //{
                //    if (_stoppingCts.Token.IsCancellationRequested)
                //    {
                //        Token.ThrowIfCancellationRequested();
                //    }

                //    await _botClient.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.UploadPhoto,
                //        Token);
                //    await Task.Delay(300, Token);
                //}

                var (fileP, fileName) = task;
                filePathToDelete = fileP;
                await using FileStream fileStream =
                    new FileStream(fileP, FileMode.Open, FileAccess.Read, FileShare.Read);

                //удаление клавиатуры у предыдущего сообщения
                await RemoveKeyboardFromPrevious(callbackQuery);
                await _botClient.SendPhotoAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    photo: new InputOnlineFile(fileStream, fileName),
                    replyMarkup: _camerasKeyboard, cancellationToken: Token);
            }
            catch (OperationCanceledException)
            {
                _logger.Warn($"Загрузка фотографии отменена");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                //удаление клавиатуры у предыдущего сообщения
                await RemoveKeyboardFromPrevious(callbackQuery);
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "Невозможно получить изображение с камеры обзора",
                    replyMarkup: _camerasKeyboard, cancellationToken: Token);
            }

            _logger.Info(string.IsNullOrEmpty(callbackQuery.From.FirstName)
                ? $"Запрос изображения с камеры обзора"
                : $"Запрос изображения с камеры обзора от {callbackQuery.From.FirstName}");
            try
            {
                System.IO.File.Delete(filePathToDelete);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Не удалось удалить изображение с камеры из временной директории - {ex.Message}");
            }
        }

        /// <summary>
        /// Ответ
        /// </summary>
        /// <param name="callbackQuery">Callback запрос</param>
        /// <param name="result">Строка для ответа</param>
        /// <param name="back"></param>
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
            //await _botClient.SendTextMessageAsync(
            //    chatId: callbackQuery.Message.Chat.Id,
            //    text: result,
            //    replyMarkup: new InlineKeyboardMarkup(new[]
            //    {
            //        InlineKeyboardButton.WithCallbackData("Назад", "back"),
            //    }), cancellationToken: Token,
            //    parseMode: ParseMode.Html);

            await _botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: result,
                replyMarkup: _keyboard, cancellationToken: Token,
                parseMode: ParseMode.Html);
        }
    }
}