using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FixMath.NET;

namespace VelcroPhysics.Shared.Contracts
{
    public static class Contract
    {
        [Conditional("Debug")]
        public static void Requires(bool condition, string message)
        {
            if (condition)
                return;

            message = BuildMessage("REQUIRED", message);
            throw new RequiredException(message);
        }

        [Conditional("Debug")]
        public static void Warn(bool condition, string message)
        {
            message = BuildMessage("WARNING", message);
            Debug.WriteLineIf(!condition, message);
        }

        [Conditional("Debug")]
        public static void Ensures(bool condition, string message)
        {
            if (condition)
                return;

            message = BuildMessage("ENSURANCE", message);
            throw new EnsuresException(message);
        }

        [Conditional("Debug")]
        public static void RequireForAll<T>(IEnumerable<T> value, Predicate<T> check)
        {
            foreach (var item in value) Requires(check(item), "Failed on: " + item);
        }

        [Conditional("Debug")]
        public static void Fail(string message)
        {
            message = BuildMessage("FAILURE", message);
            throw new RequiredException(message);
        }

        private static string BuildMessage(string type, string message)
        {
            var stackTrace = string.Join(Environment.NewLine,
                Environment.StackTrace.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .Skip(3));
            return message == null ? string.Empty : type + ": " + message + Environment.NewLine + stackTrace;
        }
    }
}