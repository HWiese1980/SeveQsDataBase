using System.Threading.Tasks;

namespace SeveQsDataBase
{
    public interface IOnPropertyChanged
    {
#if USE_SEVEQ_DB
        Task<string> OnPropertyChanged(string property = "", string by = "", int recursionLevel = 0);
#else
        void OnPropertyChanged(string property = "", string by = "");
#endif
    }
}