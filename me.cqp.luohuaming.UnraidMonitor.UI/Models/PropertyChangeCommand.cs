using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Models
{
    public class PropertyChangeCommand(object target, PropertyInfo propertyInfo, object oldValue, object newValue) : IUndoableCommand
    {
        public object Target { get; set; } = target;

        public PropertyInfo PropertyInfo { get; set; } = propertyInfo;

        public object OldValue { get; set; } = oldValue;

        public object NewValue { get; set; } = newValue;

        public void Execute()
        {
            SetPropertyValue(Target, PropertyInfo, NewValue);
        }

        public void UnExecute()
        {
            SetPropertyValue(Target, PropertyInfo, OldValue);
        }

        private void SetPropertyValue(object target, PropertyInfo propertyInfo, object value)
        {
            propertyInfo.SetValue(target, value);
        }

        public bool CheckSameSource(IUndoableCommand command)
        {
            return command is PropertyChangeCommand propertyChangeCommand 
                && propertyChangeCommand.PropertyInfo.Name == PropertyInfo.Name 
                && propertyChangeCommand.Target == Target;
        }
    }
}
