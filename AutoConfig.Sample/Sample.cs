namespace AutoConfig.Sample;

public interface ISample
{
    void Start();
}

[AutoInject]
public class Sample(ITerminal terminal, IOptions<SampleOptions> options, IOptions<SampleAOptions> optionsA, IOptions<SampleBOptions> optionsB) : ISample
{
    private readonly ITerminal _terminal = terminal;
    private readonly SampleOptions _options = options.Value;
    private readonly SampleAOptions _optionsA = optionsA.Value;
    private readonly SampleBOptions _optionsB = optionsB.Value;

    public void Start()
    {
        _terminal.Write("Reading from <color green=255>appsettings.json</color>");
        _terminal.Write(_options.Text);
        _terminal.Write(_optionsA.Text);
        _terminal.Write(_optionsB.Text);
    }
}