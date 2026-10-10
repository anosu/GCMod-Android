using MelonLoader.Preferences;

namespace GCMod;

internal sealed class PositiveIntValidator(int defaultValue) : ValueValidator
{
    public override bool IsValid(object value) => value is int number && number > 0;

    public override object EnsureValid(object value) => IsValid(value) ? value : defaultValue;
}
