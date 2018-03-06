namespace SkyWalking.Context.Tag
{
    /// <summary>
    /// The span tags are supported by sky-walking engine.
    /// As default, all tags will be stored, but these ones have particular meanings.
    /// </summary>
    public static class Tags
    {
        public static StringTag URL = new StringTag("url");

        /**
         * STATUS_CODE records the http status code of the response.
         */
        public static StringTag STATUS_CODE = new StringTag("status_code");

        /**
         * DB_TYPE records database type, such as sql, redis, cassandra and so on.
         */
        public static StringTag DB_TYPE = new StringTag("db.type");

        /**
         * DB_INSTANCE records database instance name.
         */
        public static StringTag DB_INSTANCE = new StringTag("db.instance");

        /**
         * DB_STATEMENT records the sql statement of the database access.
         */
        public static StringTag DB_STATEMENT = new StringTag("db.statement");

        /**
         * DB_BIND_VARIABLES records the bind variables of sql statement.
         */
        public static StringTag DB_BIND_VARIABLES = new StringTag("db.bind_vars");

        /**
         * MQ_BROKER records the broker address of message-middleware
         */
        public static StringTag MQ_BROKER = new StringTag("mq.broker");

        /**
         * MQ_TOPIC records the topic name of message-middleware
         */
        public static StringTag MQ_TOPIC = new StringTag("mq.topic");

        public static class HTTP
        {
            public static readonly StringTag METHOD = new StringTag("http.method");
        }
    }
}