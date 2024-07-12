namespace PhylogeneticApp.DataStructures;

using System;
using System.Collections.Concurrent;

class UpperTrianglePartitioner : Partitioner<Tuple<int, int>>
{
    private readonly int _size;
    private readonly int _numElements;

    public UpperTrianglePartitioner(int size)
    {
        _size = size;
        _numElements = size * (size - 1) / 2;
    }

    public override IList<IEnumerator<Tuple<int, int>>> GetPartitions(int partitionCount)
    {
        var partitions = new List<IEnumerator<Tuple<int, int>>>();
        int numElementsPerPartition = _numElements / partitionCount;

        int remainingElements = _numElements;
        int startIndex = 0;
        for (int i = 0; i < partitionCount; i++)
        {
            int elementsInPartition = Math.Min(numElementsPerPartition, remainingElements);
            partitions.Add(CreatePartitionEnumerator(startIndex, elementsInPartition));
            remainingElements -= elementsInPartition;
            startIndex += elementsInPartition;
        }

        return partitions;
    }
    
    public override IEnumerable<Tuple<int, int>> GetDynamicPartitions()
    {
        return Partition(_numElements);
    }

    public override bool SupportsDynamicPartitions => true;

    private IEnumerable<Tuple<int, int>> Partition(int totalElements)
    {
        int currentIndex = 0;
        int diagonal = 0;
        while (currentIndex < totalElements)
        {
            // Calculate the corresponding row and column indices for the upper triangle element
            int row = _size - 1 - (int)Math.Floor(Math.Sqrt(-8 * currentIndex + 4 * _size * (_size - 1) + 1) / 2.0 - 0.5);
            int col = currentIndex + row * (_size - row + _size - 1) / 2;

            if (col == diagonal) // Skip diagonal elements
            {
                currentIndex += _size - row; // Move to the next row
                diagonal++;
                continue;
            }

            yield return Tuple.Create(row, col);
            currentIndex++;
        }
    }

    private IEnumerator<Tuple<int, int>> CreatePartitionEnumerator(int startIndex, int count)
    {
        int currentIndex = startIndex;
        int endIndex = startIndex + count;
        while (currentIndex < endIndex)
        {
            // Calculate the corresponding row and column indices for the upper triangle element
            int row = _size - 1 - (int)Math.Floor(Math.Sqrt(-8 * currentIndex + 4 * _size * (_size - 1) + 1) / 2.0 - 0.5);
            int col = currentIndex + row * (_size - row + _size - 1) / 2;
            yield return Tuple.Create(row, col);
            currentIndex++;
        }
    }
}