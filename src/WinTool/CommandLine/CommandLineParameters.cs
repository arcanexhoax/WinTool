using System.Text;

namespace WinTool.CommandLine;

public class CommandLineParameters
{
    public BackgroundParameter? BackgroundParameter { get; set; }
    public CreateFileParameter? CreateFileParameter { get; set; }
    public ShutdownOnEndedParameter? ShutdownOnEndedParameter { get; set; }

    public static CommandLineParameters Parse(string[] args)
    {
        CommandLineParameters clp = new();
        ICommandLineParameter? parameter = null;

        foreach (var a in args)
        {
            if (a.StartsWith('/'))
            {
                switch (a)
                {
                    case BackgroundParameter.ParameterName:
                        clp.BackgroundParameter = new BackgroundParameter();
                        parameter = clp.BackgroundParameter;
                        break;
                    case CreateFileParameter.ParameterName:
                        clp.CreateFileParameter = new CreateFileParameter();
                        parameter = clp.CreateFileParameter;
                        break;
                    case ShutdownOnEndedParameter.ParameterName:
                        clp.ShutdownOnEndedParameter = new ShutdownOnEndedParameter();
                        parameter = clp.ShutdownOnEndedParameter;
                        break;
                }
            }
            else if (a.StartsWith('-') && parameter is not null)
            {
                parameter.Parse(a);
            }
        }

        return clp;
    }

    public override string ToString()
    {
        StringBuilder args = new();

        if (BackgroundParameter is not null) 
            args.Append($"{BackgroundParameter} ");
        if (CreateFileParameter is not null)
            args.Append($"{CreateFileParameter} ");
        if (ShutdownOnEndedParameter is not null)
            args.Append($"{ShutdownOnEndedParameter} ");

        return args.ToString();
    }
}
