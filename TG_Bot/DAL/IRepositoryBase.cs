using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_Bot.DAL
{
    public interface IRepositoryBase<T> where T : class
    {
        Task<T> GetState();

        IQueryable<T> Query();

    }
}
