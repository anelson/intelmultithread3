using System;
using System.Collections.Generic;
using System.Text;

namespace IntelMultiThread3.Tests {
    class RandomTestCaseGenerator {
        private Random _rand = new Random();

        public void GenerateTestCase(int maxS, int maxU, out int letters, out string s, out string[] u, out int[] k, out int[] dist) {
			letters = generateLetters();
            int lengthOfS = generateSLength(maxS);
            int[] lengthOfU = generateULengths(lengthOfS, maxU);

			s = generateS(lengthOfS, letters);

			generateU(s, lengthOfU, letters, out u, out k, out dist);
        }

		private int generateLetters() {
			Random rnd = new Random();

			return rnd.Next(4, 26);
		}

		private int generateSLength(int maxS) {
            return (int)(maxS * (1 - _rand.NextDouble() * _rand.NextDouble()));
		}
		
		private int[] generateULengths(int lengthOfS, int maxU) {
			int[] uLens = new int[16]; //According to problem statement, always 16 u's

			for (int idx = 0; idx < uLens.Length; idx++) {
                uLens[idx] = (int)(maxU * (1 - _rand.NextDouble() * _rand.NextDouble()));

				//Make sure the resultant length is neither too short nor too long
				if (uLens[idx] < 1000) {
					uLens[idx] = 1000;
				} 

				if (uLens[idx] > lengthOfS / 2) {
					uLens[idx] = lengthOfS / 2;
				}
			}
			
			return uLens;
		}

		private string generateS(int lengthOfS, int letters) {
			StringBuilder sb = new StringBuilder(lengthOfS);

			for (int count = 0; count < lengthOfS; count++) {
                sb.Append((Char)('A' + _rand.Next(0, letters - 1)));
			}

			return sb.ToString();
		}

		private void generateU(string s, int[] lengthOfU, int letters, out string[] u, out int[] k, out int [] dist) {
            u = new string[lengthOfU.Length];
            k = new int[lengthOfU.Length];
            dist = new int[lengthOfU.Length];

			for (int idx = 0; idx < lengthOfU.Length; idx++) {
				string singleU;
				int singleK, singleDist;
				
				generateSingleU(s, 
								lengthOfU[idx],
                                letters,
								out singleU,
								out singleK, 
								out singleDist);

				u[idx] = singleU;
				k[idx] = singleK;
				dist[idx] = singleDist;
			}
		}

		private void generateSingleU(string s, int lengthOfU, int letters, out string u, out int k, out int dist) {
			//Select a substring of s to use as the starting point for U
            int sIdx = _rand.Next(0, s.Length - lengthOfU);

            StringBuilder sb = new StringBuilder(lengthOfU);
			sb.Append(s, sIdx, lengthOfU);

			//Select the permutation boundary parameter k from 1 to 26
            k = _rand.Next(1, 26);

            //Build the array of probabilities used to pick a value for x at each char
            //in u
            double[] probabilityRange = generateProbabilityRange(k);

            for (int i = 0; i < lengthOfU; i++) {
                sb[i] = permuteChar(sb[i], probabilityRange, k, letters);
            }

            u = sb.ToString();
            dist = Int32.MaxValue;
		}

        private double[] generateProbabilityRange(int k) {
            double totalProbability = 0;

            for (int n = -k; n <= k; n++) {
                totalProbability += (1 / (Math.Abs((double)n) + 1));
            }

            double scalingFactor = 1 / totalProbability;

            double[] probabilityRange = new double[2 * k + 1];

            totalProbability = 0;
            for (int n = -k; n <= k; n++) {
                totalProbability += (1 / (Math.Abs((double)n) + 1)) * scalingFactor;
                probabilityRange[k + n] = totalProbability;
            }

            return probabilityRange;
        }

        private char permuteChar(char p, double[] probabilityRange, int k, int letters) {
            //Select a value for x, which will range from -k to k, with which the character
            //will be permuted
            int x = generateX(probabilityRange, k);

            //Convert the character to a 0-based integer
            int newChar = p - 'A';

            //Apply the x
            newChar += x;

            //Fix if negative or too large
            while (newChar < 0) {
                newChar += letters;
            }

            if (newChar >= letters) {
                newChar %= letters;
            }

            return (char)('A' + newChar);
        }

        private int generateX(double[] probabilityRange, int k) {
            double rand = _rand.NextDouble();

            for (int x = -k; x <= k; x++) {
                if (rand < probabilityRange[k + x]) {
                    return x;
                }
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
