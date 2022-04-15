using System.Linq;

namespace ED.Domain
{
    public static class ResourceHelper
    {
        public static string CyrillicToLatin(string? text)
        {
            return text == null
                ? string.Empty
                : string.Join("", text.Select(e => GetLatinCharacter(e)));
        }

        private static string GetLatinCharacter(char c)
        {
            return c switch
            {
                'а' => "a",
                'б' => "b",
                'в' => "v",
                'г' => "g",
                'д' => "d",
                'е' => "e",
                'ж' => "zh",
                'з' => "z",
                'и' => "i",
                'й' => "y",
                'к' => "k",
                'л' => "l",
                'м' => "m",
                'н' => "n",
                'о' => "o",
                'п' => "p",
                'р' => "r",
                'с' => "s",
                'т' => "t",
                'у' => "u",
                'ф' => "f",
                'х' => "h",
                'ц' => "ts",
                'ч' => "ch",
                'ш' => "sh",
                'щ' => "sht",
                'ь' => "y",
                'ъ' => "a",
                'ю' => "yu",
                'я' => "ya",
                'А' => "A",
                'Б' => "B",
                'В' => "V",
                'Г' => "G",
                'Д' => "D",
                'Е' => "E",
                'Ж' => "Zh",
                'З' => "Z",
                'И' => "I",
                'Й' => "Y",
                'К' => "K",
                'Л' => "L",
                'М' => "M",
                'Н' => "N",
                'О' => "O",
                'П' => "P",
                'Р' => "R",
                'С' => "S",
                'Т' => "T",
                'У' => "U",
                'Ф' => "F",
                'Х' => "H",
                'Ц' => "Ts",
                'Ч' => "Ch",
                'Ш' => "Sh",
                'Щ' => "Sht",
                'Ъ' => "A",
                'Ю' => "Yu",
                'Я' => "Ya",
                '№' => "N",
                _ => c.ToString(),
            };
        }
    }
}
