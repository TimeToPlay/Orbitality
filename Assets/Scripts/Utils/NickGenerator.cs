
using UnityEngine;

namespace Utils
{
    public class NickGenerator
    {
        private static string[] nicknames = {
            "Anheuser", "Jolly Roger", "Belch", "Leonidas", "Big Mac", "ManBearPig", "Bob Zombie", "Master Chief",
            "Boomhauer", "Mr. Clean", "Braveheart", "Mustache", "Brundon", "O'Doyle", "Captain Crunch", "Pablo",
            "Chewbacca", "Popeye", "Chubs", "Pork Chop", "Chum", "Rufio", "Derp", "Rumplestiltskin", "Django", "Snoopy",
            "Fight Club", "Spiderpig", "Flanders", "Spongebob", "Focker", "Spud", "Frodo", "Taco", "Frogger",
            "Turd Ferguson", "Gooch", "Uh-Huh", "Goonie", "Vader", "Gump", "Weiner", "Homer", "Wizzer", "Huggies",
            "Wonka", "The Hulk", "Wreck-it Ralph", "Jedi"
        };

        public static string GetRandomNickname()
        {
            return nicknames[Random.Range(0, nicknames.Length)];
        }
    }
}