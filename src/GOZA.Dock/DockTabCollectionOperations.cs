using System.Collections;

namespace GOZA.Dock;

/// <summary>Collection mutations shared by drag gestures, with rollback on rejected changes.</summary>
internal static class DockTabCollectionOperations
{
    internal static bool CanMutate(IList list) => !list.IsReadOnly && !list.IsFixedSize;

    internal static bool TryMove(IList source, IList target, object item, int insertIndex)
    {
        var sourceIndex = IndexOfReference(source, item);
        if (ReferenceEquals(source, target) || !CanMutate(source) || !CanMutate(target)
            || sourceIndex < 0 || IndexOfReference(target, item) >= 0)
            return false;

        try
        {
            // A typed target can reject this item. Do not remove its original owner first.
            target.Insert(Math.Clamp(insertIndex, 0, target.Count), item);
            source.RemoveAt(sourceIndex);
            return true;
        }
        catch (Exception exception) when (IsRejectedMutation(exception))
        {
            // CollectionChanged handlers may throw after the underlying mutation happened.
            if (IndexOfReference(source, item) < 0)
                source.Insert(Math.Clamp(sourceIndex, 0, source.Count), item);
            var targetIndex = IndexOfReference(target, item);
            if (targetIndex >= 0)
                target.RemoveAt(targetIndex);
            return false;
        }
    }

    internal static bool TryReorder(IList list, object item, int index)
    {
        var oldIndex = IndexOfReference(list, item);
        if (!CanMutate(list) || oldIndex < 0)
            return false;
        index = Math.Clamp(index, 0, list.Count - 1);
        if (index == oldIndex)
            return true;
        try
        {
            list.RemoveAt(oldIndex);
            list.Insert(index, item);
            return true;
        }
        catch (Exception exception) when (IsRejectedMutation(exception))
        {
            var currentIndex = IndexOfReference(list, item);
            if (currentIndex >= 0 && currentIndex != oldIndex)
                list.RemoveAt(currentIndex);
            if (IndexOfReference(list, item) < 0)
                list.Insert(Math.Clamp(oldIndex, 0, list.Count), item);
            return false;
        }
    }

    internal static int IndexOfReference(IList list, object item)
    {
        for (var i = 0; i < list.Count; i++)
            if (ReferenceEquals(list[i], item))
                return i;
        return -1;
    }

    private static bool IsRejectedMutation(Exception exception) =>
        exception is ArgumentException or InvalidOperationException or NotSupportedException;
}
