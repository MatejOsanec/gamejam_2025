using System;
using System.Text;

public static class ExceptionExtensions {

    public static string GenerateFullStackMessage(this Exception e) {

        var sb = new StringBuilder();
        sb.Append(e.Message);
        sb.Append(" \n");
        sb.Append(e.StackTrace);
        sb.Append(" \n");
        var innerException = e.InnerException;
        int innerExceptionCount = 0;
        while (innerException != null) {
            sb.Append("Inner exception ");
            sb.Append(++innerExceptionCount);
            sb.Append(":\n");
            sb.Append(innerException.Message);
            sb.Append(" \n");
            sb.Append(innerException.StackTrace);
            sb.Append(" \n");
            innerException = innerException.InnerException;
        }
        return sb.ToString();
    }
}
