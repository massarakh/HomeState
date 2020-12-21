using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TG_Bot.DAL;
using TG_Bot.monitoring;

namespace TG_Bot.BusinessLayer
{
    public class StateService : IStateService
    {
        private readonly IStateRepository _repository;

        public StateService(IStateRepository repository)
        {
            _repository = repository;
        }
        /// <inheritdoc />
        public async Task<Monitor> LastState()
        {
            return await _repository.GetState();
        }
    }
}
