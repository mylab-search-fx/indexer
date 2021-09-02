using System.Threading.Tasks;
using MyLab.Search.Indexer.DataContract;

namespace MyLab.Search.Indexer.LogicStrategy
{
    interface ISeedCalc
    {
        Task StartAsync();

        void Update(DataSourceEntity[] entities);

        Task SaveAsync();

        string GetLogValue();
    }
}