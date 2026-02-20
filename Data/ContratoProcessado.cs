using System.ComponentModel.DataAnnotations;

namespace IxcPosVendaWorker.Data;

public class ContratoProcessado
{
    [Key]
    public int Id { get; set; }
    
    public string IdContrato { get; set; } = string.Empty;
    
    public DateTime DataProcessamento { get; set; }
    
    public bool EmailEnviado { get; set; }
    
    public string? EmailDestino { get; set; }
    
    public string? Observacao { get; set; }
}