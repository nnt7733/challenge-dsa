namespace MinRide.Utils;

/// <summary>
/// Provides an undo stack for storing and executing reversible actions.
/// </summary>
public class UndoStack
{
    /// <summary>
    /// The stack of undo actions.
    /// </summary>
    private Stack<Action> undoActions;

    /// <summary>
    /// The maximum number of undo actions to store.
    /// </summary>
    private const int MaxSize = 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="UndoStack"/> class.
    /// </summary>
    public UndoStack()
    {
        undoActions = new Stack<Action>();
    }

    /// <summary>
    /// Pushes an undo action onto the stack.
    /// </summary>
    /// <param name="undoAction">The action to execute when undoing.</param>
    /// <remarks>
    /// If the stack exceeds <see cref="MaxSize"/>, the oldest action is removed.
    /// </remarks>
    public void Push(Action undoAction)
    {
        undoActions.Push(undoAction);

        // If stack exceeds MaxSize, remove bottom element
        if (undoActions.Count > MaxSize)
        {
            // Convert to array, remove first (bottom) element, recreate stack
            Action[] actions = undoActions.ToArray();
            undoActions.Clear();

            // Push back in reverse order (excluding the last/oldest element)
            for (int i = actions.Length - 2; i >= 0; i--)
            {
                undoActions.Push(actions[i]);
            }
        }
    }

    /// <summary>
    /// Undoes the last action by popping and executing it from the stack.
    /// </summary>
    public void Undo()
    {
        if (undoActions.Count == 0)
        {
            Console.WriteLine("Nothing to undo.");
            return;
        }

        Action action = undoActions.Pop();
        action.Invoke();
        Console.WriteLine("Undo operation completed successfully.");
    }

    /// <summary>
    /// Checks if there are any actions available to undo.
    /// </summary>
    /// <returns><c>true</c> if there are actions to undo; otherwise, <c>false</c>.</returns>
    public bool CanUndo()
    {
        return undoActions.Count > 0;
    }

    /// <summary>
    /// Clears all actions from the undo stack.
    /// </summary>
    public void Clear()
    {
        undoActions.Clear();
    }
}

