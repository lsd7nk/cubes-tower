using Cysharp.Threading.Tasks;

namespace _App
{
    public interface IDataHandler
    {
        UniTask LoadData();
        void SaveData();
    }
}