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
    private readonly SampleAOptions _optionsA;
    private readonly SampleBOptions _optionsB;

    public Sample(ITerminal terminal, IOptions<SampleOptions> options, SampleAOptions optionsA, SampleBOptions optionsB)
    {
        _terminal = terminal;
        _options = options.Value;
        _optionsA = optionsA;
        _optionsB = optionsB;
    }

    public void Start()
    {
        _terminal.Write("Reading from <color green=255>appsettings.json</color>");
        _terminal.Write(_options.Text);
        _terminal.Write(_optionsA.Text);
        _terminal.Write(_optionsB.Text);
    }
}