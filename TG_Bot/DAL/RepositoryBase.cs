using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TG_Bot.monitoring;

namespace TG_Bot.DAL
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        private readonly _4stasContext _context;

        public RepositoryBase(_4stasContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<T> GetState()
        {
            return await _context.Set<T>().LastAsync();
        }

        /// <inheritdoc />
        public IQueryable<T> Query()
        {
            return _context.Set<T>();
        }
    }
}
