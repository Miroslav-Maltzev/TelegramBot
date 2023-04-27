using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var botClient = new TelegramBotClient("6074220983:AAHYOWu0B16ZRaXbbavJPBvKkA9bKY11GvE");

using CancellationTokenSource cts = new();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};

var values = new Dictionary<long, Stack<int>>();

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();



Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    string retText;

    if (int.TryParse(messageText, out var input) || messageText == "+" || messageText == "-" || messageText == "*" || messageText == "*" || messageText == "/start")
    {
        if (messageText == "/start")
        {
            retText = "Hi! Please, write first number.";
        }
        else if (!values.ContainsKey(chatId))
        {
            Stack<int> stack = new();
            stack.Push(input);
            values.Add(chatId, stack);
            
            retText = "Please, write second number.";
        }
        else if (values[chatId].Count == 1)
        {
            Stack<int> st = values[chatId];
            st.Push(input);
            retText = "Please, write mathemathic sign.";
        }
        else
        {
            int num2 = values[chatId].Pop();
            int num1 = values[chatId].Pop();

            string sign = messageText;

            int res = Calc(num1, num2, sign);
            values.Remove(chatId);
            retText = $"Good! Your answer is {res}. Write first number.";
        }

        Console.WriteLine(retText);


        // Echo received message text
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: retText,
            cancellationToken: cancellationToken);
    }


    
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}


int Calc(int num1, int num2, string sign)
{
    switch (sign)
    {
        case "+":
            return num1 + num2;
        case "-":
            return num1 - num2;
        case "*":
            return num1 * num2;
        case "/":
            return num1 * num2;
        default: 
            return default;
    }
}