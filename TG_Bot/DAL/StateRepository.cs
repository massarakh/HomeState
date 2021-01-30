using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TG_Bot.BusinessLayer;
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
        public async Task<Data> GetState()
        {
            var state = await _context.Monitor
                .OrderByDescending(d => d.Timestamp)
                .FirstOrDefaultAsync();

            return new Data
            {
                Boiler = Convert.ToInt32(state.Boiler) == 10,
                BoilerHeat = state.BoilerHeating,
                Electricity = new Electricity
                {
                    Phase1 = state.Phase1,
                    Phase2 = state.Phase2,
                    Phase3 = state.Phase3,
                    PhaseSumm = state.PhaseSumm
                },
                Energy = state.Energy.ToString(),
                Heat = new Heat
                {
                    Batteries = Convert.ToInt32(state.Heat) == 6 || Convert.ToInt32(state.Heat) == 9,
                    Floor = Convert.ToInt32(state.Heat) == 3 || Convert.ToInt32(state.Heat) == 9
                },
                Humidity = new Humidity
                {
                    Bedroom = state.HumidityBedroom,
                    LivingRoom = state.HumidityLivingRoom
                },
                Temperature = new Temperature
                {
                    Barn = state.TemperatureBarn,
                    Bedroom = state.TemperatureBedroom,
                    LivingRoom = state.TemperatureLivingRoom,
                    Outside = state.TemperatureOutside
                },
                Timestamp = state.Timestamp?.ToString("H':'mm d'.'MM'.'yy")
            };
        }

        /// <inheritdoc />
        public IQueryable<Monitor> Query()
        {
            return _context.Monitor.AsQueryable();
        }

        /// <inheritdoc />
        public async Task<Electricity> GetElectricity()
        {
            var state = await GetState();
            return state.Electricity;
        }

        /// <inheritdoc />
        public async Task<Heat> GetHeating()
        {
            var state = await GetState();
            return state.Heat;
        }

        /// <inheritdoc />
        public async Task<Temperature> GetTemperatures()
        {
            var state = await GetState();
            return state.Temperature;
        }

        /// <inheritdoc />
        public async Task<Humidity> GetHumidity()
        {
            var state = await GetState();
            return state.Humidity;
        }
    }
}
