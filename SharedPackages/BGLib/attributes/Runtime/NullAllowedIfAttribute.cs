using System;

[AttributeUsage(AttributeTargets.Field)]
public class NullAllowedIf : NullAllowed {

    public readonly string propertyName;
    private readonly object _valueToCompare;
    private readonly ComparisonOperation _comparisonOperation;

    public NullAllowedIf(string propertyName, object equalsTo, Context context = Context.Everywhere) : this(
        propertyName,
        ComparisonOperation.Equal,
        equalsTo,
        context
    ) { }

    public NullAllowedIf(
        string propertyName,
        ComparisonOperation comparisonOperation,
        object valueToCompare,
        Context context = Context.Everywhere
    ) : base(context) {

        this.propertyName = propertyName;
        _valueToCompare = valueToCompare;
        _comparisonOperation = comparisonOperation;
    }

    public bool IsNullAllowedFor(object value, Context context) {

        switch (_comparisonOperation) {
            case ComparisonOperation.Equal:
                if (Equals(value, _valueToCompare)) {
                    return IsNullAllowedFor(context);
                }
                break;
            case ComparisonOperation.NotEqual:
                if (!Equals(value, _valueToCompare)) {
                    return IsNullAllowedFor(context);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    _comparisonOperation.ToString(),
                    "Please implement new types of comparison operations"
                );
        }
        return false;
    }
}
