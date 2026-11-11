namespace MenuSystem;

// public delegate TOut Func<TOut>();

public class MenuItem
{
    public string Title { get; set; } = default!;
    public Func<string>? TitleProvider { get; set; }

    public string Shortcut { get; set; } = default!;

    public Func<string>? MethodToRun { get; set; }

    public override string ToString() =>
        Shortcut + ") " + (TitleProvider != null ? TitleProvider() : Title);
}