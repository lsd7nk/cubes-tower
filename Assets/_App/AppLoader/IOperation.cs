using Cysharp.Threading.Tasks;

namespace _App
{
    public interface IOperation 
    {
        public UniTask OperationInit();
    }
}