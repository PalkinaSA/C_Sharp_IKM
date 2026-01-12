using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using C_Sharp_IKM.Models;

namespace C_Sharp_IKM.Controllers
{
    /// <summary>
    /// Контроллер для управления страницей Home
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Сервис логгирования для записи диагностических сообщений и ошибок
        /// </summary>
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Конструктор для инициализации контроллера
        /// </summary>
        /// <param name="logger">Интерфейс логгера, предоставляемый механизмом внедрения зависимостей</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Возвращает страницу Home
        /// </summary>
        /// <returns>Представление приветствующей страницы</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Обрабатывает ошибки, возникающие в процессе работы приложения.
        /// Генерирует уникальный идентификатор запроса для облегчения отладки
        /// </summary>
        /// <returns>Представление со сведениями об ошибке</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
