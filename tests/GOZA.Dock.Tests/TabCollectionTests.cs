using System.Collections;
using System.Collections.ObjectModel;
using Xunit;

namespace GOZA.Dock.Tests;

public class TabCollectionTests
{
    [Fact]
    public void FailureAfterSourceRemovalRestoresBothCollections()
    {
        var item = new object();
        var neighbor = new object();
        IList source = new ThrowAfterRemoveList { neighbor, item };
        IList target = new ArrayList();
        Assert.False(DockTabCollectionOperations.TryMove(source, target, item, 0));
        Assert.Same(neighbor, source[0]);
        Assert.Same(item, source[1]);
        Assert.Empty(target);
    }

    [Fact]
    public void SuccessfulMovePreservesIdentityAndUsesRequestedIndex()
    {
        var item = new object();
        var neighbor = new object();
        IList source = new ObservableCollection<object> { item };
        IList target = new ObservableCollection<object> { neighbor };
        Assert.True(DockTabCollectionOperations.TryMove(source, target, item, 0));
        Assert.Empty(source);
        Assert.Same(item, target[0]);
        Assert.Same(neighbor, target[1]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ImmutableTargetDoesNotRemoveSource(bool readOnly)
    {
        var item = new object();
        IList source = new ArrayList { item };
        IList target = readOnly ? ArrayList.ReadOnly(new ArrayList()) : Array.Empty<object>();
        Assert.False(DockTabCollectionOperations.TryMove(source, target, item, 0));
        Assert.Same(item, Assert.Single(source.Cast<object>()));
        Assert.Empty(target);
    }

    [Fact]
    public void TypedTargetRejectionPreservesSource()
    {
        var item = new object();
        IList source = new ArrayList { item };
        IList target = new ObservableCollection<string>();
        Assert.False(DockTabCollectionOperations.TryMove(source, target, item, 0));
        Assert.Same(item, source[0]);
        Assert.Empty(target);
    }

    [Fact]
    public void FailedSourceRemovalRollsBackTargetInsertion()
    {
        var item = new object();
        IList source = new RejectingRemovalList { item };
        IList target = new ArrayList();
        Assert.False(DockTabCollectionOperations.TryMove(source, target, item, 0));
        Assert.Same(item, source[0]);
        Assert.Empty(target);
    }

    [Fact]
    public void ExceptionAfterTargetInsertionIsRolledBack()
    {
        var item = new object();
        IList source = new ArrayList { item };
        IList target = new ThrowAfterInsertList();
        Assert.False(DockTabCollectionOperations.TryMove(source, target, item, 0));
        Assert.Same(item, source[0]);
        Assert.Empty(target);
    }

    [Fact]
    public void AlreadyOwnedItemIsNotRemovedFromItsSource()
    {
        var item = new object();
        IList source = new ArrayList { item };
        IList target = new ArrayList { item };
        Assert.False(DockTabCollectionOperations.TryMove(source, target, item, 0));
        Assert.Single(source);
        Assert.Single(target);
    }

    [Fact]
    public void ReorderRollsBackWhenInsertionIsRejected()
    {
        var item = new object();
        var neighbor = new object();
        var list = new RejectFirstInsertList { item, neighbor };
        Assert.False(DockTabCollectionOperations.TryReorder(list, item, 1));
        Assert.Same(item, list[0]);
        Assert.Same(neighbor, list[1]);
    }

    private sealed class RejectingRemovalList : ArrayList
    {
        public override void RemoveAt(int index) => throw new InvalidOperationException("Removal rejected");
    }

    private sealed class ThrowAfterRemoveList : ArrayList
    {
        public override void RemoveAt(int index)
        {
            base.RemoveAt(index);
            throw new InvalidOperationException("Notification failed");
        }
    }

    private sealed class ThrowAfterInsertList : ArrayList
    {
        public override void Insert(int index, object? value)
        {
            base.Insert(index, value);
            throw new InvalidOperationException("Notification failed");
        }
    }

    private sealed class RejectFirstInsertList : ArrayList
    {
        private bool _reject = true;
        public override void Insert(int index, object? value)
        {
            if (_reject)
            {
                _reject = false;
                throw new InvalidOperationException("Insertion rejected");
            }
            base.Insert(index, value);
        }
    }
}
