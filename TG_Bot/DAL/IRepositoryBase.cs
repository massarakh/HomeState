using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_Bot.DAL
{
    public interface IRepositoryBase<out T> where T : class
    {
        IQueryable<T> Query();

    }
}
