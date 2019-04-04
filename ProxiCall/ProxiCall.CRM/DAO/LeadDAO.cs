using Microsoft.EntityFrameworkCore;
using NinjaNye.SearchExtensions.Levenshtein;
using Proxicall.CRM.Models;
using Proxicall.CRM.Models.Dictionnaries;
using Proxicall.CRM.Models.Enumeration.Levenshtein;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Proxicall.CRM.DAO
{
    public class LeadDAO
    {
        public static async Task<Lead> GetLeadByName(ProxicallCRMContext context, string firstName, string lastName)
        {
            firstName = char.ToLower(firstName[0]) + firstName.Substring(1).ToLower();
            lastName = char.ToLower(lastName[0]) + lastName.Substring(1).ToLower();
            var lead = await context.Leads.Where(l =>
                l.FirstName == firstName && l.LastName == lastName
                ||
                l.FirstName == lastName && l.LastName == firstName)
            .Include(l => l.Company)
            .FirstOrDefaultAsync();

            if (lead == null)
            {
                return FindLeadWithLevenshtein(context, firstName, lastName);
            }

            return lead;
        }

        private static Lead FindLeadWithLevenshtein(ProxicallCRMContext context, string firstName, string lastName)
        {
            var resultOfLvsDistanceDB = context
                    .Leads
                    .Include(l => l.Company)
                    .LevenshteinDistanceOf(leadInDB => leadInDB.FirstName, leadInDB => leadInDB.LastName)
                    .ComparedTo(firstName, lastName);

            var allowedDistanceForFirstName = CalculateAllowedDistance(firstName);
            var allowedDistanceForLastName = CalculateAllowedDistance(lastName);

            foreach (var lvsDistance in resultOfLvsDistanceDB)
            {
                //"FirstName LastName" compared to "FirstName LastName" in the database
                var distanceFirstNameToFirstNameDB = lvsDistance.Distances[LevenshteinCompare.FirstNameToFirstNameDB.Id];
                var distanceLastNameToLastNameDB = lvsDistance.Distances[LevenshteinCompare.LastNameToLastNameDB.Id];

                if (distanceFirstNameToFirstNameDB <= allowedDistanceForFirstName
                    && distanceLastNameToLastNameDB <= allowedDistanceForLastName)
                {
                    return lvsDistance.Item;
                }
                //"FirstName LastName" compared to "LastName FirstName" in the database
                var distanceFirstNametoLastNameDB = lvsDistance.Distances[LevenshteinCompare.FirstNameToLastNameDB.Id];
                var distanceLastNameToFirstNameDB = lvsDistance.Distances[LevenshteinCompare.LastNameToFirstNameDB.Id];

                if (distanceFirstNametoLastNameDB <= allowedDistanceForFirstName
                    && distanceLastNameToFirstNameDB <= allowedDistanceForLastName)
                {
                    return lvsDistance.Item;
                }
            }
            return null;
        }

        private static int CalculateAllowedDistance(string name)
        {
            if (name.Length < 3)
            {
                return LevenshteinAllowedDistance.AllowedDistance.GetValueOrDefault(LevenshteinAllowedDistance.VERY_SMALL_WORD);
            }
            else if (name.Length < 8)
            {
                return LevenshteinAllowedDistance.AllowedDistance.GetValueOrDefault(LevenshteinAllowedDistance.SMALL_WORD); ;
            }
            else
            {
                return LevenshteinAllowedDistance.AllowedDistance.GetValueOrDefault(LevenshteinAllowedDistance.MEDIUM_WORD); ;
            }
        }
    }
}
