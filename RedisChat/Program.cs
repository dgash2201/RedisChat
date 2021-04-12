using System;
using System.Threading;
using StackExchange.Redis;

namespace RedisChat
{
    class Program
    {
        private const string _exitCommand = "/exit";
        private const ConsoleColor _defaultColor = ConsoleColor.Gray;
        private const ConsoleColor _userColor = ConsoleColor.Green;
        private const ConsoleColor _companionColor = ConsoleColor.Red;

        static void PrintResponse(RedisValue message)
        {
            Console.ForegroundColor = _companionColor;
            Console.WriteLine(message);

            Console.ForegroundColor = _defaultColor;
        }

        static void PrintMessageHistory(RedisValue[] messageHistory, string userName)
        {
            foreach (var value in messageHistory)
            {
                var sender = value.ToString().Split(":")[0];

                Console.ForegroundColor = sender == userName ? _userColor : _companionColor;
                Console.WriteLine(value);
            }
        }

        static void Main(string[] args)
        {
            var configuration = new ConfigurationOptions();
            configuration.EndPoints.Add("localhost", 6379);

            var redis = ConnectionMultiplexer.Connect(configuration);

            Console.WriteLine("Введите имя пользователя: ");
            var userName = Console.ReadLine();

            Console.WriteLine("Введите имя собеседника: ");
            var companionName = Console.ReadLine();

            Console.WriteLine();
            Console.WriteLine($"Чат c {companionName}");
            Console.WriteLine($"Для выхода из чата наберите {_exitCommand}");
            Console.WriteLine();

            var chat = new Chat(userName, companionName, redis, PrintResponse);
            var messageHistory = chat.GetMessageHistory();

            PrintMessageHistory(messageHistory, userName);

            while (true)
            {
                Console.ForegroundColor = _userColor;
                var toSend = Console.ReadLine();

                if (toSend == _exitCommand) break;

                chat.Send(toSend);
            }
        }
    }
}
