namespace Shipeng.HRMS.Service
{
    public interface ICustomerForm
    {
        Task Add(string flowInstanceId, string frmData);
        Task Edit(string flowInstanceId, string frmData);
    }
}
