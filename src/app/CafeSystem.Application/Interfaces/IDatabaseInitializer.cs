namespace CafeSystem.Application.Interfaces
{
    /// <summary>
    /// Inicializador responsável por preparar o banco de dados para a primeira execução.
    /// </summary>
    public interface IDatabaseInitializer
    {
        /// <summary>
        /// Garante que os dados mínimos existam (como o usuário administrador inicial).
        /// </summary>
        Task EnsureSeedDataAsync(CancellationToken cancellationToken = default);
    }
}
