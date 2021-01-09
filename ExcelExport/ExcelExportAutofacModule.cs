using Autofac;

namespace J4JSoftware.Excel
{
    public class ExcelExportAutofacModule : Module
    {
        protected override void Load( ContainerBuilder builder )
        {
            base.Load( builder );

            builder.RegisterType<ExcelWorkbook>()
                .AsSelf();

            builder.RegisterType<ExcelSheet>()
                .AsSelf();

            builder.RegisterType<ExcelTable>()
                .AsSelf();
        }
    }
}
