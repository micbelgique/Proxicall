namespace ProxiCall.Library.Enumeration.Levenshtein
{
    public class LevenshteinCompare : Enumeration
    {
        public static LevenshteinCompare FirstNameToFirstNameDB = new CompareFirstNameToFirstNameDB();
        public static LevenshteinCompare FirstNameToLastNameDB = new CompareFirstNameToLastName();
        public static LevenshteinCompare LastNameToFirstNameDB = new CompareLastNameToFirstNameDB();
        public static LevenshteinCompare LastNameToLastNameDB = new CompareLastNameToLastNameDB();

        protected LevenshteinCompare(int id, string name)
        : base(id, name)
        { }

        private class CompareFirstNameToFirstNameDB : LevenshteinCompare
        {
            public CompareFirstNameToFirstNameDB() : base(0, "FirstNameToFirstNameDB")
            { }
        }

        private class CompareFirstNameToLastName : LevenshteinCompare
        {
            public CompareFirstNameToLastName() : base(1, "FirstNameToLastNameDB")
            { }
        }

        private class CompareLastNameToFirstNameDB : LevenshteinCompare
        {
            public CompareLastNameToFirstNameDB() : base(2, "LastNameToFirstNameDB")
            { }
        }

        private class CompareLastNameToLastNameDB : LevenshteinCompare
        {
            public CompareLastNameToLastNameDB() : base(3, "LastNameToLastNameDB")
            { }
        }
    }
}
