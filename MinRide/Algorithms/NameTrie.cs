namespace MinRide.Algorithms;

/// <summary>
/// Represents a node in the Trie (prefix tree) for efficient name searching.
/// </summary>
public class TrieNode
{
    /// <summary>
    /// Dictionary mapping characters to child nodes.
    /// </summary>
    public Dictionary<char, TrieNode> Children { get; set; }

    /// <summary>
    /// List of IDs stored at this node (for names that end here).
    /// </summary>
    public List<int> Ids { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrieNode"/> class.
    /// </summary>
    public TrieNode()
    {
        Children = new Dictionary<char, TrieNode>();
        Ids = new List<int>();
    }
}

/// <summary>
/// Trie (prefix tree) data structure for efficient prefix-based name searches.
/// Supports O(L) lookup where L is the length of the search prefix.
/// </summary>
public class NameTrie
{
    private TrieNode root;

    /// <summary>
    /// Initializes a new instance of the <see cref="NameTrie"/> class.
    /// </summary>
    public NameTrie()
    {
        root = new TrieNode();
    }

    /// <summary>
    /// Inserts a name with its associated ID into the Trie.
    /// </summary>
    /// <param name="name">The name to insert (case-insensitive).</param>
    /// <param name="id">The ID associated with the name.</param>
    public void Insert(string name, int id)
    {
        if (string.IsNullOrEmpty(name))
            return;

        // Convert to lowercase for case-insensitive search
        name = name.ToLower();
        TrieNode current = root;

        foreach (char c in name)
        {
            if (!current.Children.ContainsKey(c))
            {
                current.Children[c] = new TrieNode();
            }
            current = current.Children[c];
        }

        // Store ID at the end of the name
        if (!current.Ids.Contains(id))
        {
            current.Ids.Add(id);
        }
    }

    /// <summary>
    /// Removes a name with its associated ID from the Trie.
    /// </summary>
    /// <param name="name">The name to remove (case-insensitive).</param>
    /// <param name="id">The ID to remove.</param>
    /// <returns>true if the ID was found and removed; otherwise false.</returns>
    public bool Remove(string name, int id)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        name = name.ToLower();
        TrieNode current = root;

        foreach (char c in name)
        {
            if (!current.Children.ContainsKey(c))
            {
                return false;
            }
            current = current.Children[c];
        }

        return current.Ids.Remove(id);
    }

    /// <summary>
    /// Searches for all IDs that match the given prefix (case-insensitive).
    /// Time complexity: O(L + M) where L is prefix length and M is number of matching IDs.
    /// </summary>
    /// <param name="prefix">The prefix to search for (case-insensitive).</param>
    /// <returns>A list of IDs that match the prefix.</returns>
    public List<int> SearchByPrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return new List<int>();

        prefix = prefix.ToLower();
        TrieNode current = root;

        // Navigate to the end of the prefix
        foreach (char c in prefix)
        {
            if (!current.Children.ContainsKey(c))
            {
                return new List<int>();
            }
            current = current.Children[c];
        }

        // Collect all IDs from this node and all descendants
        return CollectAllIds(current);
    }

    /// <summary>
    /// Collects all IDs in the subtree rooted at the given node.
    /// </summary>
    private List<int> CollectAllIds(TrieNode node)
    {
        List<int> result = new List<int>();
        Queue<TrieNode> queue = new Queue<TrieNode>();
        queue.Enqueue(node);

        while (queue.Count > 0)
        {
            TrieNode current = queue.Dequeue();
            result.AddRange(current.Ids);

            foreach (var child in current.Children.Values)
            {
                queue.Enqueue(child);
            }
        }

        return result;
    }

    /// <summary>
    /// Searches for all IDs where the name contains the given substring (case-insensitive).
    /// </summary>
    /// <param name="substring">The substring to search for (case-insensitive).</param>
    /// <returns>A list of IDs that contain the substring.</returns>
    public List<int> SearchBySubstring(string substring)
    {
        if (string.IsNullOrEmpty(substring))
            return new List<int>();

        // For substring search, we need to collect all IDs and filter
        // This is less efficient than prefix search but still better than linear scan for large datasets
        substring = substring.ToLower();
        List<int> result = new List<int>();
        
        // We'll need to traverse all paths - use DFS to collect all nodes
        CollectMatchingIds(root, "", substring, result);
        
        return result;
    }

    /// <summary>
    /// Recursively collects IDs where the name contains the given substring.
    /// </summary>
    private void CollectMatchingIds(TrieNode node, string currentPath, string substring, List<int> result)
    {
        // Check if current path contains the substring
        if (currentPath.Contains(substring))
        {
            result.AddRange(node.Ids);
            // No need to search children since substring was found
            return;
        }

        // Continue searching in children
        foreach (var kvp in node.Children)
        {
            string newPath = currentPath + kvp.Key;
            CollectMatchingIds(kvp.Value, newPath, substring, result);
        }
    }

    /// <summary>
    /// Clears all entries from the Trie.
    /// </summary>
    public void Clear()
    {
        root = new TrieNode();
    }
}
