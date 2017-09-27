using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace Assignment_1
{
    class Program
    {
        static void Main(string[] args)
        {   
            if (args.Length < 2) {
                throw new Exception ("Not enough arguments: {1} 0 - Brute Force, 1 - Efficient | {2} Name of input File");
            }

            var whichAlgoArg = Int32.Parse(args[1]);


            string fileName;
            if (args.Length > 1) {
                fileName = args[2];
            } else {
                fileName = "./inputFile.txt";
            }

            var pointsList = ExtractPoints (fileName);

            Stopwatch stopWatch = new Stopwatch ();

            List<Point> hullList;
            if (whichAlgoArg == 0) {
                stopWatch.Start ();
                hullList = ConvexHullAlgorithm.BruteForce (pointsList);
                stopWatch.Stop ();
                Console.WriteLine ("Brute Force Time: " + (double) stopWatch.Elapsed.Milliseconds / 1000.0 + " seconds.");
            } else if (whichAlgoArg == 1){
                stopWatch.Start ();
                hullList = ConvexHullAlgorithm.Efficient (pointsList);
                stopWatch.Stop ();
                Console.WriteLine ("Efficient Time: " + (double) stopWatch.Elapsed.Milliseconds / 1000.0 + " seconds.");
            } else {
                throw new Exception ("Only a 0 (Brute Force) or a 1 (Efficient) can be passed as your second argument.");
            }

            TextWriter resultsTW = new StreamWriter ("./resultsFile.txt");
            resultsTW.WriteLine (hullList.Count);
            Console.WriteLine ("ConvexHull Results:");
            Console.WriteLine ("\t" + hullList.Count);
            foreach (Point p in hullList) {
                Console.WriteLine ("\t" + p.x + " " + p.y);
                resultsTW.WriteLine (p.x + " " + p.y);
            }
            resultsTW.Close ();
        }

        private static List<Point> ExtractPoints (string fileName) {
            TextReader inputTR;
            try {
                inputTR = new StreamReader (fileName);
            } catch (Exception e) {
                throw e;
            }

            int inputCount;
            if ( !Int32.TryParse (inputTR.ReadLine (), out inputCount)) {
                inputTR.Close ();
                throw new Exception ("First line in inputFile is not a readable integer count of the number of points.");
            }

            var pointList = new List <Point> (inputCount);

            for (int i = 0; i < inputCount; i++) {
                string line = inputTR.ReadLine ();
                // Split the line by spaces to get the two individual nums
                var stringNums = line.Split (' ');
                var x = Int32.Parse (stringNums[0]);
                var y = Int32.Parse (stringNums[1]);

                pointList.Add (new Point (x, y));
            }

            // Close stream reader
            inputTR.Close ();

            return pointList;
        }
    }

    public class ConvexHullAlgorithm {
        public static List<Point> BruteForce (List<Point> pointList) {
            // Sort by y value
            //pointList.Sort ((a, b) => (a.y.CompareTo (b.y)));
            pointList.Sort ();
            var hullPointList = new List<Point> ();

            // The lowest point has to be on the hull, so add it to the list
            hullPointList.Add (pointList[0]);
            var done = false;
            while (!done) {
                var a = hullPointList[hullPointList.Count - 1];
                foreach (Point b in pointList) {
                    // Skip if b is equal to a
                    if (a.x == b.x && a.y == b.y) continue;

                    var isHull = true;
                    foreach (Point p in pointList) {
                        if (IsRight (a, b, p)) {
                            isHull = false;
                            break;
                        }
                    }
                    if (isHull) {
                        // If the final point is equal to the first point then we've circled the hull
                        if (hullPointList[0].CompareTo(b) == 0) {
                            done = true;
                            break;
                        }
                        hullPointList.Add (b);
                    }
                }
            }            
            return hullPointList;
        }

        public static List<Point> Efficient (List<Point> pointList) {
            pointList.Sort ();
            Point startPoint = pointList[0];

            pointList.RemoveAt (0);

            // Sort by Polar Coordinates using the start point as the starting point
            pointList.Sort ((a, b) => ((Math.Atan2 (a.y - startPoint.y, a.x - startPoint.x) - Math.Atan2 (b.y - startPoint.y, b.x - startPoint.x)) < 0 ? -1 : 1));
            pointList.Insert (0, startPoint);

            var hullLinkedList = new LinkedList<Point> ();
            hullLinkedList.AddLast (pointList[0]);
            hullLinkedList.AddLast (pointList[1]);
            hullLinkedList.AddLast (pointList[2]);

            for (int i = 3; i < pointList.Count; i++) {
                Point p = pointList[i];
                
                while (IsRight (hullLinkedList.Last.Previous.Value, hullLinkedList.Last.Value, p)) {
                    hullLinkedList.RemoveLast ();
                }
                hullLinkedList.AddLast (p);
            }

            var hullList = new List<Point> ();;
            foreach (Point p in hullLinkedList) {
                hullList.Add (p);
            }

            return hullList;
        }

        // If the given point is to the left of the line, return true, false otherwise
        private static bool IsLeft (Point a, Point b, Point point) {
            var determinant = ((b.x - a.x) * (point.y - a.y)) - ((b.y - a.y) * (point.x - a.x));
            return determinant > 0 ? true : false;
        }

        // If the given point is to the right of the line, return true, false otherwise
        private static bool IsRight (Point a, Point b, Point point) {
            var determinant = ((b.x - a.x) * (point.y - a.y)) - ((b.y - a.y) * (point.x - a.x));
            return determinant < 0 ? true : false;
        }
    }

    public struct Point : IComparable {
        public int x {get; private set;}
        public int y {get; private set;}

        public Point (int x, int y) {
            this.x = x;
            this.y = y;
        }

        public double Magnitude() {
            return Math.Sqrt ((x * x) + (y * y));
        }

        public int CompareTo(object obj)
        {   
            var p = (Point) obj;

            // Compare by Y
            if (y < p.y) return -1;
            else if (y > p.y) return 1;
            // Compare by X if Ys are equals
            if (x < p.x) return -1;
            else if (x > p.x) return 1;
            // Otherwise they're both equal points
            return 0;
        }
    }
}
