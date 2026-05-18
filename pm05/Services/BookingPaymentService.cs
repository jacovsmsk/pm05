using System.Linq;
using pm05.Data;
using pm05.Models;

namespace pm05.Services
{
    public static class BookingPaymentService
    {
        public static void ApplyPaymentEffects(int bookingId)
        {
            using (var db = new ApplicationDbContext())
            {
                ApplyPaymentEffects(bookingId, db);
            }
        }

        public static void ApplyPaymentEffects(int bookingId, ApplicationDbContext db)
        {
            var booking = db.Bookings.Find(bookingId);
            if (booking == null)
                return;

            var tour = db.Tours.Find(booking.TourId);
            if (tour == null)
                return;

            var paidTotal = db.Payments
                .Where(p => p.BookingId == bookingId)
                .Sum(p => (decimal?)p.Amount) ?? 0m;

            if (booking.Status != BookingStatuses.Cancelled)
            {
                if (paidTotal >= tour.Price)
                    booking.Status = BookingStatuses.Paid;
                else if (booking.Status == BookingStatuses.Paid)
                    booking.Status = BookingStatuses.AwaitingPayment;
            }

            var clientId = booking.ClientId;
            var totalSpent = (
                from p in db.Payments
                join b in db.Bookings on p.BookingId equals b.Id
                where b.ClientId == clientId
                select p.Amount).DefaultIfEmpty(0m).Sum();

            var client = db.Clients.Find(clientId);
            if (client != null)
                client.TotalSpent = totalSpent;

            db.SaveChanges();
        }
    }
}
