using System;
using UnityEngine;

namespace Edgegap.NakamaServersPlugin
{
    public static class Logger
    {
        public static void _Log<T>(T message)
        {
            Debug.Log(_FormatLog(message));
        }

        public static void _Warn<T>(T message)
        {
            Debug.LogWarning(_FormatLog(message));
        }

        public static void _Error<T>(T message)
        {
            Debug.LogError(_FormatLog(message));
        }

        public static string _FormatLog<T>(T message) =>
            $"{DateTime.UtcNow} EdgegapServersNakamaPlugin | {_ToStringOrNull(message)}";

        public static string _ToStringOrNull<T>(T value)
        {
            return value is null ? "null" : value.ToString();
        }
    }
}
