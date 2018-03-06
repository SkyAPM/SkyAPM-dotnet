using System;
using System.Collections.Generic;
using System.Text;
using SkyWalking.Context.Trace;

namespace SkyWalking.Context.Tag
{
    public abstract class AbstractTag<T>
    {

        public AbstractTag(string tagKey)
        {
            Key = tagKey;
        }

        protected abstract void Set(ISpan span, T tagValue);

        /**
         * @return the key of this tag.
         */
        public string Key { get; protected set; }
    }
}