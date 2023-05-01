using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
	public class ReviewViewModel
	{
		[Required]
		public string Text { get; set; }
		[Required]
		public int Rating { get; set; }
	}
}
