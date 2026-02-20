using Microsoft.Extensions.Hosting;

namespace IxcPosVendaWorker.Helpers;

public static class EmailTemplateSelector
{
    private static readonly Dictionary<string, string> _mapeamento = new()
    {
        { "50", "BoasVindas_200Mega.html" },
        { "200", "BoasVindas_200Mega.html" },
        { "400", "BoasVindas_400Mega.html" },
        { "700", "BoasVindas_600Mega.html" },
        { "1000", "BoasVindas_600Mega.html" },
        { "ISENTO", "BoasVindas_Basico.html" }
    };

    private const string TemplatePadrao = "BoasVindas_Padrao.html";
    
    private static string _pastaTemplates = "./Templates";

    public static void ConfigurarCaminho(string caminho)
    {
        _pastaTemplates = caminho;
    }

    public static string ObterCaminhoPorPlano(string nomePlano)
    {
        if (string.IsNullOrWhiteSpace(nomePlano))
            return Path.Combine(_pastaTemplates, TemplatePadrao);

        var planoUpper = nomePlano.ToUpper();

        foreach (var (chave, arquivo) in _mapeamento)
        {
            if (planoUpper.Contains(chave))
            {
                return Path.Combine(_pastaTemplates, arquivo);
            }
        }

        return Path.Combine(_pastaTemplates, TemplatePadrao);
    }
}