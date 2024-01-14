namespace NZ.RdpMaid.App.SerializationModels.StateModel
{
    /// <summary>
    /// Структура хранилища состояния приложения.
    /// </summary>
    /// <param name="Theme">Выбранная тема оформления.</param>
    internal record RuntimeState(
        string? Theme = null);
}