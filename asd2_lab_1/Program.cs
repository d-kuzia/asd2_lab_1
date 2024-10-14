using System;
using System.IO;

class ExternalSort
{
    static void Main(string[] args)
    {
        string filePath = "random_nums.txt";
        int fileSizeMB = 10;
        int arraySize = (fileSizeMB * 1024 * 1024) / sizeof(int);

        GenerateRandomFile(filePath, arraySize);
        Console.WriteLine("File with random nums has been created");
    }
}
