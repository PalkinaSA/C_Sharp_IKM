namespace C_Sharp_IKM.Models
{
    /// <summary>
    /// Модель представления для отображения информации об ошибках в пользовательском интерфейсе.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Получает или задает уникальный идентификатор текущего HTTP-запроса
        /// </summary>
        /// <value>
        /// Строка, содержащая ID запроса, или null, если идентификатор не был присвоен
        /// </value>
        public string? RequestId { get; set; }

        /// <summary>
        /// Возвращает логическое значение, указывающее, нужно ли отображать идентификатор запроса на странице
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
