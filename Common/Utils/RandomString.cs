namespace Common.Utils;

public static class RandomString
{
    private const string Charsets = "ABCDEFGHIJKLKMOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string Generate(int length = 8)
    {
        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = Charsets[Random.Shared.Next(Charsets.Length)];
        }

        return new string(chars);
    }
}