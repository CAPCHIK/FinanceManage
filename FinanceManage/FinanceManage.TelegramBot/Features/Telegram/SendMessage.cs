using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinanceManage.TelegramBot.Features.Telegram
{
    public class SendMessage
    {
        public record Command(
            long ChatId,
            string Text,
            int ReplyToMessageId,
            ParseMode ParseMode = ParseMode.Default,
            IReplyMarkup ReplyMarkup = default) : IRequest;

        public class Handler : IRequestHandler<Command>
        {
            private static ReplyKeyboardMarkup replyKeyboardMarkup;
            private readonly ITelegramBotClient telegramBotClient;

            public Handler(ITelegramBotClient telegramBotClient)
            {
                this.telegramBotClient = telegramBotClient;
            }
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                replyKeyboardMarkup ??= new ReplyKeyboardMarkup(HandleTopLevelCommandMessage.TopLevelCommands.Select(c => new KeyboardButton[] { new KeyboardButton(c) }), resizeKeyboard: true);
                await telegramBotClient.SendTextMessageAsync(request.ChatId,
                                                             request.Text,
                                                             parseMode: request.ParseMode,
                                                             replyToMessageId: request.ReplyToMessageId,
                                                             replyMarkup: request.ReplyMarkup,
                                                             cancellationToken: cancellationToken);
                return default;
            }
        }
    }
}
