using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoTrexia.Core
{
    public static class AnswerGenerator
    {
        private static readonly Random _random = new();

        public static List<string> GenerateOptions(
            string correctAnswer,
            List<string> allAnswers)
        {
            var pool = allAnswers
                .Where(a => a != correctAnswer)
                .Distinct()
                .ToList();

            if (pool.Count < 2)
            {
                throw new InvalidOperationException("At least two incorrect answers are required.");
            }

            var options = new List<string> { correctAnswer };

            while (options.Count < 3)
            {
                var candidate = pool[_random.Next(pool.Count)];

                if (!options.Contains(candidate))
                {
                    options.Add(candidate);
                }
            }

            return options
                .OrderBy(x => _random.Next())
                .ToList();
        }
    }
}
