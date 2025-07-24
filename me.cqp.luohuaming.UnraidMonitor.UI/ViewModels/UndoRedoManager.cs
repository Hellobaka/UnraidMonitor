using me.cqp.luohuaming.UnraidMonitor.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace me.cqp.luohuaming.UnraidMonitor.UI.ViewModels
{
    public class UndoRedoManager(int maxDepth)
    {
        private object _syncLock = new();

        public List<IUndoableCommand> UndoStack { get; set; } = [];

        public List<IUndoableCommand> RedoStack { get; set; } = [];

        public int MaxDepth { get; set; } = maxDepth;

        public bool Processing { get; set; }

        public void Clear()
        {
            UndoStack.Clear();
            RedoStack.Clear();
        }

        public void AddCommand(IUndoableCommand command)
        {
            lock (_syncLock)
            {
                if (Processing)
                {
                    return;
                }
                Processing = true;
                try
                {
                    RedoStack.Clear();

                    if (command is PropertyChangeCommand propertyChangeCommand
                        && UndoStack.LastOrDefault() is PropertyChangeCommand oldCommand
                        && propertyChangeCommand.CheckSameSource(oldCommand))
                    {
                        oldCommand.NewValue = propertyChangeCommand.NewValue;
                        return;
                    }
                    UndoStack.Add(command);
                    if (UndoStack.Count > MaxDepth)
                    {
                        UndoStack.RemoveAt(0);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    Processing = false;
                }
            }
        }

        public void Undo()
        {
            lock (_syncLock)
            {
                if (Processing)
                {
                    return;
                }
                Processing = true;
                try
                {
                    if (UndoStack.Count > 0)
                    {
                        var command = UndoStack.Last();
                        UndoStack.RemoveAt(UndoStack.Count - 1);
                        command.UnExecute();
                        RedoStack.Add(command);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    Processing = false;
                }
            }
        }

        public void Redo()
        {
            lock (_syncLock)
            {
                if (Processing)
                {
                    return;
                }
                try
                {
                    Processing = true;
                    if (RedoStack.Count > 0)
                    {
                        var command = RedoStack.Last();
                        RedoStack.RemoveAt(RedoStack.Count - 1);
                        command.Execute();
                        UndoStack.Add(command);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    Processing = false;
                }
            }
        }
    }
}
