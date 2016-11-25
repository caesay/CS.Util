using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util
{
    public class DateTimeEx
    {
        /// <summary>
        /// Construct a DateTime object from the specified epoch timestamp in milliseconds.
        /// This is commonly used as a timestamp in Java.
        /// </summary>
        /// <param name="milliseconds">Milliseconds since epoch</param>
        /// <returns>A UTC Datetime object</returns>
        public static DateTime FromEpochTimestampMilliseconds(long milliseconds)
        {
            // Java timestamp is millisecods past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Math.Round(milliseconds / 1000d)).ToLocalTime();
            return dtDateTime;
        }

        /// <summary>
        /// Construct a DateTime object from the specified epoch timestamp in seconds.
        /// This is commonly used as a timestamp in Unix.
        /// </summary>
        /// <param name="milliseconds">Seconds since epoch</param>
        /// <returns>A UTC Datetime object</returns>
        public static DateTime FromEpochTimestampSeconds(long seconds)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(seconds).ToLocalTime();
            return dtDateTime;
        }

        /// <summary>
        /// Construct a DateTime object from the specified epoch timestamp by guessing whether its in milliseconds or seconds.
        /// If the exact format is known it is better to use <see cref="FromEpochTimestampMilliseconds(long)"/> or <see cref="FromEpochTimestampSeconds(long)"/> 
        /// </summary>
        /// <param name="milliseconds">Time value since epoch</param>
        /// <returns>A UTC Datetime object</returns>
        public static DateTime FromEpochTimestamp(long value)
        {
            return value.ToString().Length > 12
                ? FromEpochTimestampMilliseconds(value)
                : FromEpochTimestampSeconds(value);
        }
    }

    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts the current DateTime object to an epoch timestamp
        /// </summary>
        /// <param name="dateTime">DateTime to convert</param>
        /// <param name="seconds">If true returns value in seconds, if not milliseconds.</param>
        /// <returns>A timestamp in seconds or milliseconds since epoch</returns>
        public static long ToEpochTimestamp(this DateTime dateTime, bool seconds = true)
        {
            var timespan = TimeZoneInfo.ConvertTimeToUtc(dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return seconds
                ? (long)timespan.TotalSeconds
                : (long)timespan.TotalMilliseconds;
        }
    }
}
