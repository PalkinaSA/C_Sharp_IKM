using C_Sharp_IKM.Data;
using C_Sharp_IKM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace C_Sharp_IKM.Controllers
{
    /// <summary>
    /// Контроллер для управления данными сотрудников.
    /// Обеспечивает отображение списка, просмотр деталей, создание, редактирование и удаление записей
    /// </summary>
    public class EmployeeController : Controller
    {
        /// <summary>
        /// Контекст базы данных
        /// </summary>
        private readonly ApplicationContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public EmployeeController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отображает список всех сотрудников
        /// </summary>
        /// <returns>Представление со списком сотрудников</returns>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Employees.ToListAsync());
        }

        /// <summary>
        /// Отображает подробную информацию о конкретном сотруднике
        /// </summary>
        /// <param name="id">Табельный номер сотрудника</param>
        /// <returns>Представление с данными сотрудника или ошибку, если сотрудник не найден</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                return BadRequest("Табельный номер должен быть положительным числом");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.ServiceNumber == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        /// <summary>
        /// Возвращает страницу для создания нового сотрудника
        /// </summary>
        /// <returns>Представление для создания нового сотрудника</returns>
        public async Task<IActionResult> Create()
        {
            return View();
        }

        /// <summary>
        /// Обрабатывает запрос на создание нового сотрудника.
        /// Выполняет проверку на уникальность табельного номера.
        /// </summary>
        /// <param name="employee">Объект сотрудника, полученный из формы</param>
        /// <returns>Редирект на список сотрудников при успехе или ту же форму при ошибках валидации</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            // Удаляем Tickets из ModelState
            ModelState.Remove(nameof(Employee.Tickets));

            // Проверяем табельный номер на валидность
            if (employee.ServiceNumber <= 0)
            {
                ModelState.AddModelError("ServiceNumber", "Табельный номер должен быть положительным числом");
            }
            
            // Проверяем, существует ли сотрудник с таким табельным номером
            var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.ServiceNumber == employee.ServiceNumber);

            if (existingEmployee != null)
            {
                ModelState.AddModelError("ServiceNumber", "Сотрудник с таким табельным номером уже существует");
            }

            // Если модель с ошибками (не валидна), то возвращаем представление с employee
            if (!ModelState.IsValid)
            {
                return View(employee);
            }

            // Добавляем сотрудника
            _context.Add(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Возвращает страницу редактирования данных сотрудника
        /// </summary>
        /// <param name="id">Табельный номер сотрудника для редактирования</param>
        /// <returns>Представление с данными сотрудника или ошибку, если сотрудник не найден</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            // Проверяем табельный номер на валидность
            if (id == null || id <= 0)
            {
                return BadRequest("Табельный номер должен быть положительным числом");
            }

            // Проверяем, есть ли сотрудник с таким номером
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Возвращаем пресдставление с сотрудником
            return View(employee);
        }

        /// <summary>
        /// Сохраняет изменения в данных сотрудника
        /// </summary>
        /// <param name="id">Табельный номер сотрудника из маршрута</param>
        /// <param name="employee">Обновленный объект сотрудника</param>
        /// <returns>Редирект на список при успехе</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            // Проверка совпадения табельного номера
            if (id != employee.ServiceNumber)
            {
                return NotFound();
            }

            // Проверка табельного номера на валидность
            if (employee.ServiceNumber <= 0)
            {
                ModelState.AddModelError("ServiceNumber", "Табельный номер должен быть положительным числом");
            }

            // Проверка на валидность модели
            if (!ModelState.IsValid) 
            {
                return View(employee); 
            }

            try
            {
                _context.Update(employee);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.ServiceNumber))
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
        /// Отображает страницу подтверждения удаления сотрудника
        /// </summary>
        /// <param name="id">Табельный номер сотрудника для удаления</param>
        /// <returns>Представление с заданным сотрудником или ошибку, если сотрудник не найден</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            // Проверка табельного номера
            if (id == null)
            {
                return NotFound();
            }

            // Проверка существоания сотрудника с данным табельным номером
            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.ServiceNumber == id);
            if (employee == null)
            {
                return NotFound();
            }

            // Возвращаем представление с сотрудником
            return View(employee);
        }

        /// <summary>
        /// Физическое удаление записи сотрудника из базы данных.
        /// Проверяет наличие связанных билетов перед удалением
        /// </summary>
        /// <param name="id">Табельный номер удаляемого сотрудника</param>
        /// <returns>Редирект на список сотрудников при успехе или ту же страницу с ошибкой при наличии зависимостей</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Получаем сотрудника по табельному номеру
            var employee = await _context.Employees.FindAsync(id);

            // Проверяем его существование
            if (employee == null) 
            {
                return RedirectToAction(nameof(Index));
            }

            // Проверяем, есть ли связанные с ним билеты
            var hasTickets = await _context.Tickets.AnyAsync(t => t.ServiceNumber == id);
            if (hasTickets)
            {
                // Вместо перенаправления возвращаем View с ошибкой
                ModelState.AddModelError("Наличие билетов", "Невозможно удалить сотрудника, у которого есть билеты");

                // Передаем сотрудника обратно в представление
                return View("Delete", employee);
            }

            // Удаляем сотрудника
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Проверяет существование сотрудника в базе данных
        /// </summary>
        /// <param name="id">Табельный номер сотрудника</param>
        /// <returns>True, если сотрудник существует</returns>
        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.ServiceNumber == id);
        }
    }
}