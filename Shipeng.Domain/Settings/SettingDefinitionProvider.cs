using Volo.Abp.Settings;

namespace Shipeng.Domain.Settings
{
    public class SettingDefinitionProvider : Volo.Abp.Settings.SettingDefinitionProvider
    {
        public override void Define(ISettingDefinitionContext context)
        {
            //在这里定义您自己的设置。例子:
            //context.Add(new SettingDefinition(Settings.MySetting1));
        }
    }
}
