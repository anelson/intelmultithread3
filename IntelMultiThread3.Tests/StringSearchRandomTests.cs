using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace IntelMultiThread3.Tests {
    [TestFixture]
    public class StringSearchRandomTests {

        [Test]
        [Explicit]
        public void GenerateRandomTest() {
            RandomTestCaseGenerator tg = new RandomTestCaseGenerator();

            int letters;
            string s;
            string[] u;
            int[] k;
            int[] dist;

            tg.GenerateTestCase(200000, 10000,
                out letters,
                out s,
                out u,
                out k,
                out dist);

            PrintTestCase(letters, s, u, k, dist);
        }

        private void PrintTestCase(int letters, string s, string[] u, int[] k, int[] dist) {
            StringBuilder sb = new StringBuilder(s.Length + u[0].Length * u.Length + 1024);

            sb.AppendFormat(@"

			// random test case {2}
			new KnownAnswerTestCase(
				{0},
				""{1}"",
				new string[] {{", 
                letters, s, s.Length);

            foreach (String str in u) {
                sb.AppendFormat(@"
					""{0}"",", 
                    str);
            }

            sb.Append(@"
				},
				new int[] {");

            foreach (int n in k) {
                sb.AppendFormat(@"
					{0},", 
                    n);
            }

            sb.Append(@"
				},
				new int[] {");

            foreach (int n in dist) {
                sb.AppendFormat(@"
					{0},", 
                    n);
            }

            sb.Append(@"
				}),");

            Console.WriteLine("Generated test case:");
            Console.Write(sb.ToString());
        }
    }
}
