using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Models;
using static TelegramBot.Models.ExchangeRates;

namespace TelegramBot
{
    public class TelegramBotService
    {
        public List<TelegramUser> Users { get; private set; }

        private readonly string _token = "token";
        private TelegramBotClient _tbClient;
        private PrivatBankAPIService _privatBankAPIService;
        private Action _receivedMessage;
        private string _nickNameBot;

        public TelegramBotService(Action receivedMessage)
        {
            _receivedMessage = receivedMessage;
            Users = new List<TelegramUser>();
            _tbClient = new TelegramBotClient(_token);
            _privatBankAPIService = new PrivatBankAPIService();

            using CancellationTokenSource cts = new();
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };

            _tbClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingError,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );
            _nickNameBot = "Support";
            GetSystemInfoAsync();
        }

        public async Task SendTextMessageAsync(TelegramUser user, string message)
        {
            Message sentMessage = await _tbClient.SendTextMessageAsync(
                    chatId: user.ChatID,
                    text: message);
            user.AddMessage($"{_nickNameBot}: {sentMessage.Text}");
            _receivedMessage();
        }

        private async void GetSystemInfoAsync()
        {
            User me = await _tbClient.GetMeAsync();
            _nickNameBot = me.Username ?? "Support";
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            long chatId = message.Chat.Id;
            string firstName = message.From?.FirstName ?? "Unknown User";

            TelegramUser? person;

            person = Users.Find(p => p.ChatID == chatId);
            if (person == null)
            {
                person = new TelegramUser(firstName, chatId);
                Users.Add(person);
            }
            person.AddMessage($"{person.FirstName}: {messageText}");

            Message sentMessage;

            Debug.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
            if (messageText == "/start")
            {
                sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $""""
                Привет {firstName}!
                Я Бот для получения курса валют ПриватБанка к гривне.
                Поддерживаемые валюты: {String.Join(",", _privatBankAPIService.SupportedCurrencies)}.
                Для получения курса валюты на текущую дату достаточно ввести тикер, например "USD".
                Для получения курса валюты на произвольную дату, нужно ввести тикер и дату в формате день.месяц.год. Например: "USD 12.05.2023".
                """",
                cancellationToken: cancellationToken);

                person.AddMessage($"{_nickNameBot}: {sentMessage.Text}");
            }
            else
            {
                string[] parts = messageText.Split(" ");
                if (parts.Length > 1)
                {
                    if (_privatBankAPIService.DateCheck(parts[1]))
                    {
                        ExchangeRates? exchangeRates = await _privatBankAPIService.RequestAsync(parts[1]);
                        messageText = GetMessageResponse(exchangeRates, parts[0]);
                    }
                    else
                    {
                        messageText = "Извините, но дата введена не верно!";
                    }
                }
                else
                {
                    ExchangeRates? exchangeRates = await _privatBankAPIService.RequestAsync();
                    messageText = GetMessageResponse(exchangeRates, messageText);
                }
                // Echo received message text
                sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: messageText,
                    cancellationToken: cancellationToken);
                person.AddMessage($"{_nickNameBot}: {sentMessage.Text}");
            }
            _receivedMessage();
        }

        private string GetMessageResponse(ExchangeRates? exchangeRates, string currency)
        {
            string messageText;
            if (exchangeRates == null)
            {
                messageText = "Извините, но запрос не корректный!";
            }
            else
            {
                ExchangeRate? exchangeRate = exchangeRates.exchangeRate?.Find(ex => ex.Currency == currency.ToUpper());
                if (exchangeRate == null)
                {
                    messageText = $"Извините, но на дату \"{exchangeRates.Date}\" отсутствует курс по выбранной валюте!";
                }
                else
                {
                    messageText = $""""
                                    Дата: {exchangeRates.Date}
                                    Базовая валюта: {exchangeRate.BaseCurrency}
                                    Валюта сделки: {exchangeRate.Currency}
                                    Курс продажи ПриватБанка: {exchangeRate.SaleRate}
                                    Курс покупки ПриватБанка: {exchangeRate.PurchaseRate}
                                    Курс продажи НБУ: {exchangeRate.SaleRateNB}
                                    Курс покупки НБУ: {exchangeRate.PurchaseRateNB}
                                    """";
                }
            }
            return messageText;
        }

        private Task HandlePollingError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Debug.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
