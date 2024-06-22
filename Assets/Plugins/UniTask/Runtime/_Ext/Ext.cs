namespace Cysharp.Threading.Tasks
{
    public partial struct UniTask
    {
        public static UniTask<int> WhenAny(UniTask[] tasks, int length)
        {
            return new UniTask<int>(new WhenAnyPromise(tasks, length), 0);
        }
    }
}