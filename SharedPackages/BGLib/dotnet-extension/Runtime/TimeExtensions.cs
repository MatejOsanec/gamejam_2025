using System;
using System.Runtime.CompilerServices;

public static class TimeExtensions {

    public static string MinSecDurationText(this float duration) {

        if (float.IsNaN(duration)) {
            return "";
        }
        return $"{duration.Minutes()}:{string.Format("{0:00}", duration.Seconds())}";
    }

    public static string MinSecMillisecDurationText(this float duration) {

        return $"{duration.MinSecDurationText()}:{string.Format("{0:000}", duration.Milliseconds())}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float OneBeatDuration(this float bpm) {

        if (bpm <= 0) {
            return 0;
        }
        return 60.0f / bpm;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float TimeToBeat(this float time, float bpm) {

        return (time / 60.0f) * bpm;
    }

    public static float SecondsToMinutes(this float seconds) {

        return seconds / 60.0f;
    }


    public static int SecondsToDays(this int time) {

        return time / (3600 * 24);
    }

    public static int SecondsToHours(this int time) {

        return time / 3600;
    }

    public static int SecondsToMinutes(this int time) {

        return time / 60;
    }


    public static int DaysToSeconds(this int days) {

      return days * 3600 * 24;
    }

    public static int HoursToSeconds(this int hours) {

      return hours * 3600;
    }

    public static int MinutesToSeconds(this int minutes) {

      return minutes * 60;
    }


    // Hours fraction
    public static int Hours(this float time) {

        return (int)(time - time.TotalDays().DaysToSeconds()) / 3600;
    }

    // Minutes fraction
    public static int Minutes(this float time) {

        return (int)(time - time.TotalHours().HoursToSeconds()) / 60;
    }

    // Seconds fraction
    public static int Seconds(this float time) {

        return (int)(time % 60);
    }

    // Milliseconds fraction
    public static int Milliseconds(this float time) {

        return (int)((time % 1) * 1000);
    }




    public static int TotalDays(this float time) {

        return ((int)time).SecondsToDays();
    }

    public static int TotalHours(this float time) {

        return ((int)time).SecondsToHours();
    }

    public static int TotalMinutes(this float time) {

        return ((int)time).SecondsToMinutes();
    }

    public static int TotalSeconds(this float time) {

        return (int)(time);
    }

    public static long ToUnixTime(this DateTime dateTime) {

        return (long) (dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }

    public static DateTime AsUnixTime(this long unixTime) {

        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromSeconds(unixTime);
    }

    public static string GetFormattedRemainingTimeTwoOfDaysHoursMinutes(this TimeSpan timeSpan) {

        if (timeSpan.Days == 0) {
            return $"{timeSpan.Hours} h {timeSpan.Minutes} m";
        }
        return $"{timeSpan.Days} d {timeSpan.Hours} h";
    }
}
