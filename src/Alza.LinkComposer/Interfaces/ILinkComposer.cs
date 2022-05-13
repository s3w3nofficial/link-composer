using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Alza.LinkComposer.Interfaces
{
    public interface ILinkComposer
    {
        Uri Link<T>(Expression<Action<T>> method) where T : LinkComposerController;

        Uri Link<T>(Expression<Action<T>> method, object additionalQueryParams) where T : LinkComposerController;
    
        Uri Link<T>(Expression<Func<T, Task>> method) where T : LinkComposerController;

        Uri Link<T>(Expression<Func<T, Task>> method, object additionalQueryParams) where T : LinkComposerController;
    }
}
