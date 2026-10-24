namespace KnowHowApi.Hubs
{
    // Quem está em cada sala de vídeo agora (por conexão do SignalR). Fica em memória:
    // vale para uma instância da API, que é como ela roda hoje.
    public class SalasChamada
    {
        private readonly object _lock = new();
        private readonly Dictionary<Guid, Dictionary<string, int>> _salas = new();

        public record ResultadoEntrada(bool OutroPresente, List<string> ConexoesSubstituidas);

        // Uma conexão por usuário na sala: entrar de outro aparelho/aba substitui a anterior.
        public ResultadoEntrada Entrar(Guid aulaId, string conexaoId, int usuarioId)
        {
            lock (_lock)
            {
                if (!_salas.TryGetValue(aulaId, out var conexoes))
                {
                    conexoes = new Dictionary<string, int>();
                    _salas[aulaId] = conexoes;
                }

                var substituidas = conexoes
                    .Where(c => c.Value == usuarioId && c.Key != conexaoId)
                    .Select(c => c.Key)
                    .ToList();
                foreach (var conexao in substituidas)
                    conexoes.Remove(conexao);

                conexoes[conexaoId] = usuarioId;
                return new ResultadoEntrada(conexoes.Values.Any(u => u != usuarioId), substituidas);
            }
        }

        public bool Sair(Guid aulaId, string conexaoId)
        {
            lock (_lock)
            {
                if (!_salas.TryGetValue(aulaId, out var conexoes) || !conexoes.Remove(conexaoId))
                    return false;

                if (conexoes.Count == 0)
                    _salas.Remove(aulaId);
                return true;
            }
        }

        public bool Contem(Guid aulaId, string conexaoId)
        {
            lock (_lock)
            {
                return _salas.TryGetValue(aulaId, out var conexoes) && conexoes.ContainsKey(conexaoId);
            }
        }

        // Ao desconectar: tira a conexão de todas as salas e devolve de quais saiu.
        public List<(Guid AulaId, int UsuarioId)> RemoverConexao(string conexaoId)
        {
            lock (_lock)
            {
                var saidas = new List<(Guid, int)>();
                foreach (var (aulaId, conexoes) in _salas.ToList())
                {
                    if (!conexoes.Remove(conexaoId, out var usuarioId))
                        continue;

                    saidas.Add((aulaId, usuarioId));
                    if (conexoes.Count == 0)
                        _salas.Remove(aulaId);
                }
                return saidas;
            }
        }
    }
}
