namespace Shipeng.Util.Paged
{
    public class ListResult<T> : IListResult<T>
    {
        IReadOnlyList<T> item;
        public ListResult()
        {

        }

        public IReadOnlyList<T> Item
        {
            get => item ??= new List<T>();
            set => item = value;
        }

        public ListResult(IReadOnlyList<T> item)
        {
            Item = item;
        }
    }
}
