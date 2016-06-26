﻿using System.Collections;
using System.Collections.Generic;
using Simple1C.Interface.ObjectModel;

namespace Simple1C.Impl
{
    internal class SyncList
    {
        public readonly List<Command> commands = new List<Command>();

        public void Compare(IList original, IList current)
        {
            if (original != null)
                for (var i = original.Count - 1; i >= 0; i--)
                {
                    var item = original[i];
                    if (current.IndexOf(item) < 0)
                    {
                        commands.Add(new DeleteCommand {index = i});
                        original.RemoveAt(i);
                    }
                }
            else
                original = new List<object>();
            for (var i = 0; i < current.Count; i++)
            {
                var item = (Abstract1CEntity) current[i];
                var originalIndex = original.IndexOf(item);
                if (originalIndex < 0)
                {
                    commands.Add(new InsertCommand {item = item, index = i});
                    original.Insert(i, null);
                }
                else
                {
                    if (originalIndex != i)
                    {
                        commands.Add(new MoveCommand {from = originalIndex, delta = i - originalIndex});
                        original.RemoveAt(originalIndex);
                        original.Insert(i, null);
                    }
                    if (item.Controller.Changed != null)
                        commands.Add(new UpdateCommand {index = i, item = item});
                }
            }
        }

        public enum CommandType
        {
            Delete,
            Insert,
            Move,
            Update
        }

        public abstract class Command
        {
            protected Command(CommandType commandType)
            {
                CommandType = commandType;
            }

            public CommandType CommandType { get; private set; }
        }

        public class DeleteCommand : Command
        {
            public DeleteCommand() : base(CommandType.Delete)
            {
            }

            public int index;
        }

        public class InsertCommand : Command
        {
            public InsertCommand() : base(CommandType.Insert)
            {
            }

            public int index;
            public Abstract1CEntity item;
        }

        
        public class MoveCommand : Command
        {
            public MoveCommand() : base(CommandType.Move)
            {
            }

            public int from;
            public int delta;
        }

        
        public class UpdateCommand : Command
        {
            public UpdateCommand() : base(CommandType.Update)
            {
            }

            public int index;
            public Abstract1CEntity item;
        }
    }
}