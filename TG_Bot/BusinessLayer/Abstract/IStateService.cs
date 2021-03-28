using System.Threading.Tasks;
using TG_Bot.Helpers;

namespace TG_Bot.BusinessLayer.Abstract
{
    public interface IStateService
    {
        Task<string> LastState();

        Task<string> Electricity();

        Task<string> Temperature();
        
        Task<string> Heating();

        Task<string> GetStatistics(Additions.StatType type);

    }
}
