namespace Shipeng.Util
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public class ServiceDescriptionAttribute : Attribute
	{

		public string ClassDescription
		{
			get;
			set;
		}

		private ServiceDescriptionAttribute()
		{
		}

		public ServiceDescriptionAttribute(string classDescription)
		{
			ClassDescription = classDescription;
		}
	}
}
