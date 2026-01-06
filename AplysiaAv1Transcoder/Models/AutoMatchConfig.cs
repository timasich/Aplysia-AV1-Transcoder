namespace AplysiaAv1Transcoder.Models;

public enum AutoMatchMode
{
    Balanced,
    Safe
}

public sealed class AutoMatchConfig
{
    public AutoMatchMode Mode { get; set; } = AutoMatchMode.Balanced;
    public int Bias { get; set; } = 0;

    public AutoMatchConfig()
    {
    }

    public AutoMatchConfig(AutoMatchMode mode, int bias)
    {
        Mode = mode;
        Bias = bias;
    }
}
