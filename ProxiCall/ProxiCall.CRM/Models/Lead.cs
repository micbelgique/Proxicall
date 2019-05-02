using ProxiCall.Library.Dictionnaries.Lead;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ProxiCall.CRM.Models
{
    //Lead <> User
    public class Lead
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        
        [Display(Name = "Company")]
        [ForeignKey("Company")]
        public string CompanyId { get; set; }
        public Company Company { get; set; }

        public int Gender { get; set; }
        private string genderName;

        [NotMapped]
        [Display(Name = "Gender")]
        public string GenderName
        {
            get
            {
                var leadGender = new LeadGender();
                leadGender.AllGender.TryGetValue(Gender, out string genderName);
                return genderName;
            }
            set
            {
                var leadGender = new LeadGender();
                if (leadGender.AllGender.ContainsValue(value))
                {
                    genderName = value;
                }
                else
                {
                    genderName = LeadGender.UNDETERMINED;
                }
            }
        }


        [NotMapped]
        [Display(Name = "Full name")]
        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }

        public Lead()
        {
            var leadGender = new LeadGender();
            Gender = leadGender.AllGender.Keys.Where(k => leadGender.AllGender[k] == LeadGender.UNDETERMINED).First();
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
