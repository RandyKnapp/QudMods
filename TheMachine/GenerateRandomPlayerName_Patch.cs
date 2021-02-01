using HarmonyLib;
using XRL.Core;

namespace TheMachine
{
    [HarmonyPatch(typeof(XRLCore))]
    [HarmonyPatch("GenerateRandomPlayerName")]
    public static class GenerateRandomPlayerName_Patch
    {
        private static readonly string[] LeaderTitles = new string[] {
            "President", "Prime Minister", "King", "Queen", "Supreme Leader", "Grand Duke", "Prince Regnant", "Emir", "Sovereign"
        };

        private static readonly string[] PhilosopherTitles = new string[]
        {
            "", "", "", "", "Dr.", "Prof.", "Rev.", "Rev. Dr."
        };

        private static readonly string[] FirstNames = new string[] {
            "James",
            "John",
            "Robert",
            "Michael",
            "William",
            "David",
            "Richard",
            "Joseph",
            "Thomas",
            "Charles",
            "Christopher",
            "Daniel",
            "Matthew",
            "Anthony",
            "Donald",
            "Mark",
            "Paul",
            "Steven",
            "Andrew",
            "Kenneth",
            "Joshua",
            "Kevin",
            "Brian",
            "George",
            "Edward",
            "Ronald",
            "Timothy",
            "Jason",
            "Jeffrey",
            "Ryan",
            "Mary",
            "Patricia",
            "Jennifer",
            "Linda",
            "Elizabeth",
            "Barbara",
            "Susan",
            "Jessica",
            "Sarah",
            "Karen",
            "Nancy",
            "Lisa",
            "Margaret",
            "Betty",
            "Sandra",
            "Ashley",
            "Dorothy",
            "Kimberly",
            "Emily",
            "Donna",
            "Michelle",
            "Carol",
            "Amanda",
            "Melissa",
            "Deborah",
            "Stephanie",
            "Rebecca",
            "Laura",
            "Sharon",
            "Cynthia"
        };

        private static readonly string[] LastNames = new string[] {
            "Smith",
            "Johnson",
            "Williams",
            "Jones",
            "Brown",
            "Davis",
            "Miller",
            "Wilson",
            "Moore",
            "Taylor",
            "Anderson",
            "Thomas",
            "Jackson",
            "White",
            "Harris",
            "Martin",
            "Thompson",
            "Garcia",
            "Martinez",
            "Robinson",
            "Clark",
            "Rodriguez",
            "Lewis",
            "Lee",
            "Walker",
            "Hall",
            "Allen",
            "Young",
            "Hernandez",
            "King",
            "Wright",
            "Lopez",
            "Hill",
            "Scott",
            "Green",
            "Adams",
            "Baker",
            "Gonzalez",
            "Nelson",
            "Carter",
            "Mitchell",
            "Perez",
            "Roberts",
            "Turner",
            "Phillips",
            "Campbell",
            "Parker",
            "Evans",
            "Edwards",
            "Collins",
            "Stewart",
            "Sanchez",
            "Morris",
            "Rogers",
            "Reed",
            "Cook",
            "Morgan",
            "Bell",
            "Murphy",
            "Bailey"
        };

        private static readonly string[] GarbageCharacters = new string[] {
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F"
        };

        public static bool Prefix(string Type, ref string __result)
        {
            switch (Type)
            {
                case "0x1 [Medical Asset]":
                    __result = GenerateNormalName("Dr.");
                    return false;

                case "0x4 [Leader Asset]":
                    __result = GenerateNormalName(LeaderTitles.GetRandomElement());
                    return false;

                case "0x2 [Philosophy Asset]":
                    __result = GenerateNormalName(PhilosopherTitles.GetRandomElement());
                    return false;

                case "0x3 [Aesthetic Asset]":
                case "0x5 [Security Asset]":
                case "0x6 [Survivalist Asset]":
                    __result = GenerateNormalName();
                    return false;

                case "0xDEADBEEF [843DF70BBD1A]":
                    __result = GenerateGarbageName();
                    return false;

                default:
                    return true;
            }
        }

        private static string GenerateNormalName(string prefix = "")
        {
            return (string.IsNullOrEmpty(prefix) ? "" : prefix + " ") + FirstNames.GetRandomElement() + " " + LastNames.GetRandomElement();
        }

        private static string GenerateGarbageName()
        {
            string name = "0x";
            int[] possibleLengths = { 2, 4, 4, 8, 8, 8, 8, 16 };
            int length = possibleLengths.GetRandomElement();
            for (int i = 0; i < length; ++i)
            {
                name += GarbageCharacters.GetRandomElement();
            }

            return name;
        }
    }
}