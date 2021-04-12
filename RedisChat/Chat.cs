using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisChat
{
    class Chat
    {
        private readonly string _currentUser;
        private readonly string _companion;
        private ConnectionMultiplexer _redis;
        private readonly string _forthChannel;
        private readonly string _backChannel;
        private readonly ISubscriber _subscriber;
        private readonly string _messagesHistoryKey;

        public Chat(string currentUser, string companion, ConnectionMultiplexer redis, Action<RedisValue> responseHandler)
        {
            _currentUser = currentUser;
            _companion = companion;
            _redis = redis;
            _forthChannel = $"channel:{_currentUser + _companion}";
            _backChannel = $"channel:{_companion + _currentUser}";
            _messagesHistoryKey = $"{_currentUser}-{_companion}";

            _subscriber = _redis.GetSubscriber();
            _subscriber.Subscribe(_backChannel, (channel, message) =>
            {
                responseHandler(message);
                SaveMessage(message);
            });
        }

        public void Send(string message)
        {
            SaveMessage($"{_currentUser}:{message}");
            _subscriber.Publish(_forthChannel, $"{_currentUser}:{message}");
        }
        
        public RedisValue[] GetMessageHistory()
        {
            var db = _redis.GetDatabase();
            return db.ListRange(_messagesHistoryKey);
        }

        private void SaveMessage(string message)
        {
            var db = _redis.GetDatabase();
            db.ListRightPush(_messagesHistoryKey, message);
        }
    }
}
