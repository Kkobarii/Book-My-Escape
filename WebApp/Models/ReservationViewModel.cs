using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class ReservationViewModel
    {
        [Required(ErrorMessage = "The Check in field is required."), Display(Name = "Check in")]
        public DateTime CheckIn { get; set; }
        [Required(ErrorMessage = "The Check out field is required."), Display(Name = "Check out")]
        public DateTime CheckOut { get; set; }
        [Required(ErrorMessage = "The Player count field is required."), Display(Name = "Number of players")]
        public int PlayerCount { get; set; }
    }
}
