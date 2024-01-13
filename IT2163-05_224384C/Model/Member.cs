using Microsoft.AspNetCore.Identity;

namespace IT2163_05_224384C.Model
{
	public class Member: IdentityUser
	{
		public string FullName { get; set; }

        public string CreditCard { get; set; }

        public string Gender { get; set; }

        public string DeliveryAddress { get; set; }

        public string AboutMe { get; set; }

        public string Photo { get; set; }

        public string PasswordHistory { get; set; }

        public DateTime LastPasswordChangeDate { get; set; }

        public DateTime LastLoginDate { get; set; }
    }
}
