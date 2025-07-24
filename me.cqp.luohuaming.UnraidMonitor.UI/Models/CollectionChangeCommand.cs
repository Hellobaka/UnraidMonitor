using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Models
{
    public class CollectionChangeCommand(object target, NotifyCollectionChangedEventArgs e) : IUndoableCommand
    {
        public object Target { get; set; } = target;

        public NotifyCollectionChangedEventArgs EventArgs { get; set; } = e;

        public void Execute()
        {
            if (Target is not System.Collections.IList list)
            {
                throw new InvalidOperationException("Target must be a collection that implements IList.");
            }
            switch (EventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in EventArgs.NewItems)
                    {
                        list.Add(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in EventArgs.OldItems)
                    {
                        list.Remove(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < EventArgs.OldItems.Count; i++)
                    {
                        list[list.IndexOf(EventArgs.OldItems[i])] = EventArgs.NewItems[i];
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (EventArgs.OldStartingIndex < 0 || EventArgs.NewStartingIndex < 0)
                    {
                        throw new InvalidOperationException("Invalid indices for move operation.");
                    }
                    var itemToMove = list[EventArgs.OldStartingIndex];
                    list.RemoveAt(EventArgs.OldStartingIndex);
                    list.Insert(EventArgs.NewStartingIndex, itemToMove);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    list.Clear();
                    break;
                default:
                    break;
            }
        }

        public void UnExecute()
        {
            if (Target is not System.Collections.IList list)
            {
                throw new InvalidOperationException("Target must be a collection that implements IList.");
            }
            switch (EventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in EventArgs.NewItems)
                    {
                        list.Remove(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in EventArgs.OldItems)
                    {
                        list.Add(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < EventArgs.OldItems.Count; i++)
                    {
                        list[list.IndexOf(EventArgs.NewItems[i])] = EventArgs.OldItems[i];
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (EventArgs.OldStartingIndex < 0 || EventArgs.NewStartingIndex < 0)
                    {
                        throw new InvalidOperationException("Invalid indices for move operation.");
                    }
                    var itemToMove = list[EventArgs.NewStartingIndex];
                    list.RemoveAt(EventArgs.NewStartingIndex);
                    list.Insert(EventArgs.OldStartingIndex, itemToMove);

                    break;
                case NotifyCollectionChangedAction.Reset:
                    throw new InvalidOperationException("Clear Operation Cannot be Redo");
                    break;
                default:
                    break;
            }
        }
    }
}
