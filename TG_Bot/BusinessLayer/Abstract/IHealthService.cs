namespace TG_Bot.BusinessLayer.Abstract
{
    public interface IHealthService
    {
        bool Check();

        bool Reconfigure(string args);
    }
}