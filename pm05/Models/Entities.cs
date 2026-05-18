using System;
using System.ComponentModel.DataAnnotations;

namespace pm05.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Login { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Salt { get; set; }

        public int RoleId { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class LoginAttempt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Login { get; set; }

        public DateTime AttemptedAt { get; set; }

        public bool IsSuccessful { get; set; }
    }

    public class Country
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
    }

    public class Tour
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; }

        public int CountryId { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; }
    }

    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; }

        [MaxLength(50)]
        public string Passport { get; set; }

        [MaxLength(50)]
        public string Phone { get; set; }

        public decimal TotalSpent { get; set; }
    }

    public class Booking
    {
        [Key]
        public int Id { get; set; }

        public int TourId { get; set; }

        public int ClientId { get; set; }

        public DateTime BookingDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }
    }

    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int BookingId { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }
    }

    public static class BookingStatuses
    {
        public const string AwaitingPayment = "Ожидает оплаты";
        public const string Paid = "Оплачено";
        public const string Cancelled = "Отменено";
    }
}
