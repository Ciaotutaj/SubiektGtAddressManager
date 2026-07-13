namespace SubiektGtAddressManager.Models;

public static class GlnValidator
{
    public static bool IsValidOrEmpty(string? value, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var trimmed = value.Trim();

        if (trimmed.Length != 13 || !trimmed.All(char.IsDigit))
        {
            error = "GLN musi mieć dokładnie 13 cyfr (albo pole musi być puste).";
            return false;
        }

        if (!HasValidCheckDigit(trimmed))
        {
            error = "Nieprawidłowa cyfra kontrolna GLN (niezgodna ze standardem GS1).";
            return false;
        }

        return true;
    }

    private static bool HasValidCheckDigit(string gln13)
    {
        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            var digit = gln13[i] - '0';
            var weight = (i % 2 == 0) ? 1 : 3;
            sum += digit * weight;
        }

        var checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == (gln13[12] - '0');
    }
}
