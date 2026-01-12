using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C_Sharp_IKM.Models
{
    /// <summary>
    /// Класс, представляющий собой модель события из базы данных
    /// </summary>
    [Table("events")]
    public class Event
    {
        /// <summary>
        /// ID события
        /// </summary>
        [Key]
        [Required(ErrorMessage = "ID события обязателен")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "ID")]
        [Range(1, int.MaxValue, ErrorMessage = "ID события должно быть положительным числом")]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Название события
        /// </summary>
        [Required(ErrorMessage = "Название обязательно")]
        [Display(Name = "Название")]
        [StringLength(100, ErrorMessage = "Название не может превышать 100 символов")]
        [Column("name")]
        public string Name { get; set; }

        /// <summary>
        /// Дата проведения события
        /// </summary>
        [Required(ErrorMessage = "Дата события обязательна")]
        [Display(Name = "Дата события")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Column("event_date", TypeName = "date")]
        public DateTime EventDate { get; set; } = DateTime.Today;

        /// <summary>
        /// Тип события
        /// </summary>
        [Required(ErrorMessage = "Тип события обязателен")]
        [Display(Name = "Тип события")]
        [StringLength(20, ErrorMessage = "Тип события не может превышать 20 символов")]
        [Column("event_type")]
        public string EventType { get; set; }

        /// <summary>
        /// Навигационное свойство для создания связи один ко многим
        /// </summary>
        public ICollection<Ticket>? Tickets { get; set; }
    }
}
