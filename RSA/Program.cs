using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace RSA
{
    internal class Program
    {
        public static readonly List<int> firstPrimesList = new List<int>
    {
        2, 3, 5, 7, 11, 13, 17, 19, 23, 29,
        31, 37, 41, 43, 47, 53, 59, 61, 67,
        71, 73, 79, 83, 89, 97, 101, 103,
        107, 109, 113, 127, 131, 137, 139,
        149, 151, 157, 163, 167, 173, 179,
        181, 191, 193, 197, 199, 211, 223,
        227, 229, 233, 239, 241, 251, 257,
        263, 269, 271, 277, 281, 283, 293,
        307, 311, 313, 317, 331, 337, 347, 349
    };


        public static void Main()
        {
            // Крок 1: Вибрати два простих числа p і q
            BigInteger p = GetBigPrime();
            BigInteger q = GetBigPrime();
           // BigInteger p = 61;
           // BigInteger q = 53;

            Console.WriteLine("q: " + q);
            Console.WriteLine("p: " + p);
            BigInteger n = p * q;
            Console.WriteLine($"n = {n}");

            // Крок 3: Обчислити m = (p - 1) * (q - 1)
            BigInteger m = (p - 1) * (q - 1);
            Console.WriteLine($"m = {m}");

            // Крок 4: Вибрати число e, взаємно просте з m
            BigInteger e = 0;
            for (BigInteger i = 3; i < m; i += 2)
            {
                if (EuclidAlgorithm(i, m) == 1)
                {
                    e = i;
                    Console.WriteLine($"e = {e} (відкритий ключ)");
                    break;
                }
            }

            // Крок 5: Обчислити d таке, що (e * d) % m = 1, використовуючи gcdExtended
            BigInteger x = 0, y = 0;
            BigInteger d = gcdExtended(e, m, ref x, ref y);
            d = x;
            if (d < 0)
            {
                d += m;
            }
            Console.WriteLine("privat key d " + d);
            // Шифрування повідомлення
            // BigInteger message = 42; 
            //BigInteger encrypted = Encrypt(message, e, n);
            //Console.WriteLine($"Зашифроване повідомлення: {encrypted}");

            string inputFilePath = "input.txt";
            string outputFilePath = "encrypted.txt";
            string decryptedFilePath = "decrypted.txt";

            string inputText = File.ReadAllText(inputFilePath);
            Console.WriteLine("Input text length: " + inputText.Length);
             int blockSize = (int)(BigInteger.Log(n, 256)); // Або 3, в залежності від величини n

            // Шифрування
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                for (int i = 0; i < inputText.Length; i += blockSize)
                {
                    // Отримати блок
                    string block = inputText.Substring(i, Math.Min(blockSize, inputText.Length - i));

                    // Перетворення блоку в число
                    BigInteger message = 0;
                    foreach (char c in block)
                    {
                        message = message * 256 + (BigInteger)c; // Використовуємо 256 для кодування символів
                    }

                    // Перевірка, чи не перевищує значення n
                    if (message >= n)
                    {
                        Console.WriteLine($"Block '{block}' exceeds the maximum value for n.");
                        continue; // пропускаємо блок, якщо він занадто великий
                    }

                    BigInteger encrypted = Encrypt(message, e, n);
                    writer.WriteLine(encrypted);
                }
            }

            // Розшифрування
            using (StreamReader reader = new StreamReader(outputFilePath))
            using (StreamWriter writer = new StreamWriter(decryptedFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (BigInteger.TryParse(line, out BigInteger encryptedMessage))
                    {
                        BigInteger decrypted = Decrypt(encryptedMessage, d, n);

                        // Відновлення символів з розшифрованого значення
                        // Використовуємо список для зберігання символів
                        StringBuilder decryptedMessage = new StringBuilder();

                        while (decrypted > 0)
                        {
                            char c = (char)(decrypted % 256);
                            decryptedMessage.Insert(0, c); // Додаємо символ на початок, щоб зберегти порядок
                            decrypted /= 256; // Переходимо до наступного символу
                        }

                        writer.Write(decryptedMessage.ToString()); // Записуємо розшифроване повідомлення в файл
                    }
                    else
                    {
                        Console.WriteLine($"Failed to parse encrypted message: {line}");
                    }
                }
            }

            Console.WriteLine("Шифрування та розшифрування завершено.");
        
    }

            static BigInteger EuclidAlgorithm(BigInteger r1, BigInteger r2)
        {
            while (r2 != 0)
            {
                BigInteger temp = r2;
                r2 = r1 % r2;
                r1 = temp;
            }

            return r1;
        }

        public static BigInteger gcdExtended(BigInteger a, BigInteger b, ref BigInteger x, ref BigInteger y)
        {
            // Base Case
            if (a == 0)
            {
                x = 0;
                y = 1;
                return b;
            }
            // To store results of
            // recursive call
            BigInteger x1 = 1, y1 = 1;
            BigInteger gcd = gcdExtended(b % a, a, ref x1, ref y1);
            // Update x and y using
            // results of recursive call
            x = y1 - (b / a) * x1;
            y = x1;
            return gcd;
        }

        public static BigInteger Encrypt(BigInteger message, BigInteger e, BigInteger n)
        {
            return BigInteger.ModPow(message, e, n);
        }

        public static BigInteger Decrypt(BigInteger encryptedMessage, BigInteger d, BigInteger n)
        {
            return BigInteger.ModPow(encryptedMessage, d, n);
        }


        public static BigInteger MulMod(BigInteger a, BigInteger b, BigInteger m)
        {
            BigInteger res = 0;
            a %= m;

            while (a > 0)
            {
                if ((a & 1) == 1)
                {
                    res = (res + b) % m;
                }
                a >>= 1; // ділимо на 2
                b = (b << 1) % m; // множимо на 2
            }
            return res;
        }

        public static BigInteger PowMod(BigInteger a, BigInteger b, BigInteger n)
        {
            BigInteger x = 1;
            a %= n;

            while (b > 0)
            {
                if (b % 2 == 1)
                {
                    x = MulMod(x, a, n);
                }
                a = MulMod(a, a, n);
                b >>= 1; // ділимо на 2
            }
            return x % n;
        }

        public static BigInteger GetRandom64()
        {
            Random rand = new Random();
            byte[] bytes = new byte[8];
            rand.NextBytes(bytes);

            // Встановлюємо старший біт у 0, щоб уникнути негативних значень
            bytes[7] &= 0x7F;

            return new BigInteger(bytes);
        }

        public static BigInteger GetLowLevelPrime()
        {
            while (true)
            {
                BigInteger candidate = GetRandom64();
                bool isPrime = true;

                // Перевірка на ділиться на прості числа з firstPrimes
                foreach (int prime in firstPrimesList)
                {
                    if (candidate == prime)
                        return candidate;

                    if (candidate % prime == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }

                if (isPrime)
                    return candidate;
            }
        }

        public static bool TrialComposite(BigInteger a, BigInteger evenC, BigInteger toTest, int maxDiv2)
        {
            if (PowMod(a, evenC, toTest) == 1)
                return false;

            for (int i = 0; i < maxDiv2; i++)
            {
                BigInteger temp = BigInteger.Pow(2, i);
                if (PowMod(a, temp * evenC, toTest) == toTest - 1)
                    return false;
            }

            return true;
        }

        public static bool MillerRabinTest(BigInteger toTest)
        {
            const int accuracy = 20;
            int maxDiv2 = 0;
            BigInteger evenC = toTest - 1;

            while (evenC % 2 == 0)
            {
                evenC /= 2;
                maxDiv2++;
            }

            Random rand = new Random();

            for (int i = 0; i < accuracy; i++)
            {
                BigInteger a = GetRandom64() % (toTest - 4) + 2;

                if (TrialComposite(a, evenC, toTest, maxDiv2))
                {
                    return false;
                }
            }

            return true;
        }

        public static BigInteger GetBigPrime()
        {
            while (true)
            {
                BigInteger candidate = GetLowLevelPrime();

                // Перевірка на позитивність кандидата
                if (candidate < 0)
                {
                    Console.WriteLine("Generated a negative candidate, regenerating...");
                    continue; // Продовжуємо, якщо число негативне
                }

                if (MillerRabinTest(candidate))
                    return candidate;
            }
        }

    }
}
