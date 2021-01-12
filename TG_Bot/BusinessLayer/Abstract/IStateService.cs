using System.Threading.Tasks;

namespace TG_Bot.BusinessLayer.Abstract
{
    public interface IStateService
    {
        Task<string> LastState();

        Task<string> Electricity();

        Task<string> Temperature();
        
        Task<string> Heating();

    }
}
