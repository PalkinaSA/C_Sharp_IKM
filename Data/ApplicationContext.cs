using C_Sharp_IKM.Models;
using Microsoft.EntityFrameworkCore;

namespace C_Sharp_IKM.Data
{
    /// <summary>
    /// Контекст базы данных приложения
    /// </summary>
    public class ApplicationContext : DbContext
    {
        /// <summary>
        /// Список сотрудников
        /// </summary>
        public DbSet<Employee> Employees => Set<Employee>();

        /// <summary>
        /// Список событий
        /// </summary>
        public DbSet<Event> Events => Set<Event>();

        /// <summary>
        /// Список билетов
        /// </summary>
        public DbSet<Ticket> Tickets => Set<Ticket>();

        /// <summary>
        /// Конструктор для контекста приложения
        /// </summary>
        /// <param name="options">Настройки базы данных</param>
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) 
        { 
            Database.EnsureCreated();
        }

        /// <summary>
        /// Метод настройки моделей
        /// </summary>
        /// <param name="modelBuilder">Строитель модели данных</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка для таблицы Events
            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("events"); // Имя таблицы в Postgres (лучше в нижнем регистре)
                entity.Property(e => e.EventDate)
                      .HasDefaultValueSql("CURRENT_DATE"); // SQL: DEFAULT CURRENT_DATE
            });

            // Настройка для таблицы Employees
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("employees");
                entity.HasKey(e => e.ServiceNumber); // Явно указываем нестандартный PK
            });

            // Настройка для таблицы Tickets
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("tickets");
                entity.HasKey(t => t.TicketNumber);

                entity.Property(t => t.SaleDate)
                      .HasDefaultValueSql("CURRENT_DATE");

                // Настройка связи "Многие к одному" (Билет к Сотруднику)
                entity.HasOne(t => t.Employee)
                      .WithMany(e => e.Tickets)
                      .HasForeignKey(t => t.ServiceNumber)
                      .OnDelete(DeleteBehavior.Cascade); // SQL: ON DELETE CASCADE

                // Настройка связи "Многие к одному" (Билет к Событию)
                entity.HasOne(t => t.Event)
                      .WithMany(ev => ev.Tickets)
                      .HasForeignKey(t => t.EventId)
                      .OnDelete(DeleteBehavior.Cascade); // SQL: ON DELETE CASCADE
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
