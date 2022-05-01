using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SkyApm.Utilities.StaticAccessor;

namespace System.Threading
{
    internal class SkyApmThreadPoolWorkItem : IThreadPoolWorkItem
    {
        private readonly string _operationName;
        private readonly IThreadPoolWorkItem _item;
        private readonly CrossThreadCarrier _carrier;

        public SkyApmThreadPoolWorkItem(IThreadPoolWorkItem item)
        {
            _operationName = item.GetType().FullName;
            var prepare = SkyApmInstances.TracingContext.CreateLocal(_operationName);
            _carrier = prepare.GetCrossThreadCarrier();

            _item = item;

            SkyApmInstances.TracingContext.Finish(prepare);
        }

        public void Execute()
        {
            var local = SkyApmInstances.TracingContext.CreateLocal("[exec]" + _operationName, _carrier);
            try
            {
                _item.Execute();
            }
            catch (Exception ex)
            {
                local.Span.ErrorOccurred(ex, SkyApmInstances.TracingConfig);
                throw ex;
            }
            finally
            {
                SkyApmInstances.TracingContext.Finish(local);
            }
        }
    }
}
