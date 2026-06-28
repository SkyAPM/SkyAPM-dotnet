using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using Xunit;

namespace SkyApm.Core.Tests
{
    public class SegmentContextAccessorTests
    {
        private static SegmentContext NewContext(string id, SpanType spanType) =>
            new SegmentContext($"trace-{id}", $"segment-{id}", true, "service", "instance", $"op-{id}", spanType);

        private static (SegmentContextAccessor accessor,
            IEntrySegmentContextAccessor entry,
            ILocalSegmentContextAccessor local,
            IExitSegmentContextAccessor exit) NewAccessor()
        {
            var entry = new EntrySegmentContextAccessor();
            var local = new LocalSegmentContextAccessor();
            var exit = new ExitSegmentContextAccessor();
            return (new SegmentContextAccessor(entry, local, exit), entry, local, exit);
        }

        [Fact]
        public void Context_Is_Null_When_Nothing_Set()
        {
            var (accessor, _, _, _) = NewAccessor();
            Assert.Null(accessor.Context);
        }

        // Regression for #552: the exit accessor was injected but never read
        // (its slot held a duplicate of the entry accessor), so an exit-only
        // context resolved to null instead of the exit segment.
        [Fact]
        public void Context_Returns_Exit_When_Only_Exit_Set()
        {
            var (accessor, _, _, exit) = NewAccessor();
            var exitContext = NewContext("exit", SpanType.Exit);
            exit.Context = exitContext;
            Assert.Same(exitContext, accessor.Context);
        }

        [Fact]
        public void Context_Returns_Entry_When_Only_Entry_Set()
        {
            var (accessor, entry, _, _) = NewAccessor();
            var entryContext = NewContext("entry", SpanType.Entry);
            entry.Context = entryContext;
            Assert.Same(entryContext, accessor.Context);
        }

        [Fact]
        public void Context_Prefers_Local_Then_Entry_Then_Exit()
        {
            var (accessor, entry, local, exit) = NewAccessor();
            var entryContext = NewContext("entry", SpanType.Entry);
            var localContext = NewContext("local", SpanType.Local);
            var exitContext = NewContext("exit", SpanType.Exit);

            // entry + exit set, no local -> entry wins over exit.
            entry.Context = entryContext;
            exit.Context = exitContext;
            Assert.Same(entryContext, accessor.Context);

            // local set -> local wins over entry and exit.
            local.Context = localContext;
            Assert.Same(localContext, accessor.Context);
        }
    }
}
