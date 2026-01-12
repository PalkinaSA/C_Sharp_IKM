using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C_Sharp_IKM.Models
{
    /// <summary>
    /// Класс, представляющий собой модель билета из базы данных
    /// </summary>
    [Table("tickets")]
    public class Ticket
    {
        /// <summary>
        /// Номер билета
        /// </summary>
        [Key]
        [Required(ErrorMessage = "Номер билета обязателен")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Номер билета")]
        [Range(1, int.MaxValue, ErrorMessage = "Номер билета должен быть положительным числом")]
        [Column("ticket_number")]
        public int TicketNumber { get; set; }

        /// <summary>
        /// Табельный номер сотрудника
        /// </summary>
        [Required(ErrorMessage = "Табельный номер обязателен")]
        [Display(Name = "Табельный номер сотрудника")]
        [Range(1, int.MaxValue, ErrorMessage = "Табельный номер должен быть положительным числом")]
        [Column("service_number")]
        public int ServiceNumber { get; set; }

        /// <summary>
        /// ID события
        /// </summary>
        [Required(ErrorMessage = "ID события обязателен")]
        [Display(Name = "ID события")]
        [Range(1, int.MaxValue, ErrorMessage = "ID события должно быть положительным числом")]
        [Column("event_id")]
        public int EventId { get; set; }

        /// <summary>
        /// Дата продажи билета
        /// </summary>
        [Required(ErrorMessage = "Дата продажи обязательна")]
        [Display(Name = "Дата продажи")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Column("sale_date", TypeName = "date")]
        public DateTime SaleDate { get; set; } = DateTime.Today;

        /// <summary>
        /// Тип билета
        /// </summary>
        [Required(ErrorMessage = "Тип билета обязателен")]
        [Display(Name = "Тип билета")]
        [StringLength(20, ErrorMessage = "Тип билета не может превышать 20 символов")]
        [Column("ticket_type")]
        public string TicketType { get; set; }

        /// <summary>
        /// Способ оплаты билета
        /// </summary>
        [Required(ErrorMessage = "Способ оплаты обязателен")]
        [Display(Name = "Способ оплаты")]
        [StringLength(20, ErrorMessage = "Способ оплаты не может превышать 20 символов")]
        [Column("payment_method")]
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Навигационное свойство для создания связи многие ко одному
        /// </summary>
        [ForeignKey("ServiceNumber")]
        public Employee? Employee { get; set; }

        /// <summary>
        /// Навигационное свойство для создания связи многие ко одному
        /// </summary>
        [ForeignKey("EventId")]
        public Event? Event { get; set; }
    }
}
