using NUnit.Framework;

public class TimeExtensionsTest {

    [Test]
    public void TimeConversions() {

        int ms = 789;
        int s = 12;
        int m = 34;
        int h = 56;
        
        int totalSeconds = h * 60 * 60 + m * 60 + s;
        float t = totalSeconds + ms / 1000.0f;

        // Fraction
        Assert.True(8 == t.Hours());
        Assert.True(34 == t.Minutes());
        Assert.True(12 == t.Seconds());
        // We cannot count exact milliseconds because the fractional part of big float number loses precision.
        // You should use double for higher precision.
        Assert.True(t.Milliseconds() > 0);

        // Total
        Assert.True(2 == t.TotalDays());
        Assert.True(h == t.TotalHours());
        Assert.True(h * 60 + m == t.TotalMinutes());
        Assert.True(totalSeconds == t.TotalSeconds());
    }

    [Test]
    public void ConversionToSeconds() {

        int days = 60 * 60 * 24 * 2;
        Assert.True(days == 2.DaysToSeconds());

        int hours = 60 * 60 * 56;
        Assert.True(hours == 56.HoursToSeconds());

        int minutes = 60 * 67;
        Assert.True(minutes == 67.MinutesToSeconds());
    }

    [Test]
    public void ConversionFromSeconds() {

        int days = 60 * 60 * 56;
        Assert.True(2 == days.SecondsToDays());

        int hours = 60 * 60 * 56;
        Assert.True(56 == hours.SecondsToHours());

        int minutes = 60 * 67;
        Assert.True(67 == minutes.SecondsToMinutes());
    }
}
