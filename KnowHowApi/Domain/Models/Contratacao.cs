namespace KnowHowApi.Domain.Models
{
    // Venda registrada pelo professor: aula(s) avulsa(s), pacote ou mês de um plano mensal.
    // Descrição, valor, total de aulas e datas são copiados no momento da venda, então
    // editar ou excluir o pacote/plano depois não altera as contratações já registradas.
    public class Contratacao
    {
        public const string TipoAvulsa = "avulsa";
        public const string TipoPacote = "pacote";
        public const string TipoMensal = "mensal";
        public const string StatusPendente = "pendente";
        public const string StatusPago = "pago";

        public Guid Id { get; set; }

        public int ProfessorId { get; set; }
        public Usuario Professor { get; set; }

        public int AlunoId { get; set; }
        public Usuario Aluno { get; set; }

        public string Tipo { get; set; } = TipoAvulsa;
        // Pacote ou plano mensal de origem (sem FK, pela razão acima); null para aula avulsa.
        public Guid? OfertaId { get; set; }

        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public int TotalAulas { get; set; }
        public int AulasUsadas { get; set; }

        public DateOnly Inicio { get; set; }
        // Validade do pacote ou fim do mês contratado; null para aula avulsa.
        public DateOnly? Fim { get; set; }

        public string StatusPagamento { get; set; } = StatusPendente;
        public DateTime? PagoEm { get; set; }
        public DateTime CriadaEm { get; set; }
    }
}
