namespace BlocoNaRua.Services.Utils;

public static class CodeGenerator
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Generate(int length = 8) =>
        new(Enumerable.Repeat(Chars, length)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
}
