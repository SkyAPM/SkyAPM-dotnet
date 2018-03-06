using SkyWalking.Context.Trace;

namespace SkyWalking.Context.Tag
{
    public class StringTag : AbstractTag<string>
    {
        public StringTag(string tagKey) : base(tagKey)
        {
        }

        protected override void Set(ISpan span, string tagValue)
        {
            span.SetTag(Key, tagValue);
        }
    }
}
