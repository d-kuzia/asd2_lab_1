using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class ExternalSort
{
    // Generate 1gb file
    static void GenerateLargeTextFile(string filePath, int numberOfIntegers)
    {
        Random random = new Random();

        using (StreamWriter writer = new StreamWriter(File.Open(filePath, FileMode.Create)))
        {
            for (int i = 0; i < numberOfIntegers; i++)
            {
                int number = random.Next(-100_000_000, 100_000_000);
                writer.WriteLine(number);
            }
        }
    }

    // Create chunks
    static void CreateSortedChunks(string inputFilePath, string tempFolder, int chunkSize)
    {
        using (StreamReader reader = new StreamReader(File.Open(inputFilePath, FileMode.Open)))
        {
            int chunkIndex = 0;
            while (!reader.EndOfStream)
            {
                List<int> chunk = new List<int>();
                for (int i = 0; i < chunkSize && !reader.EndOfStream; i++)
                {
                    chunk.Add(int.Parse(reader.ReadLine()));
                }

                chunk.Sort();

                string tempFilePath = Path.Combine(tempFolder, $"chunk_{chunkIndex}.txt");
                using (StreamWriter writer = new StreamWriter(File.Open(tempFilePath, FileMode.Create)))
                {
                    foreach (int number in chunk)
                    {
                        writer.WriteLine(number);
                    }
                }
                chunkIndex++;
            }
        }
    }

    // Merge
    static void MultiWayMerge(string tempFolder, string outputFilePath)
    {
        string[] tempFiles = Directory.GetFiles(tempFolder, "chunk_*.txt");
        int k = tempFiles.Length;

        List<StreamReader> readers = new List<StreamReader>();
        using (StreamWriter writer = new StreamWriter(File.Open(outputFilePath, FileMode.Create)))
        {
            foreach (string file in tempFiles)
            {
                readers.Add(new StreamReader(File.Open(file, FileMode.Open)));
            }

            SortedSet<Tuple<int, int>> priorityQueue = new SortedSet<Tuple<int, int>>(Comparer<Tuple<int, int>>.Create((x, y) =>
            {
                int result = x.Item1.CompareTo(y.Item1);
                return result != 0 ? result : x.Item2.CompareTo(y.Item2);
            }));

            for (int i = 0; i < readers.Count; i++)
            {
                if (!readers[i].EndOfStream)
                {
                    int number = int.Parse(readers[i].ReadLine());
                    priorityQueue.Add(Tuple.Create(number, i));
                }
            }

            while (priorityQueue.Count > 0)
            {
                var smallest = priorityQueue.Min;
                priorityQueue.Remove(smallest);

                writer.WriteLine(smallest.Item1);

                int readerIndex = smallest.Item2;
                if (!readers[readerIndex].EndOfStream)
                {
                    int nextNumber = int.Parse(readers[readerIndex].ReadLine());
                    priorityQueue.Add(Tuple.Create(nextNumber, readerIndex));
                }
            }

            // Закриваємо всі читачі
            foreach (var reader in readers)
            {
                reader.Close();
            }
        }
    }

    // Main
    static void Main(string[] args)
    {
        string inputFilePath = "large_file.txt";
        string tempFolder = "temp_chunks";
        string outputFilePath = "sorted_large_file.txt";

        int numberOfIntegers = 100_000_000; // 1гб
        int chunkSize = 25_000_000;         // 256гб

        // File
        Console.WriteLine("Generating a large file...");
        GenerateLargeTextFile(inputFilePath, numberOfIntegers);

        // Chunks
        Console.WriteLine("Creating and sorting chunks...");
        Directory.CreateDirectory(tempFolder);
        CreateSortedChunks(inputFilePath, tempFolder, chunkSize);

        // Merge
        Console.WriteLine("Merging sorted chunks...");
        MultiWayMerge(tempFolder, outputFilePath);

        Console.WriteLine("Sorting is complete. Result in file: " + outputFilePath);
    }
}
