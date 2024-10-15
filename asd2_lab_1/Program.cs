using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class ExternalSort
{
    static void Main(string[] args)
    {
        // Default sort ----------------------------------------------------------------------------
        /*string inputFilePath = "random_numbers.txt";
        string tempFolder = "temp";
        string outputFilePath = "sorted_numbers.txt";
        int fileSizeMB = 10;
        int chunkSizeMB = 100;
        int arraySize = (fileSizeMB * 1024 * 1024) / sizeof(int);
        int chunkSize = (chunkSizeMB * 1024 * 1024) / sizeof(int);

        Directory.CreateDirectory(tempFolder);

        // Generate file with random nums
        GenerateRandomFile(inputFilePath, arraySize);
        Console.WriteLine("File with random nums has been created.");

        // Create and sort chunks
        CreateSortedChunks(inputFilePath, tempFolder, chunkSize);
        Console.WriteLine("Chunks have been created and sorted.");

        // Merging chunks to output file
        MultiWayMerge(tempFolder, outputFilePath);
        Console.WriteLine("File has been sorted.");*/
        //------------------------------------------------------------------------------------------

        string inputFilePath = "largeFile_1GB.bin";
        string tempFolder = "temp";
        string outputFilePath = "sorted_largeFile_1GB.bin";
        int maxMemorySizeMB = 512;

        GenerateLargeFile(inputFilePath, 1L * 1024 * 1024 * 1024);  // 1gb

        if (!Directory.Exists(tempFolder))
        {
            Directory.CreateDirectory(tempFolder);
        }

        CreateSortedChunks(inputFilePath, tempFolder, maxMemorySizeMB);

        MultiWayMerge(tempFolder, outputFilePath);

        Console.WriteLine("Sort done!");

        // nums to console -------------------------------------------------------------------------
        /*using (BinaryReader reader = new BinaryReader(File.Open(outputFilePath, FileMode.Open)))
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                int number = reader.ReadInt32();
                Console.WriteLine(number);
            }
        }*/
        //------------------------------------------------------------------------------------------
    }

    static void GenerateRandomFile(string filePath, int count)
    {
        Random rand = new Random();
        using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            for (int i = 0; i < count; i++)
            {
                int randomNumber = rand.Next();
                writer.Write(randomNumber);
            }
        }
    }

    static void GenerateLargeFile(string filePath, long fileSize)
    {
        int bufferSize = 1024 * 1024; // 1mb
        Random random = new Random();

        using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            long writtenBytes = 0;
            while (writtenBytes < fileSize)
            {
                int[] buffer = new int[bufferSize / sizeof(int)];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = random.Next();
                }

                foreach (int number in buffer)
                {
                    writer.Write(number);
                }

                writtenBytes += bufferSize;
                Console.WriteLine($"{writtenBytes / (1024 * 1024)} MB written...");
            }
        }

        Console.WriteLine("1 GB file generation complete.");
    }

    /*static void CreateSortedChunks(string inputFilePath, string tempFolder, int chunkSize)
    {
        using (BinaryReader reader = new BinaryReader(File.Open(inputFilePath, FileMode.Open)))
        {
            int chunkIndex = 0;
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                // Read chunk
                int[] chunk = new int[Math.Min(chunkSize, (int)(reader.BaseStream.Length - reader.BaseStream.Position) / sizeof(int))];
                for (int i = 0; i < chunk.Length; i++)
                {
                    chunk[i] = reader.ReadInt32();
                }

                Array.Sort(chunk);

                // Sorted chunk to temp file
                string tempFilePath = Path.Combine(tempFolder, $"chunk_{chunkIndex}.bin");
                using (BinaryWriter writer = new BinaryWriter(File.Open(tempFilePath, FileMode.Create)))
                {
                    foreach (int number in chunk)
                    {
                        writer.Write(number);
                    }
                }
                chunkIndex++;
            }
        }
    }*/
    static void CreateSortedChunks(string inputFilePath, string tempFolder, int maxMemorySizeMB)
    {
        int chunkSize = (maxMemorySizeMB * 1024 * 1024) / sizeof(int); // Конвертуємо в кількість цілих чисел
        using (BinaryReader reader = new BinaryReader(File.Open(inputFilePath, FileMode.Open)))
        {
            int chunkIndex = 0;
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                // Read 512mb chunk
                int[] chunk = new int[Math.Min(chunkSize, (int)(reader.BaseStream.Length - reader.BaseStream.Position) / sizeof(int))];
                for (int i = 0; i < chunk.Length; i++)
                {
                    chunk[i] = reader.ReadInt32();
                }

                Array.Sort(chunk);

                // Sorted chunk to temp file
                string tempFilePath = Path.Combine(tempFolder, $"chunk_{chunkIndex}.bin");
                using (BinaryWriter writer = new BinaryWriter(File.Open(tempFilePath, FileMode.Create)))
                {
                    foreach (int number in chunk)
                    {
                        writer.Write(number);
                    }
                }
                chunkIndex++;
            }
        }
    }

    static void MultiWayMerge(string tempFolder, string outputFilePath)
    {
        string[] tempFiles = Directory.GetFiles(tempFolder, "chunk_*.bin");
        int k = tempFiles.Length;

        List<BinaryReader> readers = new List<BinaryReader>();
        using (BinaryWriter writer = new BinaryWriter(File.Open(outputFilePath, FileMode.Create)))
        {
            // Open all temps to reaD
            foreach (string file in tempFiles)
            {
                readers.Add(new BinaryReader(File.Open(file, FileMode.Open)));
            }

            // Priority queue for merging
            SortedSet<Tuple<int, int>> priorityQueue = new SortedSet<Tuple<int, int>>(Comparer<Tuple<int, int>>.Create((x, y) =>
            {
                int result = x.Item1.CompareTo(y.Item1);
                return result != 0 ? result : x.Item2.CompareTo(y.Item2);
            }));

            // Read first nums to add to priority queue
            for (int i = 0; i < readers.Count; i++)
            {
                if (readers[i].BaseStream.Position < readers[i].BaseStream.Length)
                {
                    int number = readers[i].ReadInt32();
                    priorityQueue.Add(Tuple.Create(number, i));
                }
            }

            // Merging
            while (priorityQueue.Count > 0)
            {
                var smallest = priorityQueue.Min;
                priorityQueue.Remove(smallest);

                // Smallest to output file
                writer.Write(smallest.Item1);

                int readerIndex = smallest.Item2;
                if (readers[readerIndex].BaseStream.Position < readers[readerIndex].BaseStream.Length)
                {
                    int nextNumber = readers[readerIndex].ReadInt32();
                    priorityQueue.Add(Tuple.Create(nextNumber, readerIndex));
                }
            }

            // Close readers
            foreach (var reader in readers)
            {
                reader.Close();
            }
        }
    }
}
