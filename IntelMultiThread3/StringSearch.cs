using System;
using System.Threading;

public class StringSearch {
    private class SearchTaskInfo {
        public int[] s;
        public int[] u;
        public int idx;
        public volatile int optimalIndex;
        public AutoResetEvent doneEvent;
    }

    //private int[] _distTable;
	private int[,] _distTable;

    public int[] search(string S, string[] u, int letters) {
        buildDistTable(letters);

		//Convert the strings to int arrays for performance
		int[] intS = stringToIntArray(S);

        SearchTaskInfo[] tasks = new SearchTaskInfo[u.Length];

        for (int idx = 0; idx < u.Length; idx++) {
            tasks[idx] = new SearchTaskInfo();
            tasks[idx].s = intS;
            tasks[idx].u = stringToIntArray(u[idx]);
			tasks[idx].idx = idx;
            tasks[idx].doneEvent = new AutoResetEvent(false);

            ThreadPool.QueueUserWorkItem(new WaitCallback(findOptimalIndexCallback), tasks[idx]);
        }

        int[] optimalIndices = new int[u.Length];

        foreach (SearchTaskInfo task in tasks) {
            task.doneEvent.WaitOne();
            task.doneEvent.Close();
        }

        for (int idx = 0; idx < u.Length; idx++) {
            optimalIndices[idx] = tasks[idx].optimalIndex;
        }

        return optimalIndices;
    }
	
    public int[] naiveSearch(string S, string[] u, int letters) {
        buildDistTable(letters);

		int[] optimalIndices = new int[u.Length];

        int[] intS = stringToIntArray(S);
		for (int idx = 0; idx < u.Length; idx++) {
            optimalIndices[idx] = findOptimalIndex(intS, stringToIntArray(u[idx]), idx);
		}

		return optimalIndices;
    }

	public void buildDistTable(int letters) {
		//Build an array with enough elements to fit a 26*26
		//square array, but in a single dimension with 32 columns
		//to a row to enable speedy indexing by bit-shifting the row
		//index.
		//_distTable = new int[32*26];
		_distTable = new int[letters,letters];

		for (int i = 0; i < letters; i++) {
			for (int j = 0; j < letters; j++) {
                int distance = computeDist(i, 
										   j, 
										   letters);
				//_distTable[(i << 5) + j] = distance * distance;
				_distTable[i, j] = distance * distance;
			}
		}
	}

    /// <summary>Computes the circular distance between two characters in an alphabet of the first
    /// <code>letters</code> uppercase letters.
    /// 
    /// The distance between two letters is circular, for example, in an alphabet with
    /// 5 letters, the possible letters are A..E, and the distance between A and E is 1,
    /// as is the distance between A and B.
    /// 
    /// The algorithm to compute this distance is thus:
    /// 
    /// if the numeric difference between two letters is less than or equal floor(n/2), where
    /// n is the number of letters used in the alphabet, then the circular distance
    /// is the same as the numeric difference (eg, B - A = 1, thus dist = 1).
    /// However, if the numeric diff is more than floor(n/2), the circular distance
    /// is n - the numeric difference (eg, 5 - (E - A) = 5 - 4 = 1, thus dist = 1).
    /// 
    /// ASSUMPTION: If the distance from A to E in a 5 letter system is 1, then the distance
    /// from E to A in the same system is also 1.  Given the problem statement, in which
    /// dist() is squared prior to summation, the sign of the distance is likely not
    /// relevant, however for unit testing purposes a decision regarding sign must be made.</summary>
    /// 
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    /// <param name="letters"></param>
    /// 
    /// <returns>The square of the distance between two characters</returns>
	public int dist(int c1, int c2) {
        //return _distTable[(c1 << 5) + c2];		
        return _distTable[c1, c2];		
	}

    /// <summary>Computes the sum of the square of the dist() function for each character
    ///     in strings s and t, for the entire length of t and for s from i to i+|t|.</summary>
    /// 
    /// <param name="s"></param>
    /// <param name="t"></param>
    /// <param name="i"></param>
    /// <param name="letters">The number of letters used in the alphabet</param>
    /// 
    /// <returns></returns>
	public int stringDist(int[] s, int[] t, int i) {
		int totalDist = 0, tLength = t.Length;

        for (int j = 0; j < tLength; j++) {
            //INLINED
			// totalDist += dist(s[i+j], t[j]);
            //totalDist += _distTable[(s[i + j] << 5) + t[j]];
            totalDist += _distTable[s[i + j], t[j]];
		}

        return totalDist;
	}

    public void findOptimalIndexCallback(object state) {
        SearchTaskInfo sti = (SearchTaskInfo)state;

		int tickCount = Environment.TickCount;
		
		Console.WriteLine("Starting optimal index search at {0}",
						  DateTime.Now);

        sti.optimalIndex = findOptimalIndex(sti.s, sti.u, sti.idx);
		
		Console.WriteLine("Finished optimal index search at {0} ({1} ticks)",
						  DateTime.Now,
						  Environment.TickCount - tickCount);

        sti.doneEvent.Set();
    }

	public int findOptimalIndex(int[] s, int[] t, int uNumber) {
		int optimalIndex = 0;
		int minDist = Int32.MaxValue;
		int tLength = t.Length;

		//for (int idx = 0; idx <= s.Length - t.Length; idx++) {
		int idx = s.Length - tLength;
		do {
			// INLINED, plus some optimization
//             int dist = stringDist(s, t, idx);
//             if (dist < minDist) {
//                 minDist = dist;
//                 optimalIndex = idx;
//             }
			int totalDist = 0;
	
			//for (int j = 0; j < tLength; j++) {
			int j = tLength - 1;
			int idxPlusJ = idx + j + 1;
			do {
				//INLINED
				// totalDist += dist(s[i+j], t[j]);
				//totalDist += _distTable[(s[idx + j] << 5) + t[j]];
				//totalDist += _distTable[s[idx + j], t[j]];
				totalDist += _distTable[s[--idxPlusJ], t[j]];

				//OPTIMIZATION: if the total distance is already more
				//than the smallest we've seen so far, don't bother computing
				//the rest of the distance since it's obviously not optimal
				if (totalDist > minDist) {
					goto next;
				}
			} while (j-- != 0);
	
            if (totalDist < minDist) {
                minDist = totalDist;
                optimalIndex = idx;
            }

        next:
            ;
			if (idx % 10000 == 0) {
				Console.WriteLine("Tested indexes through {0} for u[{1}]",
								  idx,
								  uNumber);
			}
		} while (idx-- != 0);

		return optimalIndex;
	}
	
	public int computeDist(int c1, int c2, int letters) {
		if (c1 > c2) {
			int temp = c2;
			c2 = c1;
			c1 = temp;
		}

		int diff = c2 - c1;
		
		if (diff <= (letters>>1)) {
			return diff;
		} else {
			return letters - diff;
		}
	}

	public int[] stringToIntArray(string s) {
		int[] ns = new int[s.Length];
		int idx = 0;

		foreach (Char c in s) {
			ns[idx++] = c - 'A';
		}

        return ns;
	}
}
