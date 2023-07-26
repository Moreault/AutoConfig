namespace AutoConfig.Sample;

public interface ISample
{
    void Start();
}

[AutoInject]
public class Sample : ISample
{
    private readonly ITerminal _terminal;
    private readonly SampleOptions _options;

    public Sample(ITerminal terminal, IOptions<SampleOptions> options)
    {
        _terminal = terminal;
        _options = options.Value;
    }

    public void Start()
    {
        _terminal.Write("Reading from <color green=255>appsettings.json</color>");
        _terminal.Write(_options.Text);
    }
}