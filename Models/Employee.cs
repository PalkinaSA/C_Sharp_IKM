using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace C_Sharp_IKM.Models
{
    /// <summary>
    /// Класс, представляющий собой модель сотрудника из базы данных
    /// </summary>
    [Table("employees")]
    public class Employee
    {
        /// <summary>
        /// Табельный номер сотрудника
        /// </summary>
        [Key]
        [Required(ErrorMessage = "Табельный номер обязателен")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Табельный номер")]
        [Range(1, int.MaxValue, ErrorMessage = "Табельный номер должен быть положительным числом")]
        [Column("service_number")]
        public int ServiceNumber { get; set; }

        /// <summary>
        /// Имя сотрудника
        /// </summary>
        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        [StringLength(50, ErrorMessage = "Имя не может превышать 50 мимволов")]
        [Column("name")]
        public string Name { get; set; }

        /// <summary>
        /// Фамилия сотрудника
        /// </summary>
        [Required(ErrorMessage = "Фамилия обязательна")]
        [Display(Name = "Фамилия")]
        [StringLength(50, ErrorMessage = "Фамилия не может превышать 50 символов")]
        [Column("surname")]
        public string Surname { get; set; }

        /// <summary>
        /// Должность сотрудника
        /// </summary>
        [Required(ErrorMessage = "Должность обязательна")]
        [Display(Name = "Должность")]
        [StringLength(50, ErrorMessage = "Должность не может превышать 50 символов")]
        [Column("post")]
        public string Post { get; set; }

        /// <summary>
        /// Номер телефона сотрудника
        /// </summary>
        [Required(ErrorMessage = "Номер телефона обязателен")]
        [Display(Name = "Номер телефона")]
        [StringLength(20, ErrorMessage = "Номер телефона не может превышать 20 символов")]
        [Column("phone_number")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Полное имя сотрудника: Имя и фамилия
        /// </summary>
        [Display(Name = "Полное имя")]
        [NotMapped]
        public string FullName => $"{Surname} {Name}";

        /// <summary>
        /// Навигационное свойство для создания связи одни ко многим
        /// </summary>
        public ICollection<Ticket>? Tickets { get; set; }
    }
}
