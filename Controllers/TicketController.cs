using C_Sharp_IKM.Models;
using C_Sharp_IKM.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace C_Sharp_IKM.Controllers
{
    /// <summary>
    /// Контроллер для управления данными билетов.
    /// Обеспечивает отображение списка, просмотр деталей, создание, редактирование и удаление записей
    /// </summary>
    public class TicketController : Controller
    {
        /// <summary>
        /// Контекст базы данных
        /// </summary>
        private readonly ApplicationContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public TicketController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отображает список всех билетов
        /// </summary>
        /// <returns>Представление со списком билетов</returns>
        public async Task<IActionResult> Index()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Employee)
                .Include(t => t.Event)
                .ToListAsync();

            return View(tickets);
        }

        /// <summary>
        /// Отображает подробную информацию о конкретном билете
        /// </summary>
        /// <param name="id">Номер билета</param>
        /// <returns>Представление с данными билета или ошибку, если билет не найден</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                return BadRequest("Номер билета должен быть положительным числом");
            }

            var ticket = await _context.Tickets
                .Include(t => t.Employee)
                .Include(t => t.Event)
                .FirstOrDefaultAsync(m => m.TicketNumber == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        /// <summary>
        /// Возвращает страницу для создания нового билета
        /// </summary>
        /// <returns>Представление для создания нового билета</returns>
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        /// <summary>
        /// Обрабатывает запрос на создание нового билета.
        /// Выполняет проверку на уникальность номера билета, существование сотрудника и события
        /// </summary>
        /// <param name="ticket">Объект билета, полученный из формы</param>
        /// <returns>Редирект на список билетов при успехе или ту же форму при ошибках валидации</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            // Удаляем навигационные свойства из Ticket
            ModelState.Remove(nameof(Ticket.Employee));
            ModelState.Remove(nameof(Ticket.Event));

            // Проверка номера билета
            if (ticket.TicketNumber <= 0)
            {
                ModelState.AddModelError("TicketNumber", "Номер билета должен быть положительным числом");
            }

            // Проверка табельного номера
            if (ticket.ServiceNumber <= 0)
            {
                ModelState.AddModelError("ServiceNumber", "Табельный номер должен быть положительным числом");
            }

            // Проверка ID события
            if (ticket.EventId <= 0)
            {
                ModelState.AddModelError("EventId", "ID события должно быть положительным числом");
            }

            // Проверка даты продажи (не может быть в будущем)
            if (ticket.SaleDate > DateTime.Today)
            {
                ModelState.AddModelError("SaleDate", "Дата продажи не может быть в будущем");
            }

            // Проверяем, существует ли билет с таким номером
            var existingTicket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.TicketNumber == ticket.TicketNumber);
            if (existingTicket != null)
            {
                ModelState.AddModelError("TicketNumber", "Билет с таким номером уже существует");
            }
            
            // Проверяем существование сотрудника
            var employeeExists = await _context.Employees.AnyAsync(e => e.ServiceNumber == ticket.ServiceNumber);
            if (!employeeExists)
            {
                ModelState.AddModelError("ServiceNumber", "Сотрудник не найден");
            }
            
            // Проверяем существование события
            var eventExists = await _context.Events.AnyAsync(e => e.Id == ticket.EventId);
            if (!eventExists)
            {
                ModelState.AddModelError("EventId", "Событие не найдено"); 
            }

            // Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(ticket);
            }

            // Добавляем билет
            _context.Add(ticket);
            // Сохраняем изменения
            await _context.SaveChangesAsync();
            // Переходим на страницу со списком билетов
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Возвращает страницу редактирования данных билета
        /// </summary>
        /// <param name="id">Номер билета для редактирования</param>
        /// <returns>Представление с данными билета или ошибку, если билет не найден</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            // Проверяем номер на валидность
            if (id == null || id <= 0)
            {
                return BadRequest("Номер билета должен быть положительным числом");
            }

            // Проверяем, есть ли билет с таким номером
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Инициализируем выпадающие списки
            await PopulateDropdowns();
            // Возвращаем представление с билетом
            return View(ticket);
        }

        /// <summary>
        /// Сохраняет изменения в данных билета
        /// </summary>
        /// <param name="id">Номер билета для редактирования</param>
        /// <param name="ticket">Обновлённый объект билета</param>
        /// <returns>Редирект на список при успехе</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ticket ticket)
        {
            // Проверка на совпадение номеров билетов
            if (id != ticket.TicketNumber)
            {
                return NotFound();
            }

            // Дополнительные проверки
            if (ticket.TicketNumber <= 0)
            {
                ModelState.AddModelError("TicketNumber", "Номер билета должен быть положительным числом");
            }

            if (ticket.ServiceNumber <= 0)
            {
                ModelState.AddModelError("ServiceNumber", "Табельный номер должен быть положительным числом");
            }

            if (ticket.EventId <= 0)
            {
                ModelState.AddModelError("EventId", "ID события должно быть положительным числом");
            }
            
            // Проверяем существование сотрудника
            var employeeExists = await _context.Employees.AnyAsync(e => e.ServiceNumber == ticket.ServiceNumber);
            if (!employeeExists)
            {
                ModelState.AddModelError("ServiceNumber", "Сотрудник не найден");
            }

            // Проверяем существование события
            var eventExists = await _context.Events.AnyAsync(e => e.Id == ticket.EventId);
            if (!eventExists)
            {
                ModelState.AddModelError("EventId", "Событие не найдено");
            }

            // Проверяем валидность модели
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(ticket);
            }

            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(ticket.TicketNumber))
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
        /// Отображает страницу подтверждения удаления билета
        /// </summary>
        /// <param name="id">Номер билета для удаления</param>
        /// <returns>Представление с заданным билетом или ошибку, если билет не найден</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            // Проверка номера билета
            if (id == null || id <= 0)
            {
                return BadRequest("Номер билета должен быть положительным числом");
            }

            // Проверка существоания билета с данным номером
            var ticket = await _context.Tickets
                .Include(t => t.Employee)
                .Include(t => t.Event)
                .FirstOrDefaultAsync(m => m.TicketNumber == id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Возвращаем представление с билетом
            return View(ticket);
        }

        /// <summary>
        /// Физическое удаление записи билета из базы данных
        /// </summary>
        /// <param name="id">Номер билета для удаления</param>
        /// <returns>Редирект на список сотрудников</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Проверяем существование билета
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null) 
            { 
                return RedirectToAction(nameof(Index));
            }

            // Удаляем биелт из базы
            _context.Tickets.Remove(ticket);
            // Сохраняем изменения
            await _context.SaveChangesAsync();
            // Переходим на страницу со списком билетов
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Проверяет существование билета в базе данных
        /// </summary>
        /// <param name="id">Номер билета</param>
        /// <returns>True, если билет найден</returns>
        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.TicketNumber == id);
        }

        /// <summary>
        /// Инициализирует выпадающие списки
        /// </summary>
        /// <returns>Асинхронная задача</returns>
        private async Task PopulateDropdowns()
        {
            var employees = await _context.Employees.ToListAsync();
            var events = await _context.Events.ToListAsync();

            // Списки сотрудников и билетов
            ViewData["ServiceNumber"] = new SelectList(employees, "ServiceNumber", "FullName");
            ViewData["EventId"] = new SelectList(events, "Id", "Name");

            // Статические списки для типов билетов и способов оплаты
            ViewData["TicketTypes"] = new SelectList(new[]
            {
                "Взрослый", "VIP", "Студенческий", "Детский", "Пенсионный", "Льготный"
            });

            ViewData["PaymentMethods"] = new SelectList(new[]
            {
                "Наличные", "Карта", "Безналичный расчет", "Онлайн", "Перевод"
            });
        }
    }
}