using C_Sharp_IKM.Data;
using C_Sharp_IKM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace C_Sharp_IKM.Controllers
{
    /// <summary>
    /// Контроллер для управления данными событий.
    /// Обеспечивает отображение списка, просмотр деталей, создание, редактирование и удаление записей
    /// </summary>
    public class EventController : Controller
    {
        /// <summary>
        /// Контекст базы данных
        /// </summary>
        private readonly ApplicationContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public EventController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отображает список всех событий
        /// </summary>
        /// <returns>Представление со списком событий</returns>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.ToListAsync());
        }

        /// <summary>
        /// Отображает информацию об отдельном событии
        /// </summary>
        /// <param name="id">ID события</param>
        /// <returns>Представление с данными события или ошибку, если событие не найдено</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                return BadRequest("ID события должно быть положительным числом");
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        /// <summary>
        /// Возвращает страницу для создания нового события
        /// </summary>
        /// <returns>Представление для создания нового события</returns>
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        /// <summary>
        /// Обрабатывает запрос на создание нового события.
        /// Выполняет проверку на уникальность ID события
        /// </summary>
        /// <param name="event">Событие, полученное из формы</param>
        /// <returns>Редирект на список событий при успехе или ту же форму при ошибках валидации</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event)
        {
            // Удаляем Tickets из ModelState
            ModelState.Remove(nameof(Event.Tickets));

            // Проверяем дату
            if (@event.EventDate != default)
            {
                @event.EventDate = @event.EventDate.Date;
            }

            // Проверяем ID события
            if (@event.Id <= 0)
            {
                ModelState.AddModelError("Id", "ID события должно быть положительным числом");
            }

            // Проверка даты (событие не может быть в прошлом)
            if (@event.EventDate < DateTime.Today.Date)
            {
                ModelState.AddModelError("EventDate", "Дата события не может быть в прошлом");
            }
            
            // Проверка существования события с таким же ID
            var existingEvent = await _context.Events
                .AsNoTracking().FirstOrDefaultAsync(e => e.Id == @event.Id);
            if (existingEvent != null)
            {
                ModelState.AddModelError("Id", "Событие с таким ID уже существует");
            }

            // Проверка валидности модели
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(@event);
            }

            // Добавляем событие
            _context.Add(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Возвращает страницу редактирования данных события
        /// </summary>
        /// <param name="id">ID события для редактирования</param>
        /// <returns>Представление с данными события или ошибку, если событие не найдено</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            // Проверяем ID события на валидность
            if (id == null || id <= 0)
            {
                return BadRequest("ID события должно быть положительным числом");
            }

            // Проверяем существование события
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            // Инициализируем выпадающие списки
            await PopulateDropdowns();
            // Возвращаем представление с переданным событием
            return View(@event);
        }

        /// <summary>
        /// Сохраняет изменения в данных события
        /// </summary>
        /// <param name="id">ID события для изменения</param>
        /// <param name="event">Обновлённый объект события</param>
        /// <returns>Редирект на список при успехе</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event @event)
        {
            // Проверка совпадения ID событий
            if (id != @event.Id)
            {
                return NotFound();
            }

            // Проверка ID события на валидность
            if (@event.Id <= 0)
            {
                ModelState.AddModelError("Id", "ID события должно быть положительным числом");
            }

            // Проверка валидности модели
            if (!ModelState.IsValid) 
            {
                await PopulateDropdowns();
                return View(@event);
            }

            try
            {
                _context.Update(@event);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(@event.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Отображает страницу подтверждения удаления события
        /// </summary>
        /// <param name="id">ID события для удаления</param>
        /// <returns>Представление с заданным событием или ошибку, если событие не найдено</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            // Проверка ID события
            if (id == null || id <= 0)
            {
                return BadRequest("ID события должно быть положительным числом");
            }

            // Проверка существования события по его ID
            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            // Возвращаем представление с событием
            return View(@event);
        }

        /// <summary>
        /// Физическое удаление записи события из базы данных.
        /// Проверяет наличие связанных билетов перед удалением
        /// </summary>
        /// <param name="id">ID удаляемого события</param>
        /// <returns>Редирект на список событий при успехе или ту же страницу с ошибкой при наличии зависимостей</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Получаем событие по его ID
            var @event = await _context.Events.FindAsync(id);

            // Проверяем его существование
            if (@event == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Проверяем, есть ли связанные билеты
            var hasTickets = await _context.Tickets.AnyAsync(t => t.EventId == id);
            if (hasTickets)
            {
                // Вместо перенаправления возвращаем View с ошибкой
                ModelState.AddModelError("Наличие билетов", "Невозможно удалить событие, на которое есть билеты");

                // Передаем событие обратно в представление
                return View("Delete", @event);
            }

            // Удаляем событие
            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Проверяет существование события в базе данных
        /// </summary>
        /// <param name="id">ID события</param>
        /// <returns>True, если событие найдено</returns>
        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }

        /// <summary>
        /// Инициализирует список типов событий для отображения в выпадающих списках
        /// </summary>
        /// <returns>Асинхронная задача</returns>
        private async Task PopulateDropdowns()
        {
            ViewBag.EventTypes = new SelectList(new[]
            {
                "Конференция", "Семинар", "Тренинг", "Вебинар",
                "Выставка", "Форум", "Презентация", "Круглый стол",
                "Мастер-класс", "Фестиваль", "Лекция"
            });
        }
    }
}