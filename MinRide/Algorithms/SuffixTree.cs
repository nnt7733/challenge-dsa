namespace MinRide.Algorithms;

/// <summary>
/// Represents a node in the Suffix Tree for efficient substring searching.
/// </summary>
public class SuffixTreeNode
{
    /// <summary>
    /// Dictionary mapping characters to child nodes.
    /// </summary>
    public Dictionary<char, SuffixTreeNode> Children { get; set; }

    /// <summary>
    /// Set of IDs that contain the suffix ending at this node.
    /// </summary>
    public HashSet<int> Ids { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SuffixTreeNode"/> class.
    /// </summary>
    public SuffixTreeNode()
    {
        Children = new Dictionary<char, SuffixTreeNode>();
        Ids = new HashSet<int>();
    }
}

/// <summary>
/// Suffix Tree data structure for efficient substring-based name searches.
/// Supports O(L + M) substring lookup where L is substring length and M is number of matches.
/// </summary>
public class SuffixTree
{
    private SuffixTreeNode root;

    /// <summary>
    /// Initializes a new instance of the <see cref="SuffixTree"/> class.
    /// </summary>
    public SuffixTree()
    {
        root = new SuffixTreeNode();
    }

    /// <summary>
    /// Inserts a name with its associated ID into the Suffix Tree.
    /// Inserts all suffixes of the name for efficient substring search.
    /// Time complexity: O(L²) where L is the length of the name.
    /// </summary>
    /// <param name="name">The name to insert (case-insensitive).</param>
    /// <param name="id">The ID associated with the name.</param>
    public void Insert(string name, int id)
    {
        if (string.IsNullOrEmpty(name))
            return;

        name = name.ToLower();

        // Insert all suffixes of the name
        for (int i = 0; i < name.Length; i++)
        {
            string suffix = name.Substring(i);
            InsertSuffix(suffix, id);
        }
    }

    /// <summary>
    /// Inserts a single suffix into the tree.
    /// </summary>
    private void InsertSuffix(string suffix, int id)
    {
        SuffixTreeNode current = root;

        foreach (char c in suffix)
        {
            if (!current.Children.ContainsKey(c))
            {
                current.Children[c] = new SuffixTreeNode();
            }
            current = current.Children[c];
        }

        // Add ID to this node
        current.Ids.Add(id);
    }

    /// <summary>
    /// Removes a name with its associated ID from the Suffix Tree.
    /// </summary>
    /// <param name="name">The name to remove (case-insensitive).</param>
    /// <param name="id">The ID to remove.</param>
    public void Remove(string name, int id)
    {
        if (string.IsNullOrEmpty(name))
            return;

        name = name.ToLower();

        // Remove from all suffixes
        for (int i = 0; i < name.Length; i++)
        {
            string suffix = name.Substring(i);
            RemoveSuffix(suffix, id);
        }
    }

    /// <summary>
    /// Removes a single suffix from the tree.
    /// </summary>
    private void RemoveSuffix(string suffix, int id)
    {
        SuffixTreeNode current = root;

        foreach (char c in suffix)
        {
            if (!current.Children.ContainsKey(c))
            {
                return; // Suffix not found
            }
            current = current.Children[c];
        }

        current.Ids.Remove(id);
    }

    /// <summary>
    /// Searches for all IDs where the name contains the given substring (case-insensitive).
    /// Time complexity: O(L + M) where L is substring length and M is number of matching IDs.
    /// </summary>
    /// <param name="substring">The substring to search for (case-insensitive).</param>
    /// <returns>A list of IDs that contain the substring.</returns>
    public List<int> SearchBySubstring(string substring)
    {
        if (string.IsNullOrEmpty(substring))
            return new List<int>();

        substring = substring.ToLower();
        SuffixTreeNode current = root;

        // Navigate to the end of the substring
        foreach (char c in substring)
        {
            if (!current.Children.ContainsKey(c))
            {
                return new List<int>(); // Substring not found
            }
            current = current.Children[c];
        }

        // Collect all IDs from this node and all descendants
        return CollectAllIds(current);
    }

    /// <summary>
    /// Collects all IDs in the subtree rooted at the given node using BFS.
    /// </summary>
    private List<int> CollectAllIds(SuffixTreeNode node)
    {
        List<int> result = new List<int>();
        Queue<SuffixTreeNode> queue = new Queue<SuffixTreeNode>();
        queue.Enqueue(node);

        while (queue.Count > 0)
        {
            SuffixTreeNode current = queue.Dequeue();
            result.AddRange(current.Ids);

            foreach (var child in current.Children.Values)
            {
                queue.Enqueue(child);
            }
        }

        return result;
    }

    /// <summary>
    /// Clears all entries from the Suffix Tree.
    /// </summary>
    public void Clear()
    {
        root = new SuffixTreeNode();
    }
}
