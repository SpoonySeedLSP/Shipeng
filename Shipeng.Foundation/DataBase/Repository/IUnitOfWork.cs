using SqlSugar;

namespace Shipeng.Util.DataBase
{
    public interface IUnitOfWork
    {
        SqlSugarClient GetDbClient();
        void BeginTrans();

        void Commit();
        void Rollback();
        void CurrentBeginTrans();

        void CurrentCommit();
        void CurrentRollback();
    }
}