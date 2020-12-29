using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TG_Bot.monitoring;

namespace TG_Bot.DAL
{
    class StateRepository : IStateRepository
    {
        private readonly _4stasContext _context;

        /// <inheritdoc />
        public StateRepository(_4stasContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<Monitor> GetState()
        {
            return await _context.Monitor
                .OrderByDescending(d => d.Timestamp)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public IQueryable<Monitor> Query()
        {
            return _context.Monitor.AsQueryable();
        }

        /// <inheritdoc />
        public async Task<string> GetElectricity()
        {
            var res = await GetState();
            return $"";//TODO надо разобраться где возвращать
        }
    }
}
