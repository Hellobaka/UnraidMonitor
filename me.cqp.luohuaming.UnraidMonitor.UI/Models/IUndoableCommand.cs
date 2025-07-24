using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Models
{
    public interface IUndoableCommand
    {
        void Execute();
        
        void UnExecute();
    }
}
