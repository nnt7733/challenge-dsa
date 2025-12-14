namespace MinRide.Algorithms;

/// <summary>
/// Provides various sorting algorithm implementations.
/// </summary>
public static class SortAlgorithms
{
    /// <summary>
    /// Sorts a list using the merge sort algorithm.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to sort.</param>
    /// <param name="comparison">A comparison delegate to compare elements.</param>
    /// <returns>A new sorted list.</returns>
    /// <remarks>
    /// Time Complexity: O(n log n) in all cases (best, average, and worst).
    /// Space Complexity: O(n) for the temporary arrays used during merging.
    /// This is a stable sorting algorithm.
    /// </remarks>
    public static List<T> MergeSort<T>(List<T> list, Comparison<T> comparison)
    {
        // Base case: list with 0 or 1 element is already sorted
        if (list.Count <= 1)
        {
            return new List<T>(list);
        }

        // Split list into two halves
        int mid = list.Count / 2;
        List<T> left = list.GetRange(0, mid);
        List<T> right = list.GetRange(mid, list.Count - mid);

        // Recursively sort each half
        left = MergeSort(left, comparison);
        right = MergeSort(right, comparison);

        // Merge sorted halves
        return Merge(left, right, comparison);
    }

    /// <summary>
    /// Merges two sorted lists into a single sorted list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the lists.</typeparam>
    /// <param name="left">The first sorted list.</param>
    /// <param name="right">The second sorted list.</param>
    /// <param name="comparison">A comparison delegate to compare elements.</param>
    /// <returns>A merged sorted list containing all elements from both input lists.</returns>
    private static List<T> Merge<T>(List<T> left, List<T> right, Comparison<T> comparison)
    {
        List<T> result = new List<T>(left.Count + right.Count);
        int leftIndex = 0;
        int rightIndex = 0;

        // Compare elements from both lists and add the smaller one
        while (leftIndex < left.Count && rightIndex < right.Count)
        {
            if (comparison(left[leftIndex], right[rightIndex]) <= 0)
            {
                result.Add(left[leftIndex]);
                leftIndex++;
            }
            else
            {
                result.Add(right[rightIndex]);
                rightIndex++;
            }
        }

        // Add remaining elements from left list
        while (leftIndex < left.Count)
        {
            result.Add(left[leftIndex]);
            leftIndex++;
        }

        // Add remaining elements from right list
        while (rightIndex < right.Count)
        {
            result.Add(right[rightIndex]);
            rightIndex++;
        }

        return result;
    }
}

